namespace ChessEngine.AI;

public class Negamax
{
    private readonly IEvaluationFunction _evaluator;

    public Negamax(IEvaluationFunction evaluator)
    {
        _evaluator = evaluator;
    }
}