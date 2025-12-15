namespace ChessEngine.Core.Players
{
    public enum Player
    {
        None = 0,
        White = 1,
        Black = -1
    }

    public static class PlayerExtensions
    {
        /// <summary>
        /// Returns the opposing player for the specified player.
        /// </summary>
        public static Player Opponent(this Player player)
        {
            return (Player)(-(int)player);
        }
    }
}