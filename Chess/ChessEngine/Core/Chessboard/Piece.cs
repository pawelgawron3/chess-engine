using ChessEngine.Core.Players;

namespace ChessEngine.Core.Chessboard;

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
public readonly struct Piece
{
    public PieceType Type { get; }
    public Player Owner { get; }

    public Piece(PieceType type, Player owner)
    {
        Type = type;
        Owner = owner;
    }
}