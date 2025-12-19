using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;

namespace ChessEngine.Infrastructure.Hashing;

public class ZobristHasher
{
    public ulong CurrentHash { get; set; }
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
            int capturedPlayerIndex = capturedPiece.Value.Owner == Player.White ? 0 : 1;
            int capturedPieceIndex = (int)capturedPiece.Value.Type;
            int capturedSquare = move.Type == MoveType.EnPassant
                ? move.From.Row * 8 + move.To.Column
                : move.To.Row * 8 + move.To.Column;

            CurrentHash ^= Zobrist.PieceKeys[capturedPlayerIndex, capturedPieceIndex, capturedSquare];
        }

        if (move.PromotionPiece != null)
            CurrentHash ^= Zobrist.PieceKeys[playerIndex, (int)move.PromotionPiece.Value, toIndex];
        else
            CurrentHash ^= Zobrist.PieceKeys[playerIndex, pieceIndex, toIndex];

        if (previousEnPassantFile.HasValue)
            CurrentHash ^= Zobrist.EnPassantKeys[previousEnPassantFile.Value];

        if (newEnPassantFile.HasValue)
            CurrentHash ^= Zobrist.EnPassantKeys[newEnPassantFile.Value];

        CurrentHash ^= Zobrist.CastlingKeys[(int)castlingBefore];
        CurrentHash ^= Zobrist.CastlingKeys[(int)castlingAfter];

        if (move.Type == MoveType.Castling)
        {
            int fromRow = move.From.Row;
            int rookPieceIndex = (int)PieceType.Rook;
            if (move.To.Column == 6)
            {
                int rookFrom = fromRow * 8 + 7;
                int rookTo = fromRow * 8 + 5;

                CurrentHash ^= Zobrist.PieceKeys[playerIndex, rookPieceIndex, rookFrom];
                CurrentHash ^= Zobrist.PieceKeys[playerIndex, rookPieceIndex, rookTo];
            }
            else if (move.To.Column == 2)
            {
                int rookFrom = fromRow * 8 + 0;
                int rookTo = fromRow * 8 + 3;

                CurrentHash ^= Zobrist.PieceKeys[playerIndex, rookPieceIndex, rookFrom];
                CurrentHash ^= Zobrist.PieceKeys[playerIndex, rookPieceIndex, rookTo];
            }
        }

        CurrentHash ^= Zobrist.SideToMoveKey;

        if (PositionCounts.ContainsKey(CurrentHash))
            PositionCounts[CurrentHash]++;
        else
            PositionCounts[CurrentHash] = 1;
    }

    internal ulong ComputeZobristHash()
    {
        ulong hash = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = _board[row, col];
                if (piece == null) continue;

                int playerIndex = piece.Value.Owner == Player.White ? 0 : 1;
                int typeIndex = (int)piece.Value.Type;
                int squareIndex = row * 8 + col;

                hash ^= Zobrist.PieceKeys[playerIndex, typeIndex, squareIndex];
            }
        }

        hash ^= Zobrist.CastlingKeys[(int)_getCastlingRights()];

        if (_getEnPassantFile() is int file)
            hash ^= Zobrist.EnPassantKeys[file];

        if (_getCurrentPlayer() == Player.Black)
            hash ^= Zobrist.SideToMoveKey;

        return hash;
    }
}