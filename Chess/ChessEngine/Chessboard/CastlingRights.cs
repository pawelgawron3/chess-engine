namespace ChessEngine.Chessboard;

public struct CastlingRights
{
    public (bool KingMoved, bool RookAMoved, bool RookHMoved) White;
    public (bool KingMoved, bool RookAMoved, bool RookHMoved) Black;

    public CastlingRights((bool, bool, bool) white, (bool, bool, bool) black)
    {
        White = white; Black = black;
    }
}