namespace ChessEngine;

/// <summary>
/// Represents a position on the chessboard using zero-based coordinates.
/// Row and column are integers from 0 to 7.
/// </summary>
public readonly struct Position
{
    public int Row { get; }
    public int Column { get; }

    /// <summary>
    /// Checks whether the position is inside the standard 8x8 chessboard.
    /// </summary>
    public bool IsValid
    {
        get
        {
            return Row >= 0 && Row < 8 && Column >= 0 && Column < 8;
        }
    }

    /// <summary>
    /// Gets the color of the square at this position on the chessboard.
    /// </summary>
    public SquareColor Color
    {
        get
        {
            return (Row + Column) % 2 == 0 ? SquareColor.Light : SquareColor.Dark;
        }
    }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override bool Equals(object? obj) => obj is Position position && position.Row == Row && position.Column == Column;

    public override int GetHashCode() => HashCode.Combine(Row, Column);

    public static bool operator ==(Position a, Position b) => a.Equals(b);

    public static bool operator !=(Position a, Position b) => !a.Equals(b);
}