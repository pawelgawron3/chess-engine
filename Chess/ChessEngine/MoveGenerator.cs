using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class MoveGenerator
{
    public static IEnumerable<Move> GenerateMoves(Board board, Player player)
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
                        foreach (var move in GeneratePawnMoves(board, from, piece))
                            yield return move;
                        break;

                        //case PieceType.Knight:
                        //    foreach (var move in GenerateKnightMoves(from, piece.Owner))
                        //        yield return move;
                        //    break;

                        //case PieceType.Bishop:
                        //    foreach (var move in GenerateBishopMoves(from, piece.Owner))
                        //        yield return move;
                        //    break;

                        //case PieceType.Rook:
                        //    foreach (var move in GenerateRookMoves(from, piece.Owner))
                        //        yield return move;
                        //    break;

                        //case PieceType.Queen:
                        //    foreach (var move in GenerateQueenMoves(from, piece.Owner))
                        //        yield return move;
                        //    break;

                        //case PieceType.King:
                        //    foreach (var move in GenerateKingMoves(from, piece.Owner))
                        //        yield return move;
                        //    break;
                }
            }
        }
    }

    private static IEnumerable<Move> GeneratePawnMoves(Board board, Position pos, Piece piece)
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
}