using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class PseudoLegalMoveGenerator
{
    public static IEnumerable<Move> GeneratePseudoLegalMoves(Board board, Player player)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (board[row, col] == null || board[row, col]?.Owner != player) continue;

                Piece piece = board[row, col]!;
                Position from = new Position(row, col);

                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        foreach (var move in GeneratePseudoLegalPawnMoves(board, from, piece))
                            yield return move;
                        break;

                    //case PieceType.Knight:
                    //    foreach (var move in GeneratePseudoLegalKnightMoves(from, piece.Owner))
                    //        yield return move;
                    //    break;

                    //case PieceType.Bishop:
                    //    foreach (var move in GeneratePseudoLegalBishopMoves(from, piece.Owner))
                    //        yield return move;
                    //    break;

                    //case PieceType.Rook:
                    //    foreach (var move in GeneratePseudoLegalRookMoves(from, piece.Owner))
                    //        yield return move;
                    //    break;

                    //case PieceType.Queen:
                    //    foreach (var move in GeneratePseudoLegalQueenMoves(from, piece.Owner))
                    //        yield return move;
                    //    break;

                    case PieceType.King:
                        foreach (var move in GeneratePseudoLegalKingMoves(board, from, player))
                            yield return move;
                        break;
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

    private static IEnumerable<Move> GeneratePseudoLegalKingMoves(Board board, Position pos, Player player)
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

            if (targetPiece == null || targetPiece.Owner == player.Opponent())
            {
                yield return new Move(pos, target);
            }
        }
    }
}