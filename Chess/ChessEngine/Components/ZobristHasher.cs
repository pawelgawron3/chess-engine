namespace ChessEngine.Components;

public class ZobristHasher
{
    public ulong CurrentHash { get; private set; }
    public Dictionary<ulong, int> PositionCounts { get; } = new();

    private readonly Board _board;
    private readonly Func<Player> _getCurrentPlayer;
    private readonly Func<Dictionary<Player, (bool, bool, bool)>> _getCastlingRights;
    private readonly Func<int?> _getEnPassantFile;

    public ZobristHasher(Board board,
                         Func<Player> getCurrentPlayer,
                         Func<Dictionary<Player, (bool, bool, bool)>> getCastlingRights,
                         Func<int?> getEnPassantFile)
    {
        _board = board;
        _getCurrentPlayer = getCurrentPlayer;
        _getCastlingRights = getCastlingRights;
        _getEnPassantFile = getEnPassantFile;

        CurrentHash = ComputeZobristHash();
        PositionCounts[CurrentHash] = 1;
    }

    public ulong ComputeZobristHash()
    {
        ulong hash = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = _board[row, col];
                if (piece == null) continue;

                int playerIndex = piece.Owner == Player.White ? 0 : 1;
                int typeIndex = (int)piece.Type;
                int squareIndex = row * 8 + col;

                hash ^= Zobrist.PieceKeys[playerIndex, typeIndex, squareIndex];
            }
        }

        foreach (var kv in _getCastlingRights())
        {
            int playerIndex = kv.Key == Player.White ? 0 : 1;
            var (king_moved, rookA_moved, rookH_moved) = kv.Value;

            if (!king_moved && !rookA_moved)
                hash ^= Zobrist.CastlingKeys[playerIndex, 0];
            if (!king_moved && !rookH_moved)
                hash ^= Zobrist.CastlingKeys[playerIndex, 1];
        }

        if (_getEnPassantFile() is int file)
            hash ^= Zobrist.EnPassantKeys[file];

        if (_getCurrentPlayer() == Player.White)
            hash ^= Zobrist.SideToMoveKey;

        return hash;
    }

    public void UpdateHash()
    {
        CurrentHash = ComputeZobristHash();

        if (PositionCounts.ContainsKey(CurrentHash))
            PositionCounts[CurrentHash]++;
        else
            PositionCounts[CurrentHash] = 1;
    }
}