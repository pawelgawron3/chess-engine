using ChessEngine.Game;
using ChessEngine.Utils;

namespace ChessEngine.Tests.Integration;

public class PerftFenTests
{
    [Theory]
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", 1, 20UL)]
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", 2, 400UL)]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", 1, 48UL)]
    [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", 2, 2039UL)]
    public void Perft_FromFen(string fen, int depth, ulong expected)
    {
        GameStateEngine state = new();
        FenLoader.LoadFen(state, fen);

        ulong nodes = Perft.Run(state, depth);
        Assert.Equal(expected, nodes);
    }
}