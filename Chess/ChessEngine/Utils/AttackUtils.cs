using ChessEngine.Chessboard;
using static ChessEngine.Utils.PositionUtils;

namespace ChessEngine.Utils;

public static class AttackUtils
{
    public static bool IsKingInCheck(Board board, Player player)
    {
        Position kingPos = GetKingPosition(board, player);
        return IsSquareAttacked(board, kingPos, player.Opponent());
    }

    public static bool IsSquareAttacked(Board board, Position square, Player attacker)
    {
        if (!IsInside(square)) return false;

        return IsAttackedByPawn(board, square, attacker) || IsAttackedByKnight(board, square, attacker) ||
               IsAttackedByKing(board, square, attacker) || OrthAttacks(board, square, attacker) ||
               DiagAttacks(board, square, attacker);
    }

    public static Piece? GetCapturedPiece(Board board, Move move) =>
        move.Type switch
        {
            MoveType.Normal or MoveType.Promotion => board[move.To],
            MoveType.EnPassant => board[move.From.Row, move.To.Column],
            _ => null
        };

    public static bool IsCapture(Board board, Move move)
    {
        return GetCapturedPiece(board, move) != null;
    }

    private static Position GetKingPosition(Board board, Player player)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = board[row, col];
                if (piece?.Type == PieceType.King && piece.Value.Owner == player)
                {
                    return new Position(row, col);
                }
            }
        }

        throw new InvalidOperationException($"Error! King not found for player {player}!");
    }

    private static bool IsAttackedByPawn(Board board, Position square, Player attacker)
    {
        int dir = (attacker == Player.White) ? -1 : 1;

        (int dr, int dc)[] pawnTargets =
        {
            (square.Row - dir, square.Column - 1),
            (square.Row - dir, square.Column + 1)
        };

        foreach (var (dr, dc) in pawnTargets)
        {
            if (!IsInside(dr, dc)) continue;
            Piece? piece = board[dr, dc];
            if (piece?.Type == PieceType.Pawn && piece.Value.Owner == attacker)
                return true;
        }

        return false;
    }

    private static bool IsAttackedByKnight(Board board, Position square, Player attacker)
    {
        foreach (var (dr, dc) in KnightJumps)
        {
            int row = square.Row + dr;
            int col = square.Column + dc;

            if (!IsInside(row, col)) continue;
            Piece? piece = board[row, col];
            if (piece?.Type == PieceType.Knight && piece.Value.Owner == attacker)
                return true;
        }

        return false;
    }

    private static bool IsAttackedByKing(Board board, Position square, Player attacker)
    {
        foreach (var (dr, dc) in KingDirections)
        {
            int row = square.Row + dr;
            int col = square.Column + dc;

            if (!IsInside(row, col)) continue;
            Piece? piece = board[row, col];
            if (piece?.Type == PieceType.King && piece.Value.Owner == attacker)
                return true;
        }

        return false;
    }

    private static bool OrthAttacks(Board board, Position square, Player attacker)
    {
        foreach (var (dr, dc) in OrthDirections)
        {
            int row = square.Row + dr;
            int col = square.Column + dc;

            while (IsInside(row, col))
            {
                Piece? piece = board[row, col];
                if (piece != null)
                {
                    if (piece.Value.Owner == attacker && (piece.Value.Type == PieceType.Rook || piece.Value.Type == PieceType.Queen))
                        return true;
                    break;
                }
                row += dr;
                col += dc;
            }
        }

        return false;
    }

    private static bool DiagAttacks(Board board, Position square, Player attacker)
    {
        foreach (var (dr, dc) in DiagDirections)
        {
            int row = square.Row + dr;
            int col = square.Column + dc;

            while (IsInside(row, col))
            {
                Piece? piece = board[row, col];
                if (piece != null)
                {
                    if (piece.Value.Owner == attacker && (piece.Value.Type == PieceType.Bishop || piece.Value.Type == PieceType.Queen))
                        return true;
                    break;
                }
                row += dr;
                col += dc;
            }
        }

        return false;
    }

    private static readonly (int dr, int dc)[] KnightJumps =
    {
        (-2, -1), (-2, 1), (-1, 2), (1, 2),
        (2, 1), (2, -1), (1, -2), (-1, -2)
    };

    private static readonly (int dr, int dc)[] KingDirections =
    {
        (-1,0), (1,0), (0,-1), (0,1),
        (-1,-1), (-1,1), (1,-1), (1,1)
    };

    private static readonly (int dr, int dc)[] OrthDirections =
    {
        (-1, 0), (1, 0),
        (0, -1), (0, 1)
    };

    private static readonly (int dr, int dc)[] DiagDirections =
    {
        (-1, -1), (-1, 1),
        (1, -1), (1, 1)
    };
}