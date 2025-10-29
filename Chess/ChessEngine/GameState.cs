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
    public List<MoveRecord> MoveHistory { get; } = new List<MoveRecord>();
    public Result? GameResult { get; private set; }

    public event Action<Result>? GameEnded;

    public Dictionary<Player, (bool KingMoved, bool RookAMoved, bool RookHMoved)> CastlingRights { get; }

    public event Action<MoveRecord>? MoveMade;

    public int FullMoveCounter => _fullMoveNumber;

    private int _fullMoveNumber = 1;
    private int _halfMoveClock = 0;
    private ulong _currentHash;
    public int? EnPassantFile { get; private set; } = null;
    private readonly Dictionary<ulong, int> _positionCounts = new Dictionary<ulong, int>();

    public GameState()
    {
        Board = new Board();
        Board.Initialize();

        CastlingRights = new Dictionary<Player, (bool, bool, bool)>
        {
            {Player.White, (false, false, false) },
            {Player.Black, (false, false, false) },
        };

        _currentHash = ComputeZobristHash();
        _positionCounts[_currentHash] = 1;
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
        PieceType? promotionPiece = move.PromotionPiece;

        UpdateCastlingRights(movedPiece, move);
        Board.MakeMove(move);
        UpdateEnPassantFile(move, movedPiece);
        UpdateHash();
        bool kingInCheck = AttackUtils.IsKingInCheck(Board, CurrentPlayer.Opponent());
        MoveHistory.Add(new MoveRecord(move, movedPiece, capturedPiece, _halfMoveClock, promotionPiece, kingInCheck));
        MoveMade?.Invoke(MoveHistory.Last());

        UpdateClocks(movedPiece, capturedPiece);
        SwitchPlayer();
        ClearSelection();
        CheckForGameOver();

        return true;
    }

    /// <summary>
    /// Updates castling rights for the current player.
    /// </summary>
    private void UpdateCastlingRights(Piece movedPiece, Move move)
    {
        if (movedPiece.Type == PieceType.King)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];
            if (!king_moved) CastlingRights[movedPiece.Owner] = (true, rookA_moved, rookH_moved);
        }
        else if (movedPiece.Type == PieceType.Rook)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];
            if (!rookA_moved && move.From.Column == 0)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, true, rookH_moved);
            }
            else if (!rookH_moved && move.From.Column == 7)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, rookA_moved, true);
            }
        }
    }

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

    private void SwitchPlayer() => CurrentPlayer = CurrentPlayer.Opponent();

    /// <summary>
    /// Checks game ending conditions.
    /// </summary>
    private void CheckForGameOver()
    {
        if (_positionCounts.TryGetValue(_currentHash, out int count) && count >= 3)
        {
            GameResult = Result.Draw(GameEndReason.ThreefoldRepetition);
            GameEnded?.Invoke(GameResult);
            return;
        }

        if (_halfMoveClock >= 100)
        {
            GameResult = Result.Draw(GameEndReason.FiftyMovesRule);
            GameEnded?.Invoke(GameResult);
            return;
        }

        var legalMoves = GetLegalMoves().ToList();
        if (legalMoves.Any()) return;

        bool kingInCheck = AttackUtils.IsKingInCheck(Board, CurrentPlayer);

        GameResult = kingInCheck
            ? Result.Win(CurrentPlayer.Opponent())
            : Result.Draw(GameEndReason.Stalemate);

        GameEnded?.Invoke(GameResult);
    }

    /// <summary>
    /// Attempts to undo the last move.
    /// </summary>
    public void TryUndoMove()
    {
        if (MoveHistory.Count == 0)
            return;

        var last = MoveHistory.Last();
        MoveHistory.RemoveAt(MoveHistory.Count - 1);
        Board.UndoMove(last);

        _halfMoveClock = last.HalfMoveClockBefore;
        SwitchPlayer();
        GameResult = null;
    }

    private ulong ComputeZobristHash()
    {
        ulong hash = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = Board[row, col];
                if (piece != null)
                {
                    int playerIndex = piece.Owner == Player.White ? 0 : 1;
                    int typeIndex = (int)piece.Type;
                    int squareIndex = row * 8 + col;

                    hash ^= Zobrist.PieceKeys[playerIndex, typeIndex, squareIndex];
                }
            }
        }

        foreach (var kv in CastlingRights)
        {
            int playerIndex = kv.Key == Player.White ? 0 : 1;
            var (king_moved, rookA_moved, rookH_moved) = kv.Value;

            if (!king_moved && !rookA_moved)
                hash ^= Zobrist.CastlingKeys[playerIndex, 0];
            if (!king_moved && !rookH_moved)
                hash ^= Zobrist.CastlingKeys[playerIndex, 1];
        }

        if (EnPassantFile.HasValue)
            hash ^= Zobrist.EnPassantKeys[EnPassantFile.Value];

        if (CurrentPlayer == Player.White)
            hash ^= Zobrist.SideToMoveKey;

        return hash;
    }

    private void UpdateHash()
    {
        _currentHash = ComputeZobristHash();

        if (_positionCounts.ContainsKey(_currentHash))
            _positionCounts[_currentHash]++;
        else
            _positionCounts[_currentHash] = 1;
    }

    private void UpdateEnPassantFile(Move move, Piece movedPiece)
    {
        EnPassantFile = null;

        if (movedPiece.Type == PieceType.Pawn && Math.Abs(move.From.Row - move.To.Row) == 2)
        {
            EnPassantFile = move.From.Column;
        }
    }
}