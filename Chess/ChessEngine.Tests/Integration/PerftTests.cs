using ChessEngine.Game;

namespace ChessEngine.Tests.Integration;

public class PerftTests
{
    [Theory]
    [InlineData(1, 20UL)]
    [InlineData(2, 400UL)]
    [InlineData(3, 8902UL)]
    [InlineData(4, 197281UL)]
    [InlineData(5, 4865609UL)]
    public void Perft_StartPosition(int depth, ulong expected)
    {
        var state = new GameStateEngine();
        ulong nodes = Perft.Run(state, depth);
        Assert.Equal(expected, nodes);
    }
}