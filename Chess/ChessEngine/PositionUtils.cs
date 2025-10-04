namespace ChessEngine;

/// <summary>
/// Checks whether the position is inside the standard 8x8 chessboard.
/// </summary>
public static class PositionUtils
{
    public static bool IsInside(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    public static bool IsInside(Position pos)
    {
        return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
    }
}