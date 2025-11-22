using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.Game;
using ChessEngine.MoveGeneration;
using ChessEngine.Utils;

namespace ChessEngine.AI;

public class Negamax
{
    private const int MATE_SCORE = 1_000_000;
    private readonly IEvaluationFunction _evaluator;

    public Negamax(IEvaluationFunction evaluator)
    {
        _evaluator = evaluator;
    }

    public (Move? BestMove, int Score) Search(GameState state, int depth)
    {
        state.Services.SimulationMode = true;

        KillerMoves.Init(depth);
        Move? bestMove = null;
        int score = NegamaxSearch(state, depth, int.MinValue + 1, int.MaxValue - 1, true, ref bestMove);

        state.Services.SimulationMode = false;
        return (bestMove, score);
    }

    private int NegamaxSearch(GameState state, int depth, int alpha, int beta, bool isRootMove, ref Move? bestRootMove)
    {
        if (state.GameResult != null)
        {
            return state.GameResult.Reason switch
            {
                GameEndReason.Checkmate => -MATE_SCORE - depth,
                _ => 0
            };
        }

        if (depth == 0)
            return _evaluator.Evaluate(state) * (int)state.CurrentPlayer;

        ulong hash = state.Services.Hasher.CurrentHash;
        ref TTEntry entry = ref TranspositionTable.Probe(hash);

        int alphaOrig = alpha;
        Move? bestMoveLocal = null;

        if (!isRootMove && entry.Hash == hash && entry.Depth >= depth)
        {
            switch (entry.Bound)
            {
                case BoundType.Exact:
                    return entry.Score;

                case BoundType.LowerBound:
                    if (entry.Score > alpha)
                        alpha = entry.Score;
                    break;

                case BoundType.UpperBound:
                    if (entry.Score < beta)
                        beta = entry.Score;
                    break;
            }

            if (alpha >= beta)
                return entry.Score;
        }

        var movePicker = new MovePicker(LegalMoveGenerator.GenerateLegalMoves(state), state, depth, entry.BestMove);

        while (movePicker.TryGetNext(out var move))
        {
            state.TryMakeMove(move);
            int score = -NegamaxSearch(state, depth - 1, -beta, -alpha, false, ref bestRootMove);
            state.TryUndoMove();

            if (score > alpha)
            {
                alpha = score;
                bestMoveLocal = move;

                if (isRootMove)
                    bestRootMove = move;
            }

            if (alpha >= beta)
            {
                if (AttackUtils.IsQuietMove(state.Board, move))
                    KillerMoves.AddKillerMove(depth, move);

                break;
            }
        }

        BoundType bound;
        if (alpha <= alphaOrig)
            bound = BoundType.UpperBound;
        else if (alpha >= beta)
            bound = BoundType.LowerBound;
        else
            bound = BoundType.Exact;

        TranspositionTable.Store(hash, depth, alpha, bound, bestMoveLocal);

        return alpha;
    }
}