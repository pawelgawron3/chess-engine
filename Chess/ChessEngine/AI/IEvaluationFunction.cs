using ChessEngine.Game;

namespace ChessEngine.AI;

public interface IEvaluationFunction
{
    public abstract int Evaluate(GameStateEngine state);
}