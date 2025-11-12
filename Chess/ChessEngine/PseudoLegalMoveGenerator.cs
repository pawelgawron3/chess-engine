using static ChessEngine.AttackUtils;
using static ChessEngine.PositionUtils;

namespace ChessEngine;

public static class PseudoLegalMoveGenerator
{
    public readonly record struct MoveContext(Board Board, Position From, Piece Piece);

    /// <summary>
    /// Generates all pseudo-legal moves for the currently selected piece.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMovesForPiece(GameState state, Position from)
    {
        Piece? piece = state.Board[from];
        if (piece == null)
            return Enumerable.Empty<Move>();

        var ctx = new MoveContext(state.Board, from, piece);
        return GenerateMovesFor(ctx, state.Services.Rules.EnPassantFile, state.Services.Rules.CastlingRights);
    }

    /// <summary>
    /// Generates all pseudo-legal moves for the current player.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMoves(GameState state)
    {
        Player player = state.CurrentPlayer;
        var castlingRights = state.Services.Rules.CastlingRights;
        int? enPassantFile = state.Services.Rules.EnPassantFile;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = state.Board[row, col];
                if (piece == null || piece.Owner != player) continue;

                Position from = new Position(row, col);
                var ctx = new MoveContext(state.Board, from, piece);

                foreach (var move in GenerateMovesFor(ctx, enPassantFile, castlingRights))
                    yield return move;
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(MoveContext ctx, int? enPassantFile)
    {
        var (board, pos, piece) = ctx;
        int direction = (piece.Owner == Player.White) ? -1 : 1;
        int promotionRow = (piece.Owner == Player.White) ? 0 : 7;

        Position oneStep = new Position(pos.Row + direction, pos.Column);
        if (IsInside(oneStep) && board[oneStep] == null)
        {
            if (oneStep.Row == promotionRow)
            {
                yield return new Move(pos, oneStep, MoveType.Promotion, PieceType.Queen);
                yield return new Move(pos, oneStep, MoveType.Promotion, PieceType.Rook);
                yield return new Move(pos, oneStep, MoveType.Promotion, PieceType.Bishop);
                yield return new Move(pos, oneStep, MoveType.Promotion, PieceType.Knight);
            }
            else
            {
                yield return new Move(pos, oneStep);
            }

            int startRow = (piece.Owner == Player.White) ? 6 : 1;
            Position twoStep = new Position(pos.Row + (2 * direction), pos.Column);
            if (pos.Row == startRow && board[twoStep] == null)
                yield return new Move(pos, twoStep);
        }

        Position[] diagonals =
        {
            new Position(pos.Row + direction, pos.Column - 1),
            new Position(pos.Row + direction, pos.Column + 1)
        };

        foreach (var target in diagonals)
        {
            if (!IsInside(target)) continue;
            Piece? targetPiece = board[target];

            if (targetPiece != null && targetPiece.Owner != piece.Owner)
            {
                if (target.Row == promotionRow)
                {
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Queen);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Rook);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Bishop);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Knight);
                }
                else
                {
                    yield return new Move(pos, target);
                }
            }
        }

        if (enPassantFile is int epFile && Math.Abs(epFile - pos.Column) == 1)
        {
            int targetRow = pos.Row + direction;
            int targetCol = epFile;

            var sidePawn = board[pos.Row, targetCol];
            if (sidePawn?.Type == PieceType.Pawn && sidePawn.Owner != piece.Owner)
            {
                if (IsInside(targetRow, targetCol) && board[targetRow, targetCol] == null)
                    yield return new Move(pos, new Position(targetRow, targetCol), MoveType.EnPassant);
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalKnightMoves(MoveContext ctx)
    {
        var (board, pos, piece) = ctx;

        int[][] jumps =
        [
            [-2, -1], [-2, 1],
            [-1, 2], [1, 2],
            [2, 1], [2, -1],
            [1, -2], [-1, -2],
        ];

        foreach (var jump in jumps)
        {
            int row = pos.Row + jump[0];
            int col = pos.Column + jump[1];

            if (!IsInside(row, col)) continue;

            Position target = new Position(row, col);
            Piece? targetPiece = board[target];

            if (targetPiece == null || targetPiece.Owner != piece.Owner)
                yield return new Move(pos, target);
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalBishopMoves(MoveContext ctx)
    {
        int[][] directions =
        [
            [-1, -1], [-1, 1],
            [1, -1], [1, 1],
        ];

        return GenerateSlidingMoves(ctx, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalRookMoves(MoveContext ctx)
    {
        int[][] directions =
        [
            [-1, 0], [1, 0],
            [0, -1], [0, 1],
        ];

        return GenerateSlidingMoves(ctx, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalQueenMoves(MoveContext ctx)
    {
        int[][] directions =
        [
            [-1, 0], [1, 0],
            [0, -1], [0, 1],
            [-1, -1], [-1, 1],
            [1, -1], [1, 1],
        ];

        return GenerateSlidingMoves(ctx, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalKingMoves(MoveContext ctx,
        CastlingRights castlingRights)
    {
        var (board, pos, piece) = ctx;

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
                yield return new Move(pos, target);
        }

        int row = (piece.Owner == Player.White) ? 7 : 0;
        Player player = piece.Owner;

        var rights = player == Player.White ? castlingRights.White : castlingRights.Black;

        if (!rights.KingMoved)
        {
            if (!rights.RookHMoved &&
                board[row, 5] == null && board[row, 6] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 5), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 6), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 6), MoveType.Castling);
            }

            if (!rights.RookAMoved &&
                board[row, 1] == null && board[row, 2] == null && board[row, 3] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 3), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 2), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 2), MoveType.Castling);
            }
        }
    }

    private static IEnumerable<Move> GenerateSlidingMoves(MoveContext ctx, int[][] directions)
    {
        var (board, pos, piece) = ctx;

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

    private static IEnumerable<Move> GenerateMovesFor(MoveContext ctx,
        int? enPassantFile,
        CastlingRights castlingRights)
    {
        return ctx.Piece.Type switch
        {
            PieceType.Pawn => GeneratePseudoLegalPawnMoves(ctx, enPassantFile),
            PieceType.Knight => GeneratePseudoLegalKnightMoves(ctx),
            PieceType.Bishop => GeneratePseudoLegalBishopMoves(ctx),
            PieceType.Rook => GeneratePseudoLegalRookMoves(ctx),
            PieceType.Queen => GeneratePseudoLegalQueenMoves(ctx),
            PieceType.King => GeneratePseudoLegalKingMoves(ctx, castlingRights),
            _ => Enumerable.Empty<Move>()
        };
    }
}