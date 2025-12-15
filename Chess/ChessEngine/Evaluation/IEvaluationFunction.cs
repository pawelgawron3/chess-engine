using ChessEngine.Game;

namespace ChessEngine.Evaluation;

public interface IEvaluationFunction
{
    public abstract int Evaluate(GameStateEngine state);
}