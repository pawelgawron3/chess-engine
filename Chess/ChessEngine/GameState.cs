using ChessEngine.Components;
using static ChessEngine.LegalMoveGenerator;

namespace ChessEngine;

/// <summary>
/// Represents the current state of a chess game, including the board, active player,
/// move history, and game logic.
/// </summary>
public class GameState
{
    public Board Board { get; }

    public Player CurrentPlayer { get; private set; } = Player.White;

    public Position? SelectedPosition { get; private set; }
    public Result? GameResult { get; private set; }

    private readonly RuleManager _ruleManager;
    private readonly MoveHistoryManager _historyManager;
    private readonly ZobristHasher _hasher;
    private readonly GameResultEvaluator _resultEvaluator;

    private int _halfMoveClock = 0;
    private int _fullMoveNumber = 1;

    public event Action<MoveRecord>? MoveMade;

    public GameState()
    {
        Board = new Board();
        Board.Initialize();

        var castlingRights = new Dictionary<Player, (bool, bool, bool)>
        {
            {Player.White, (false, false, false) },
            {Player.Black, (false, false, false) },
        };

        _ruleManager = new RuleManager(castlingRights);
        _historyManager = new MoveHistoryManager();
        _hasher = new ZobristHasher(Board, () => CurrentPlayer, () => castlingRights, () => _ruleManager.EnPassantFile);
        _resultEvaluator = new GameResultEvaluator(this);
    }

    /// <summary>
    /// Returns all legal moves for the currently selected piece.
    /// </summary>
    public IEnumerable<Move> GetLegalMovesForPiece() =>
        (SelectedPosition == null) ? Enumerable.Empty<Move>() : GenerateLegalMovesForPiece(this);

    /// <summary>
    /// Returns all legal moves for the current player.
    /// </summary>
    public IEnumerable<Move> GetLegalMoves() => GenerateLegalMoves(this);

    /// <summary>
    /// Selects a position on the board.
    /// </summary>
    public void SelectPosition(Position pos)
    {
        SelectedPosition = pos;
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        SelectedPosition = null;
    }

    /// <summary>
    /// Attempts to execute a move.
    /// </summary>
    public bool TryMakeMove(Move move)
    {
        if (GameResult != null || !IsMoveLegal(this, move))
            return false;

        Piece movedPiece = Board[move.From]!;
        Piece? capturedPiece = AttackUtils.GetCapturedPiece(Board, move);

        _ruleManager.UpdateCastlingRights(movedPiece, move);
        Board.MakeMove(move);

        int? prevEnPassant = _ruleManager.EnPassantFile;
        _ruleManager.UpdateEnPassantFile(move, movedPiece);
        int? newEnPassant = _ruleManager.EnPassantFile;
        _hasher.ApplyMove(move, movedPiece, capturedPiece, prevEnPassant, newEnPassant);

        bool kingInCheck = AttackUtils.IsKingInCheck(Board, CurrentPlayer.Opponent());
        MoveRecord record = new MoveRecord(move, movedPiece, capturedPiece, _halfMoveClock, move.PromotionPiece, kingInCheck);

        _historyManager.Add(record);
        MoveMade?.Invoke(record);

        UpdateClocks(movedPiece, capturedPiece);
        SwitchPlayer();

        ClearSelection();
        GameResult = _resultEvaluator.Evaluate(_hasher.CurrentHash, _hasher.PositionCounts, _halfMoveClock);

        return true;
    }

    private void SwitchPlayer() => CurrentPlayer = CurrentPlayer.Opponent();

    /// <summary>
    /// Updates half-move number and full-move number.
    /// </summary>
    private void UpdateClocks(Piece movedPiece, Piece? capturedPiece)
    {
        _halfMoveClock = (movedPiece.Type == PieceType.Pawn || capturedPiece != null)
            ? 0
            : _halfMoveClock + 1;

        if (CurrentPlayer == Player.Black)
            _fullMoveNumber++;
    }

    /// <summary>
    /// Attempts to undo the last move.
    /// </summary>
    public void TryUndoMove()
    {
        MoveRecord? last = _historyManager.Undo(Board);
        if (last == null) return;

        _halfMoveClock = last.HalfMoveClockBefore;
        SwitchPlayer();
        GameResult = null;
    }
}