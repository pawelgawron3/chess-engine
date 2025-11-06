namespace ChessEngine.AI;

public class Evaluator : IEvaluationFunction
{
    private static readonly Dictionary<PieceType, int> _pieceValues = new()
    {
        { PieceType.Pawn, 100},
        { PieceType.Knight, 300},
        { PieceType.Bishop, 310},
        { PieceType.Rook, 500},
        { PieceType.Queen, 900},
        { PieceType.King, 10_000}
    };

    public int Evaluate(GameState state)
    {
        throw new NotImplementedException();
    }
}