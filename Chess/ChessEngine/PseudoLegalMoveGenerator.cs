using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class PseudoLegalMoveGenerator
{
    /// <summary>
    /// Generates all pseudo-legal moves for the currently selected piece.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMovesForPiece(GameState state, Position from)
    {
        Piece? piece = state.Board[from];
        MoveRecord? lastMove = (state.MoveHistory.Count > 0) ? state.MoveHistory.Last() : null;

        if (piece == null) return Enumerable.Empty<Move>();

        return GenerateMovesFor(state.Board, from, piece, lastMove);
    }

    /// <summary>
    /// Generates all pseudo-legal moves for the current player.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMoves(GameState state)
    {
        Player player = state.CurrentPlayer;
        MoveRecord? lastMove = (state.MoveHistory.Count > 0) ? state.MoveHistory.Last() : null;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (state.Board[row, col] == null || state.Board[row, col]?.Owner != player) continue;

                Position from = new Position(row, col);
                Piece piece = state.Board[from]!;

                foreach (var move in GenerateMovesFor(state.Board, from, piece, lastMove))
                {
                    yield return move;
                }
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(Board board, Position pos, Piece piece, MoveRecord? lastMove)
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

        if (lastMove != null &&
            lastMove.MovedPiece.Type == PieceType.Pawn &&
            lastMove.MovedPiece.Owner != piece.Owner &&
            Math.Abs(lastMove.Move.From.Row - lastMove.Move.To.Row) == 2)
        {
            int lastPawnCol = lastMove.Move.To.Column;

            if (Math.Abs(lastPawnCol - pos.Column) == 1 && lastMove.Move.To.Row == pos.Row)
            {
                Position enPassantTarget = new Position(pos.Row + direction, lastPawnCol);

                if (IsInside(enPassantTarget) && board[enPassantTarget] == null)
                {
                    yield return new Move(pos, enPassantTarget, MoveType.EnPassant);
                }
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

    private static IEnumerable<Move> GenerateMovesFor(Board board, Position from, Piece piece, MoveRecord? lastMove)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn: return GeneratePseudoLegalPawnMoves(board, from, piece, lastMove);
            case PieceType.Knight: return GeneratePseudoLegalKnightMoves(board, from, piece);
            case PieceType.Bishop: return GeneratePseudoLegalBishopMoves(board, from, piece);
            case PieceType.Rook: return GeneratePseudoLegalRookMoves(board, from, piece);
            case PieceType.Queen: return GeneratePseudoLegalQueenMoves(board, from, piece);
            case PieceType.King: return GeneratePseudoLegalKingMoves(board, from, piece);
            default: return Enumerable.Empty<Move>();
        }
    }
}