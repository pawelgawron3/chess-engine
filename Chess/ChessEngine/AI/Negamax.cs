using ChessEngine.Chessboard;
using ChessEngine.Game;
using ChessEngine.MoveGeneration;

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

        Move? bestMove = null;
        int score = NegamaxSearch(state, depth, int.MinValue + 1, int.MaxValue - 1, true, ref bestMove);

        state.Services.SimulationMode = false;
        return (bestMove, score);
    }

    private int NegamaxSearch(GameState state, int depth, int alpha, int beta, bool isRootMove, ref Move? bestRootMove)
    {
        if (depth == 0 || state.GameResult != null)
            return _evaluator.Evaluate(state) * (int)state.CurrentPlayer;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.TryMakeMove(move);
            int score = -NegamaxSearch(state, depth - 1, -beta, -alpha, false, ref bestRootMove);
            state.TryUndoMove();

            if (score > alpha)
            {
                alpha = score;

                if (isRootMove)
                    bestRootMove = move;
            }

            if (alpha >= beta)
                break;
        }

        return alpha;
    }
}