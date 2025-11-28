using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Game;

public class GameStateEngine
{
    public Board Board { get; }
    public Player CurrentPlayer { get; set; } = Player.White;

    public GameResult? GameResult { get; set; }
    public GameServices Services { get; }

    public GameStateEngine()
    {
        Board = new Board();
        Board.Initialize();
        Services = new GameServices(this);
    }

    public IEnumerable<Move> GetLegalMoves() => LegalMoveGenerator.GenerateLegalMoves(this);

    public IEnumerable<Move> GetLegalMovesForPiece(Position pos) => LegalMoveGenerator.GenerateLegalMovesForPiece(this, pos);
}