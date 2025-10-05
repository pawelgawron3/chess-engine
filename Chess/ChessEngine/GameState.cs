using static ChessEngine.PositionUtils;

namespace ChessEngine;

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

    public IEnumerable<Move> GetPseudoLegalMoves()
    {
        return PseudoLegalMoveGenerator.GeneratePseudoLegalMoves(Board, CurrentPlayer);
    }

    public void SelectPosition(Position pos)
    {
        SelectedPosition = pos;
    }

    public void ClearSelection()
    {
        SelectedPosition = null;
    }

    public bool TryMakeMove(Move move)
    {
        if (!IsMoveLegal(move))
            return false;

        Board.MakeMove(move);
        MoveHistory.Add(move);
        SelectedPosition = null;
        CurrentPlayer = CurrentPlayer.Opponent();
        return true;
    }

    public bool IsMoveLegal(Move move)
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

        return true;
    }
}