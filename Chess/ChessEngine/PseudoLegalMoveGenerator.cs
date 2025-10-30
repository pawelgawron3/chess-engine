using static ChessEngine.AttackUtils;
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
        var castlingRights = state.Services.Rules.CastlingRights;
        int? enPassantFile = state.Services.Rules.EnPassantFile;

        if (piece == null) return Enumerable.Empty<Move>();

        return GenerateMovesFor(state.Board, from, piece, enPassantFile, castlingRights);
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
                if (state.Board[row, col] == null || state.Board[row, col]?.Owner != player) continue;

                Position from = new Position(row, col);
                Piece piece = state.Board[from]!;

                foreach (var move in GenerateMovesFor(state.Board, from, piece, enPassantFile, castlingRights))
                {
                    yield return move;
                }
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(Board board, Position pos, Piece piece, int? enPassantFile)
    {
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

    private static IEnumerable<Move> GeneratePseudoLegalKingMoves(Board board, Position pos, Piece piece, Dictionary<Player, (bool KingMoved, bool RookAMoved, bool RookHMoved)> castlingRights)
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

        int row = (piece.Owner == Player.White) ? 7 : 0;
        Player player = (row == 7) ? Player.White : Player.Black;

        if (!castlingRights[player].KingMoved)
        {
            if (!castlingRights[player].RookHMoved &&
                board[row, 5] == null && board[row, 6] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 5), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 6), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 6), MoveType.Castling);
            }
            if (!castlingRights[player].RookAMoved &&
                board[row, 1] == null && board[row, 2] == null && board[row, 3] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 3), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 2), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 2), MoveType.Castling);
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

    private static IEnumerable<Move> GenerateMovesFor(Board board, Position from, Piece piece,
        int? enPassantFile, Dictionary<Player, (bool, bool, bool)> castlingRights)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn: return GeneratePseudoLegalPawnMoves(board, from, piece, enPassantFile);
            case PieceType.Knight: return GeneratePseudoLegalKnightMoves(board, from, piece);
            case PieceType.Bishop: return GeneratePseudoLegalBishopMoves(board, from, piece);
            case PieceType.Rook: return GeneratePseudoLegalRookMoves(board, from, piece);
            case PieceType.Queen: return GeneratePseudoLegalQueenMoves(board, from, piece);
            case PieceType.King: return GeneratePseudoLegalKingMoves(board, from, piece, castlingRights);
            default: return Enumerable.Empty<Move>();
        }
    }
}