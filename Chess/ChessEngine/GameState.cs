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
    public Dictionary<Player, (bool KingMoved, bool RookAMoved, bool RookHMoved)> CastlingRights { get; }
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
        CastlingRights = new Dictionary<Player, (bool, bool, bool)>
        {
            {Player.White, (false, false, false) },
            {Player.Black, (false, false, false) },
        };
    }

    /// <summary>
    /// Returns all legal moves for the currently selected piece.
    /// </summary>
    public IEnumerable<Move> GetLegalMovesForPiece()
    {
        if (SelectedPosition == null)
            return Enumerable.Empty<Move>();

        return GenerateLegalMovesForPiece(this);
    }

    /// <summary>
    /// Returns all legal moves for the current player.
    /// </summary>
    public IEnumerable<Move> GetLegalMoves()
    {
        return GenerateLegalMoves(this);
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

        if (!IsMoveLegal(this, move))
            return false;

        Piece movedPiece = Board[move.From]!;
        Piece? capturedPiece = move.Type switch
        {
            MoveType.Normal or MoveType.Promotion => Board[move.To],
            MoveType.EnPassant => Board[move.From.Row, move.To.Column],
            _ => null
        };

        if (movedPiece.Type == PieceType.King)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];
            if (king_moved == false)
                CastlingRights[movedPiece.Owner] = (true, rookA_moved, rookH_moved);
        }
        else if (movedPiece.Type == PieceType.Rook)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];

            if (rookA_moved == false && move.From.Column == 0)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, true, rookH_moved);
            }
            else if (rookH_moved == false && move.From.Column == 7)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, rookA_moved, true);
            }
        }

        Board.MakeMove(move);
        MoveHistory.Add(new MoveRecord(move, movedPiece, capturedPiece, _halfMoveClock));

        _halfMoveClock = (movedPiece.Type == PieceType.Pawn || capturedPiece != null) ? 0 : _halfMoveClock + 1;

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
        CurrentPlayer = CurrentPlayer.Opponent();
        GameResult = null;
    }
}