namespace ChessEngine;

public static class Zobrist
{
    public static readonly ulong[,,] PieceKeys = new ulong[2, 6, 64]; // [player, pieceType, square]
    public static readonly ulong SideToMoveKey;
    public static readonly ulong[,] CastlingKeys = new ulong[2, 2]; // [player, side]
    public static readonly ulong[] EnPassantKeys = new ulong[8];

    static Zobrist()
    {
        Random rng = new Random(2025);

        for (int i = 0; i < PieceKeys.GetLength(0); i++)
            for (int j = 0; j < PieceKeys.GetLength(1); j++)
                for (int k = 0; k < PieceKeys.GetLength(2); k++)
                    PieceKeys[i, j, k] = RandomUlong(rng);

        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                CastlingKeys[i, j] = RandomUlong(rng);

        for (int i = 0; i < 8; i++)
            EnPassantKeys[i] = RandomUlong(rng);

        SideToMoveKey = RandomUlong(rng);
    }

    private static ulong RandomUlong(Random rng)
    {
        byte[] buffer = new byte[8];
        rng.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }
}