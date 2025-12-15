using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Players;
using ChessEngine.Core.Rules;
using ChessEngine.Game;

namespace ChessEngine.Evaluation;

public class GameResultEvaluator
{
    private readonly GameStateEngine _state;

    public GameResultEvaluator(GameStateEngine state)
    {
        _state = state;
    }

    public GameResult? Evaluate(ulong currentHash, Dictionary<ulong, int> positions, int halfMoveClock)
    {
        if (positions.TryGetValue(currentHash, out int count) && count >= 3)
            return GameResult.Draw(GameEndReason.ThreefoldRepetition);

        if (halfMoveClock >= 100)
            return GameResult.Draw(GameEndReason.FiftyMovesRule);

        if (IsInsufficientMaterial(_state.Board))
            return GameResult.Draw(GameEndReason.InsufficientMaterial);

        if (_state.GetLegalMoves().Any())
            return null;

        bool kingInCheck = AttackUtils.IsKingInCheck(_state, _state.CurrentPlayer);
        return kingInCheck
            ? GameResult.Win(_state.CurrentPlayer.Opponent())
            : GameResult.Draw(GameEndReason.Stalemate);
    }

    public string ToDisplayString(GameResult? result)
    {
        if (result == null)
            return "Game in progress...";

        return result.Winner switch
        {
            Player.None => $"Draw: {FormatReason(result.Reason)}",
            _ => $"Winner: {result.Winner} ({FormatReason(result.Reason)})"
        };
    }

    private string FormatReason(GameEndReason reason) => reason switch
    {
        GameEndReason.Stalemate => "Stalemate",
        GameEndReason.FiftyMovesRule => "50-move rule",
        GameEndReason.InsufficientMaterial => "Insufficient Material",
        GameEndReason.ThreefoldRepetition => "Threefold Repetition",
        _ => reason.ToString()
    };

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