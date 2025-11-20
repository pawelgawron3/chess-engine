using ChessEngine.Game;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Tests.Integration;

public class ZobristIntegrationTests
{
    [Fact]
    public void MakeUndo_MustRestoreZobristHash()
    {
        GameState state = new GameState();
        var initialHash = state.Services.Hasher.CurrentHash;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.TryMakeMove(move);
            state.TryUndoMove();

            Assert.Equal(initialHash, state.Services.Hasher.CurrentHash);
        }
    }
}