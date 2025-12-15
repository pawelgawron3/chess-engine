using ChessEngine.Core.Players;

namespace ChessEngine.Core.Rules;

/// <summary>
/// Specifies the reason why a chess game ended.
/// </summary>
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
public class GameResult
{
    public Player Winner { get; }
    public GameEndReason Reason { get; }

    public GameResult(Player winner, GameEndReason reason)
    {
        Winner = winner;
        Reason = reason;
    }

    public static GameResult Win(Player winner)
    {
        return new GameResult(winner, GameEndReason.Checkmate);
    }

    public static GameResult Draw(GameEndReason reason)
    {
        return new GameResult(Player.None, reason);
    }
}