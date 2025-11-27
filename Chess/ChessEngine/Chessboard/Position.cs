namespace ChessEngine.Chessboard;

/// <summary>
/// Represents a position on the chessboard using zero-based coordinates.
/// Row and column are integers from 0 to 7.
/// </summary>
public readonly struct Position
{
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public int ToIndex() => Row * 8 + Column;

    public override bool Equals(object? obj) => obj is Position position && position.Row == Row && position.Column == Column;

    public override int GetHashCode() => HashCode.Combine(Row, Column);

    public static bool operator ==(Position a, Position b) => a.Equals(b);

    public static bool operator !=(Position a, Position b) => !a.Equals(b);
}