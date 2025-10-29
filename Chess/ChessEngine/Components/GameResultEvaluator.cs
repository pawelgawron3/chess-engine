namespace ChessEngine.Components;

public class GameResultEvaluator
{
    private readonly GameState _state;

    public GameResultEvaluator(GameState state)
    {
        _state = state;
    }

    public Result? Evaluate(ulong currentHash, Dictionary<ulong, int> positions, int halfMoveClock)
    {
        if (positions.TryGetValue(currentHash, out int count) && count >= 3)
            return Result.Draw(GameEndReason.ThreefoldRepetition);

        if (halfMoveClock >= 100)
            return Result.Draw(GameEndReason.FiftyMovesRule);

        var legalMoves = _state.GetLegalMoves().ToList();
        if (legalMoves.Any())
            return null;

        bool kingInCheck = AttackUtils.IsKingInCheck(_state.Board, _state.CurrentPlayer);
        return kingInCheck
            ? Result.Win(_state.CurrentPlayer.Opponent())
            : Result.Draw(GameEndReason.Stalemate);
    }
}