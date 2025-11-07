namespace ChessEngine.AI;

public class Negamax
{
    private readonly IEvaluationFunction _evaluator;

    public Negamax(IEvaluationFunction evaluator)
    {
        _evaluator = evaluator;
    }

    public (Move? BestMove, int Score) Search(GameState state, int depth)
    {
        state.Services.SimulationMode = true;

        int bestScore = int.MinValue;
        Move? bestMove = null;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.TryMakeMove(move);
            int score = -NegamaxSearch(state, depth - 1);
            state.TryUndoMove();

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        state.Services.SimulationMode = false;
        return (bestMove, bestScore);
    }

    private int NegamaxSearch(GameState state, int depth)
    {
        if (depth == 0 || state.GameResult != null)
            return _evaluator.Evaluate(state) * (int)state.CurrentPlayer;

        int bestScore = int.MinValue;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.TryMakeMove(move);
            int score = -NegamaxSearch(state, depth - 1);
            state.TryUndoMove();

            if (score > bestScore)
                bestScore = score;
        }

        return bestScore;
    }
}