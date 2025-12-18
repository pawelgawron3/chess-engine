using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;
using ChessEngine.Game;
using static ChessEngine.Infrastructure.Utils.PositionUtils;

namespace ChessEngine.Core.Rules;

public static class AttackUtils
{
    public static bool IsKingInCheck(GameStateEngine state, Player player)
    {
        Position kingPos = player == Player.White ? state.WhiteKingPos : state.BlackKingPos;
        return IsSquareAttacked(state.Board, kingPos, player.Opponent());
    }

    public static bool IsSquareAttacked(Board board, Position square, Player attacker)
    {
        if (!IsInside(square)) return false;

        return IsAttackedByPawn(board, square, attacker) || IsAttackedByKnight(board, square, attacker) ||
               OrthAttacks(board, square, attacker) || DiagAttacks(board, square, attacker) ||
               IsAttackedByKing(board, square, attacker);
    }

    public static Piece? GetCapturedPiece(Board board, Move move)
    {
        if (move.Type == MoveType.EnPassant)
            return board[move.From.Row, move.To.Column];
        else if (move.IsCapture)
            return board[move.To];
        else return null;
    }

    private static bool IsAttackedByPawn(Board board, Position square, Player attacker)
    {
        int dir = attacker == Player.White ? -1 : 1;

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