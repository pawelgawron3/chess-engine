using ChessEngine.Chessboard;
using ChessEngine.Game;
using static ChessEngine.Utils.AttackUtils;
using static ChessEngine.Utils.PositionUtils;

namespace ChessEngine.MoveGeneration;

public static class PseudoLegalMoveGenerator
{
    /// <summary>
    /// Generates all pseudo-legal moves for the currently selected piece.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMovesForPiece(GameStateEngine state, Position from)
    {
        Piece? piece = state.Board[from];
        if (piece == null)
            yield break;

        foreach (var move in GenerateMovesFor(state.Board, from, piece.Value, state.Services.Rules.EnPassantFile, state.Services.Rules.CastlingRights))
            yield return move;
    }

    /// <summary>
    /// Generates all pseudo-legal moves for the current player.
    /// </summary>
    public static IEnumerable<Move> GeneratePseudoLegalMoves(GameStateEngine state)
    {
        Player player = state.CurrentPlayer;
        var castlingRights = state.Services.Rules.CastlingRights;
        int? enPassantFile = state.Services.Rules.EnPassantFile;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = state.Board[row, col];
                if (piece == null || piece.Value.Owner != player) continue;

                Position from = new Position(row, col);
                foreach (var move in GenerateMovesFor(state.Board, from, piece.Value, enPassantFile, castlingRights))
                    yield return move;
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(Board board, Position pos, Piece piece, int? enPassantFile)
    {
        int direction = piece.Owner == Player.White ? -1 : 1;
        int promotionRow = piece.Owner == Player.White ? 0 : 7;

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

            int startRow = piece.Owner == Player.White ? 6 : 1;
            Position twoStep = new Position(pos.Row + 2 * direction, pos.Column);
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

            if (targetPiece != null && targetPiece.Value.Owner != piece.Owner)
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
            if (sidePawn?.Type == PieceType.Pawn && sidePawn.Value.Owner != piece.Owner)
            {
                if (IsInside(targetRow, targetCol) && board[targetRow, targetCol] == null)
                    yield return new Move(pos, new Position(targetRow, targetCol), MoveType.EnPassant);
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalKnightMoves(Board board, Position pos, Piece piece)
    {
        (int dr, int dc)[] jumps =
        {
            (-2, -1), (-2, 1), (-1, 2), (1, 2),
            (2, 1), (2, -1), (1, -2), (-1, -2)
        };

        foreach (var (dr, dc) in jumps)
        {
            int row = pos.Row + dr;
            int col = pos.Column + dc;

            if (!IsInside(row, col)) continue;

            Piece? targetPiece = board[row, col];

            if (targetPiece == null || targetPiece.Value.Owner != piece.Owner)
                yield return new Move(pos, new Position(row, col));
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalBishopMoves(Board board, Position pos, Piece piece)
    {
        (int dr, int dc)[] directions =
        {
            (-1, -1), (-1, 1),
            (1, -1), (1, 1)
        };

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalRookMoves(Board board, Position pos, Piece piece)
    {
        (int dr, int dc)[] directions =
        {
            (-1, 0), (1, 0),
            (0, -1), (0, 1)
        };

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalQueenMoves(Board board, Position pos, Piece piece)
    {
        (int dr, int dc)[] directions =
        {
            (-1, 0), (1, 0),
            (0, -1), (0, 1),
            (-1, -1), (-1 ,1),
            (1, -1), (1, 1),
        };

        return GenerateSlidingMoves(board, pos, piece, directions);
    }

    private static IEnumerable<Move> GeneratePseudoLegalKingMoves(Board board,
        Position pos,
        Piece piece,
        CastlingRights castlingRights)
    {
        (int dr, int dc)[] kingDirections =
        {
            (-1,0), (1,0), (0,-1), (0,1),
            (-1,-1), (-1,1), (1,-1), (1,1)
        };

        foreach (var (dr, dc) in kingDirections)
        {
            int r = pos.Row + dr;
            int c = pos.Column + dc;
            if (!IsInside(r, c)) continue;
            Piece? targetPiece = board[r, c];

            if (targetPiece == null || targetPiece.Value.Owner != piece.Owner)
                yield return new Move(pos, new Position(r, c));
        }

        int row = piece.Owner == Player.White ? 7 : 0;
        Player player = piece.Owner;

        var (KingMoved, RookAMoved, RookHMoved) = (player == Player.White) ? castlingRights.White : castlingRights.Black;

        if (!KingMoved)
        {
            if (!RookHMoved &&
                board[row, 5] == null && board[row, 6] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 5), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 6), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 6), MoveType.Castling);
            }

            if (!RookAMoved &&
                board[row, 1] == null && board[row, 2] == null && board[row, 3] == null &&
                !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 3), player.Opponent()) &&
                !IsSquareAttacked(board, new Position(row, 2), player.Opponent()))
            {
                yield return new Move(pos, new Position(row, 2), MoveType.Castling);
            }
        }
    }

    private static IEnumerable<Move> GenerateSlidingMoves(Board board, Position pos, Piece piece, (int, int)[] directions)
    {
        foreach (var (dr, dc) in directions)
        {
            int row = pos.Row + dr;
            int col = pos.Column + dc;

            while (IsInside(row, col))
            {
                Piece? targetPiece = board[row, col];
                if (targetPiece == null)
                {
                    yield return new Move(pos, new Position(row, col));
                }
                else
                {
                    if (targetPiece.Value.Owner != piece.Owner)
                    {
                        yield return new Move(pos, new Position(row, col));
                    }
                    break;
                }

                row += dr;
                col += dc;
            }
        }
    }

    private static IEnumerable<Move> GenerateMovesFor(Board board, Position from, Piece piece, int? enPassantFile, CastlingRights castlingRights)
        => piece.Type switch
        {
            PieceType.Pawn => GeneratePseudoLegalPawnMoves(board, from, piece, enPassantFile),
            PieceType.Knight => GeneratePseudoLegalKnightMoves(board, from, piece),
            PieceType.Bishop => GeneratePseudoLegalBishopMoves(board, from, piece),
            PieceType.Rook => GeneratePseudoLegalRookMoves(board, from, piece),
            PieceType.Queen => GeneratePseudoLegalQueenMoves(board, from, piece),
            PieceType.King => GeneratePseudoLegalKingMoves(board, from, piece, castlingRights),
            _ => Enumerable.Empty<Move>()
        };
}