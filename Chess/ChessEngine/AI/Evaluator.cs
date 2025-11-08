namespace ChessEngine.AI;

public class Evaluator : IEvaluationFunction
{
    private static readonly Dictionary<PieceType, int> _pieceValues = new()
    {
        { PieceType.Pawn, 100},
        { PieceType.Knight, 320},
        { PieceType.Bishop, 330},
        { PieceType.Rook, 500},
        { PieceType.Queen, 900},
        { PieceType.King, 20_000}
    };

    public int Evaluate(GameState state)
    {
        int score = 0;

        foreach (var (piece, pos) in state.Board.GetAllPiecesWithPosition())
        {
            int value = _pieceValues[piece.Type];
            score += (int)piece.Owner * value;
        }

        return score;
    }
}