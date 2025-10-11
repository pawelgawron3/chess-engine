namespace ChessEngine;

public enum GameEndReason
{
    Checkmate,
    Stalemate,
    FiftyMovesRule,
    InsufficientMaterial,
    ThreefoldRepetition
}

/// <summary>
/// Represents the outcome of a chess game, including the winner (if any) and the reason the game ended.
/// Provides helper methods to create a win or a draw result.
/// </summary>
public class Result
{
    public Player Winner { get; }
    public GameEndReason Reason { get; }

    public Result(Player winner, GameEndReason reason)
    {
        Winner = winner;
        Reason = reason;
    }

    public static Result Win(Player winner)
    {
        return new Result(winner, GameEndReason.Checkmate);
    }

    public static Result Draw(GameEndReason reason)
    {
        return new Result(Player.None, reason);
    }
}