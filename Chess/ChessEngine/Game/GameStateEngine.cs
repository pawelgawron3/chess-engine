using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;
using ChessEngine.Core.Rules;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Game;

public class GameStateEngine
{
    public Board Board { get; }
    public Player CurrentPlayer { get; set; } = Player.White;

    public GameResult? GameResult { get; set; }
    public GameServices Services { get; }
    public Position WhiteKingPos { get; set; } = new(7, 4);
    public Position BlackKingPos { get; set; } = new(0, 4);

    public GameStateEngine()
    {
        Board = new Board();
        Board.Initialize();
        Services = new GameServices(this);
    }

    public IEnumerable<Move> GetLegalMoves() => LegalMoveGenerator.GenerateLegalMoves(this);

    public IEnumerable<Move> GetLegalMovesForPiece(Position pos) => LegalMoveGenerator.GenerateLegalMovesForPiece(this, pos);
}