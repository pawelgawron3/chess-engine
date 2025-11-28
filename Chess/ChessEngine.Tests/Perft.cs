using ChessEngine.Game;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Tests;

public static class Perft
{
    public static ulong Run(GameStateEngine state, int depth)
    {
        if (depth == 0)
            return 1;

        ulong nodes = 0;

        foreach (var move in LegalMoveGenerator.GenerateLegalMoves(state))
        {
            state.Services.EngineMakeMove(move, out var undo);
            nodes += Run(state, depth - 1);
            state.Services.EngineUndoMove(move, undo);
        }

        return nodes;
    }
}