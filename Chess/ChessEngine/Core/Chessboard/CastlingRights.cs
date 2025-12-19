namespace ChessEngine.Core.Chessboard;

[Flags]
public enum CastlingRights : byte
{
    None = 0,
    WhiteKing = 1 << 0,     // 1
    WhiteQueen = 1 << 1,    // 2
    BlackKing = 1 << 2,     // 4
    BlackQueen = 1 << 3     // 8
}