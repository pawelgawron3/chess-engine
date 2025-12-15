using ChessEngine.Evaluation;
using ChessEngine.Game;
using ChessEngine.Infrastructure.IO;
using ChessEngine.Search;

namespace ChessEngine.Tests.Integration;

public class NegamaxTests
{
    [Theory]
    [InlineData("7k/8/5KQ1/8/8/8/8/8 w - - 0 1")]
    [InlineData("6k1/5ppp/8/r7/8/8/5PPP/6K1 b - - 0 1")]
    [InlineData("r1bq1bkr/ppp3pp/2n5/3np3/2B5/5Q2/PPPP1PPP/RNB1K2R w - - 0 1")] // Fried Liver Attack
    public void Negamax_ShouldFindBestMove(string fen)
    {
        GameStateEngine state = new GameStateEngine();
        FenLoader.LoadFen(state, fen);

        Evaluator evaluator = new Evaluator();
        Negamax negamax = new Negamax(evaluator);
        var (bestMove, score) = negamax.IterativeDeepeningSearch(state, 5);

        Assert.NotNull(bestMove);
        Assert.InRange(score, 999_999, 1_000_010);
    }
}