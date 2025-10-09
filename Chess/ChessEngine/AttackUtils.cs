using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class AttackUtils
{
    public static Position GetKingPosition(Board board, Player player)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = board[row, col];
                if (piece != null && piece.Type == PieceType.King && piece.Owner == player)
                {
                    return new Position(row, col);
                }
            }
        }

        throw new InvalidOperationException($"Error! King not found for player {player}!");
    }

    public static bool IsSquareAttacked(Board board, Position square, Player attacker)
    {
        if (!IsInside(square)) return false;

        int pawnDir = (attacker == Player.White) ? -1 : 1;
        Position[] pawnTargets =
        {
            new Position(square.Row - pawnDir, square.Column - 1),
            new Position(square.Row - pawnDir, square.Column + 1)
        };

        foreach (var pos in pawnTargets)
        {
            if (!IsInside(pos)) continue;
            Piece? piece = board[pos];
            if (piece != null && piece.Type == PieceType.Pawn && piece.Owner == attacker)
                return true;
        }

        int[][] knightJumps =
        [
            [-2, -1], [-2, 1], [-1, -2], [-1, 2],
            [1, -2],  [1, 2],  [2, -1],  [2, 1]
        ];

        foreach (var jump in knightJumps)
        {
            int row = square.Row + jump[0];
            int col = square.Column + jump[1];
            if (!IsInside(row, col)) continue;
            Piece? piece = board[row, col];
            if (piece != null && piece.Type == PieceType.Knight && piece.Owner == attacker)
                return true;
        }

        int[][] orthDirs =
        [
            [-1, 0], [1, 0], [0, -1], [0, 1]
        ];

        foreach (var direction in orthDirs)
        {
            int row = square.Row + direction[0];
            int col = square.Column + direction[1];

            while (IsInside(row, col))
            {
                Piece? piece = board[row, col];
                if (piece != null)
                {
                    if (piece.Owner == attacker && (piece.Type == PieceType.Rook || piece.Type == PieceType.Queen))
                        return true;
                    break;
                }
                row += direction[0];
                col += direction[1];
            }
        }

        int[][] diagDirs =
        [
            [-1, -1], [-1, 1], [1, -1], [1, 1]
        ];

        foreach (var direction in diagDirs)
        {
            int row = square.Row + direction[0];
            int col = square.Column + direction[1];

            while (IsInside(row, col))
            {
                Piece? piece = board[row, col];
                if (piece != null)
                {
                    if (piece.Owner == attacker && (piece.Type == PieceType.Bishop || piece.Type == PieceType.Queen))
                        return true;
                    break;
                }
                row += direction[0];
                col += direction[1];
            }
        }

        Position[] kingAdjacency =
        {
                new Position(square.Row - 1, square.Column),
                new Position(square.Row + 1, square.Column),
                new Position(square.Row, square.Column - 1),
                new Position(square.Row, square.Column + 1),
                new Position(square.Row - 1, square.Column - 1),
                new Position(square.Row - 1, square.Column + 1),
                new Position(square.Row + 1, square.Column - 1),
                new Position(square.Row + 1, square.Column + 1)
        };

        foreach (var pos in kingAdjacency)
        {
            if (!IsInside(pos)) continue;
            Piece? piece = board[pos];
            if (piece != null && piece.Type == PieceType.King && piece.Owner == attacker)
                return true;
        }

        return false;
    }
}