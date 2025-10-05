namespace ChessEngine;

public class GameState
{
    public Board Board { get; }
    public Player CurrentPlayer => Board.CurrentPlayer;
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
        if (Board.MakeMove(move))
        {
            MoveHistory.Add(move);
            SelectedPosition = null;
            return true;
        }
        return false;
    }
}