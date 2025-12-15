using ChessEngine.Core.Players;
using ChessEngine.Core.Rules;
using ChessEngine.Game;
using ChessEngine.Infrastructure.IO;

namespace ChessEngine.Tests.Integration;

public class GameEndTests
{
    [Theory]
    [InlineData("7k/5K2/6Q1/8/8/8/8/8 b - - 0 1", GameEndReason.Stalemate, Player.None)]
    [InlineData("7k/5KQ1/8/8/8/8/8/8 b - - 0 1", GameEndReason.Checkmate, Player.White)]
    [InlineData("7K/5k2/6q1/8/8/8/8/8 w - - 0 1", GameEndReason.Stalemate, Player.None)]
    [InlineData("7k/8/8/8/8/8/8/K7 w - - 10 10", GameEndReason.InsufficientMaterial, Player.None)]
    [InlineData("8/8/8/8/8/8/8/8 w - - 100 50", GameEndReason.FiftyMovesRule, Player.None)]
    public void GameEndDetection_Fen(string fen, GameEndReason expectedReason, Player? expectedWinner)
    {
        GameStateEngine state = new GameStateEngine();
        FenLoader.LoadFen(state, fen);

        state.GameResult = state.Services.Evaluator.Evaluate(
            state.Services.Hasher.CurrentHash,
            state.Services.Hasher.PositionCounts,
            state.Services.HalfMoveClock
        );

        Assert.Equal(expectedReason, state.GameResult?.Reason);
        Assert.Equal(expectedWinner, state.GameResult?.Winner);
    }
}