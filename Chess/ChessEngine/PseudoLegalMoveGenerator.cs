using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class PseudoLegalMoveGenerator
{
    public static IEnumerable<Move> GeneratePseudoLegalMovesForPiece(Board board, Position from)
    {
        Piece? piece = board[from];
        if (piece == null) return Enumerable.Empty<Move>();

        return GenerateMovesFor(board, from, piece);
    }

    public static IEnumerable<Move> GeneratePseudoLegalMoves(Board board, Player player)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (board[row, col] == null || board[row, col]?.Owner != player) continue;

                Position from = new Position(row, col);
                Piece piece = board[from]!;

                foreach (var move in GenerateMovesFor(board, from, piece))
                {
                    yield return move;
                }
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(Board board, Position pos, Piece piece)
    {
        int direction = (piece.Owner == Player.White) ? -1 : 1;

        Position oneStep = new Position(pos.Row + direction, pos.Column);
        if (IsInside(oneStep) && board[oneStep] == null)
        {
            yield return new Move(pos, oneStep);

            int startRow = (piece.Owner == Player.White) ? 6 : 1;
            Position twoStep = new Position(pos.Row + (2 * direction), pos.Column);
            if (pos.Row == startRow && board[twoStep] == null)
            {
                yield return new Move(pos, twoStep);
            }
        }

        Position[] diagonals =
        {
            new Position(pos.Row + direction, pos.Column - 1),
            new Position(pos.Row + direction, pos.Column + 1)
        };

        foreach (var target in diagonals)
        {
            Piece? targetPiece = board[target];
            if (IsInside(target) && targetPiece != null && targetPiece.Owner != piece.Owner)
            {
                yield return new Move(pos, target);
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalKnightMoves(Board board, Position pos, Piece piece)
    {
        int[][] jumps =
        [
            [-2, -1],
            [-2, 1],
            [-1, 2],
            [1, 2],
            [2, 1],
            [2, -1],
            [1, -2],
            [-1, -2],
        ];

        foreach (var jump in jumps)
        {
            int row = pos.Row + jump[0];
            int col = pos.Column + jump[1];

            if (!IsInside(row, col)) continue;

            Position target = new Position(row, col);
            Piece? targetPiece = board[target];

            if (targetPiece == null || targetPiece.Owner != piece.Owner)
            {
                yield return new Move(pos, target);
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalBishopMoves(Board board, Position pos, Piece piece)
    {
        int[][] directions =
        [
            [-1, -1],
            [-1, 1],
            [1, -1],
            [1, 1],
        ];

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalRookMoves(Board board, Position pos, Piece piece)
    {
        int[][] directions =
        [
            [-1, 0],
            [1, 0],
            [0, -1],
            [0, 1],
        ];

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalQueenMoves(Board board, Position pos, Piece piece)
    {
        int[][] directions =
        [
            [-1, 0],
            [1, 0],
            [0, -1],
            [0, 1],
            [-1, -1],
            [-1, 1],
            [1, -1],
            [1, 1],
        ];

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalKingMoves(Board board, Position pos, Piece piece)
    {
        Position[] directions =
        {
        new Position(pos.Row - 1, pos.Column),
        new Position(pos.Row + 1, pos.Column),
        new Position(pos.Row, pos.Column - 1),
        new Position(pos.Row, pos.Column + 1),
        new Position(pos.Row - 1, pos.Column - 1),
        new Position(pos.Row - 1, pos.Column + 1),
        new Position(pos.Row + 1, pos.Column - 1),
        new Position(pos.Row + 1, pos.Column + 1)
        };

        foreach (var target in directions)
        {
            if (!IsInside(target)) continue;

            Piece? targetPiece = board[target];

            if (targetPiece == null || targetPiece.Owner != piece.Owner)
            {
                yield return new Move(pos, target);
            }
        }
    }

    private static IEnumerable<Move> GenerateSlidingMoves(Board board, Position pos, Piece piece, int[][] directions)
    {
        foreach (var dir in directions)
        {
            int row = pos.Row + dir[0];
            int col = pos.Column + dir[1];

            while (IsInside(row, col))
            {
                Position target = new Position(row, col);
                Piece? targetPiece = board[target];

                if (targetPiece == null)
                {
                    yield return new Move(pos, target);
                }
                else
                {
                    if (targetPiece.Owner != piece.Owner)
                    {
                        yield return new Move(pos, target);
                    }
                    break;
                }

                row += dir[0];
                col += dir[1];
            }
        }
    }

    private static IEnumerable<Move> GenerateMovesFor(Board board, Position from, Piece piece)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn: return GeneratePseudoLegalPawnMoves(board, from, piece);
            case PieceType.Knight: return GeneratePseudoLegalKnightMoves(board, from, piece);
            case PieceType.Bishop: return GeneratePseudoLegalBishopMoves(board, from, piece);
            case PieceType.Rook: return GeneratePseudoLegalRookMoves(board, from, piece);
            case PieceType.Queen: return GeneratePseudoLegalQueenMoves(board, from, piece);
            case PieceType.King: return GeneratePseudoLegalKingMoves(board, from, piece);
            default: return Enumerable.Empty<Move>();
        }
    }
}