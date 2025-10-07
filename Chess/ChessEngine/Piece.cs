namespace ChessEngine;

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

/// <summary>
/// Represents a chess piece with a specific type and owner (player).
/// </summary>
public class Piece
{
    public PieceType Type { get; }
    public Player Owner { get; }

    public Piece(PieceType type, Player owner)
    {
        Type = type;
        Owner = owner;
    }

    /// <summary>
    /// Creates a deep copy of this <see cref="Piece"/> instance.
    /// </summary>
    public Piece Clone() => new Piece(Type, Owner);
}