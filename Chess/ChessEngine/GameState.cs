using static ChessEngine.PositionUtils;

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
    public List<Move> MoveHistory { get; } = new List<Move>();

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
    /// Generates all pseudo-legal moves for the currently selected piece.
    /// </summary>
    public IEnumerable<Move> GetPseudoLegalMovesForPiece()
    {
        if (SelectedPosition == null)
            return Enumerable.Empty<Move>();

        return PseudoLegalMoveGenerator.GeneratePseudoLegalMovesForPiece(Board, SelectedPosition.Value);
    }

    /// <summary>
    /// Generates all pseudo-legal moves for the current player.
    /// </summary>
    public IEnumerable<Move> GetPseudoLegalMoves()
    {
        return PseudoLegalMoveGenerator.GeneratePseudoLegalMoves(Board, CurrentPlayer);
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
        if (!IsMovePseudoLegal(move))
            return false;

        Board.MakeMove(move);
        MoveHistory.Add(move);
        ClearSelection();
        CurrentPlayer = CurrentPlayer.Opponent();
        return true;
    }

    /// <summary>
    /// Checks whether a given move is pseudo-legal within the current game state.
    /// </summary>
    public bool IsMovePseudoLegal(Move move)
    {
        if (!IsInside(move.From) || !IsInside(move.To))
            return false;

        Piece? piece = Board[move.From];
        if (piece == null)
            return false;

        if (piece.Owner != CurrentPlayer)
            return false;

        Piece? targetPiece = Board[move.To];
        if (targetPiece != null && targetPiece.Owner == CurrentPlayer)
            return false;

        if (!PseudoLegalMoveGenerator
        .GeneratePseudoLegalMovesForPiece(Board, move.From)
        .Any(m => m.To.Equals(move.To)))
            return false;

        return true;
    }
}