namespace ChessEngine.Components;

public class ZobristHasher
{
    public ulong CurrentHash { get; internal set; }
    public Dictionary<ulong, int> PositionCounts { get; } = new();

    private readonly Board _board;
    private readonly Func<Player> _getCurrentPlayer;
    private readonly Func<CastlingRights> _getCastlingRights;
    private readonly Func<int?> _getEnPassantFile;

    public ZobristHasher(Board board,
                         Func<Player> getCurrentPlayer,
                         Func<CastlingRights> getCastlingRights,
                         Func<int?> getEnPassantFile)
    {
        _board = board;
        _getCurrentPlayer = getCurrentPlayer;
        _getCastlingRights = getCastlingRights;
        _getEnPassantFile = getEnPassantFile;

        CurrentHash = ComputeZobristHash();
        PositionCounts[CurrentHash] = 1;
    }

    public void ApplyMove(Move move,
        Piece movedPiece,
        Piece? capturedPiece,
        int? previousEnPassantFile,
        int? newEnPassantFile,
        CastlingRights castlingBefore,
        CastlingRights castlingAfter)
    {
        int playerIndex = movedPiece.Owner == Player.White ? 0 : 1;
        int pieceIndex = (int)movedPiece.Type;

        int fromIndex = move.From.Row * 8 + move.From.Column;
        int toIndex = move.To.Row * 8 + move.To.Column;

        CurrentHash ^= Zobrist.PieceKeys[playerIndex, pieceIndex, fromIndex];

        if (capturedPiece != null)
        {
            int capturedPlayerIndex = capturedPiece.Owner == Player.White ? 0 : 1;
            int capturedPieceIndex = (int)capturedPiece.Type;
            int capturedSquare = (movedPiece.Type == PieceType.Pawn && move.Type == MoveType.EnPassant)
                ? move.From.Row * 8 + move.To.Column
                : move.To.Row * 8 + move.To.Column;

            CurrentHash ^= Zobrist.PieceKeys[capturedPlayerIndex, capturedPieceIndex, capturedSquare];
        }

        PieceType newType = move.PromotionPiece ?? movedPiece.Type;
        int newPieceIndex = (int)newType;
        CurrentHash ^= Zobrist.PieceKeys[playerIndex, newPieceIndex, toIndex];

        if (previousEnPassantFile.HasValue)
            CurrentHash ^= Zobrist.EnPassantKeys[previousEnPassantFile.Value];

        if (newEnPassantFile.HasValue)
            CurrentHash ^= Zobrist.EnPassantKeys[newEnPassantFile.Value];

        ulong beforeHash = ComputeCastlingRightsHash(castlingBefore);
        ulong afterHash = ComputeCastlingRightsHash(castlingAfter);

        CurrentHash ^= beforeHash;
        CurrentHash ^= afterHash;

        CurrentHash ^= Zobrist.SideToMoveKey;

        if (PositionCounts.ContainsKey(CurrentHash))
            PositionCounts[CurrentHash]++;
        else
            PositionCounts[CurrentHash] = 1;
    }

    private static ulong ComputeCastlingRightsHash(CastlingRights rights)
    {
        ulong hash = 0;

        if (!rights.White.KingMoved && !rights.White.RookAMoved)
            hash ^= Zobrist.CastlingKeys[0, 0];
        if (!rights.White.KingMoved && !rights.White.RookHMoved)
            hash ^= Zobrist.CastlingKeys[0, 1];

        if (!rights.Black.KingMoved && !rights.Black.RookAMoved)
            hash ^= Zobrist.CastlingKeys[1, 0];
        if (!rights.Black.KingMoved && !rights.Black.RookHMoved)
            hash ^= Zobrist.CastlingKeys[1, 1];

        return hash;
    }

    private ulong ComputeZobristHash()
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

        hash ^= ComputeCastlingRightsHash(_getCastlingRights());

        if (_getEnPassantFile() is int file)
            hash ^= Zobrist.EnPassantKeys[file];

        if (_getCurrentPlayer() == Player.Black)
            hash ^= Zobrist.SideToMoveKey;

        return hash;
    }
}