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

        if (IsInsufficientMaterial(_state.Board))
            return Result.Draw(GameEndReason.InsufficientMaterial);

        if (_state.GetLegalMoves().Any())
            return null;

        bool kingInCheck = AttackUtils.IsKingInCheck(_state.Board, _state.CurrentPlayer);
        return kingInCheck
            ? Result.Win(_state.CurrentPlayer.Opponent())
            : Result.Draw(GameEndReason.Stalemate);
    }

    private bool IsInsufficientMaterial(Board board)
    {
        var pieces = board.GetAllPiecesWithPosition().ToList();
        int pieceCount = pieces.Count;

        if (pieceCount == 2)
            return true;

        if (pieceCount == 3 && pieces.Any(p => p.piece.Type == PieceType.Bishop || p.piece.Type == PieceType.Knight))
            return true;

        if (pieceCount == 4 && HasTwoBishopsOnSameColor(pieces))
            return true;

        return false;
    }

    private bool HasTwoBishopsOnSameColor(List<(Piece piece, Position pos)> pieces)
    {
        var bishops = pieces.Where(p => p.piece.Type == PieceType.Bishop).ToList();
        if (bishops.Count != 2)
            return false;

        return (bishops[0].pos.Row + bishops[0].pos.Column) % 2
                == (bishops[1].pos.Row + bishops[1].pos.Column) % 2;
    }
}