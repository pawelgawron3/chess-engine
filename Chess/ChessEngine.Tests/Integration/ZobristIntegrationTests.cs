using ChessEngine.Game;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Tests.Integration;

public class ZobristIntegrationTests
{
    [Fact]
    public void MakeUndoEngine_MustRestoreZobristHash()
    {
        GameStateEngine state = new GameStateEngine();
        var initialHash = state.Services.Hasher.CurrentHash;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.Services.EngineMakeMove(move, out var undo);
            state.Services.EngineUndoMove(move, undo);

            Assert.Equal(initialHash, state.Services.Hasher.CurrentHash);
        }
    }
}