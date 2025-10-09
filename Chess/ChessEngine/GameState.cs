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
    public Result? Result { get; private set; } = null;

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
        if (!IsMoveLegal(Board, move, CurrentPlayer))
            return false;

        Piece movedPiece = Board[move.From]!;
        Piece? capturedPiece = Board[move.To];

        Board.MakeMove(move);

        MoveHistory.Add(new MoveRecord(move, movedPiece, capturedPiece));

        ClearSelection();
        CurrentPlayer = CurrentPlayer.Opponent();
        return true;
    }

    public void UndoLastMove()
    {
        if (MoveHistory.Count == 0)
            return;

        var last = MoveHistory.Last();
        MoveHistory.RemoveAt(MoveHistory.Count - 1);

        Board[last.Move.From] = last.MovedPiece;
        Board[last.Move.To] = last.CapturedPiece;

        CurrentPlayer = CurrentPlayer.Opponent();
    }
}