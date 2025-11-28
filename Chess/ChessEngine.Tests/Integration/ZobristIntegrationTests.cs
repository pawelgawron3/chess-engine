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

    [Fact]
    public void MakeUndoEngine_MustRestoreZobristHash()
    {
        GameState state = new GameState();
        var initialHash = state.Services.Hasher.CurrentHash;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.Services.EngineMakeMove(move, out var undo);
            state.Services.EngineUndoMove(move, undo);

            Assert.Equal(initialHash, state.Services.Hasher.CurrentHash);
        }
    }
}