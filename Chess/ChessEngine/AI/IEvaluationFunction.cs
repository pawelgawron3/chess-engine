namespace ChessEngine.AI;

public interface IEvaluationFunction
{
    public abstract int Evaluate(GameState state);
}