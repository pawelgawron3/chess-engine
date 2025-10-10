using static ChessEngine.LegalMoveGenerator;

namespace ChessEngine;

/// <summary>
/// Represents the current state of a chess game, including the board, active player,
/// selected square, move history, and game progression logic.
/// </summary>
public class GameState
{
    public Board Board { get; }

    public Player CurrentPlayer { get; private set; } = Player.White;

    public Position? SelectedPosition { get; private set; }
    public List<MoveRecord> MoveHistory { get; } = new List<MoveRecord>();
    public Result? GameResult { get; private set; } = null;
    public int FullMoveCounter => _fullMoveNumber;
    private int _fullMoveNumber = 1;
    private int _halfMoveClock = 0;

    public GameState(Board board)
    {
        Board = board;
    }

    public GameState()
    {
        Board = new Board();
        Board.Initialize();
    }

    /// <summary>
    /// Returns all legal moves for the currently selected piece.
    /// </summary>
    public IEnumerable<Move> GetLegalMovesForPiece()
    {
        if (SelectedPosition == null)
            return Enumerable.Empty<Move>();

        return GenerateLegalMovesForPiece(Board, SelectedPosition.Value, CurrentPlayer);
    }

    /// <summary>
    /// Returns all legal moves for the current player.
    /// </summary>
    public IEnumerable<Move> GetLegalMoves()
    {
        return GenerateLegalMoves(Board, CurrentPlayer);
    }

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
        if (GameResult != null)
            return false;

        if (!IsMoveLegal(Board, move, CurrentPlayer))
            return false;

        Piece movedPiece = Board[move.From]!;
        Piece? capturedPiece = Board[move.To];

        Board.MakeMove(move);
        MoveHistory.Add(new MoveRecord(move, movedPiece, capturedPiece, _halfMoveClock));

        if (movedPiece.Type == PieceType.Pawn || capturedPiece != null)
            _halfMoveClock = 0;
        else _halfMoveClock++;

        if (CurrentPlayer == Player.Black)
            _fullMoveNumber++;

        ClearSelection();
        CurrentPlayer = CurrentPlayer.Opponent();
        CheckForGameOver();

        return true;
    }

    /// <summary>
    /// Checks game ending conditions.
    /// </summary>
    private void CheckForGameOver()
    {
        if (_halfMoveClock >= 100)
        {
            GameResult = Result.Draw(GameEndReason.FiftyMovesRule);
            return;
        }

        if (!GetLegalMoves().Any())
        {
            bool kingInCheck = AttackUtils.IsKingInCheck(Board, CurrentPlayer);

            if (kingInCheck)
            {
                GameResult = Result.Win(CurrentPlayer.Opponent());
            }
            else
            {
                GameResult = Result.Draw(GameEndReason.Stalemate);
            }
        }
    }

    public void UndoLastMove()
    {
        if (MoveHistory.Count == 0)
            return;

        var last = MoveHistory.Last();
        MoveHistory.RemoveAt(MoveHistory.Count - 1);

        Board[last.Move.From] = last.MovedPiece;
        Board[last.Move.To] = last.CapturedPiece;
        _halfMoveClock = last.HalfMoveClockBefore;

        CurrentPlayer = CurrentPlayer.Opponent();
        GameResult = null;
    }
}