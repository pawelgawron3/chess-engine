using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;
using ChessEngine.Game;
using static ChessEngine.Core.Rules.AttackUtils;
using static ChessEngine.Infrastructure.Utils.PositionUtils;

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

                foreach (var move in GenerateMovesFor(state.Board, new Position(row, col), piece.Value, enPassantFile, castlingRights))
                    yield return move;
            }
        }
    }

    private static IEnumerable<Move> GeneratePseudoLegalPawnMoves(Board board, Position pos, Piece piece, int? enPassantFile)
    {
        int direction = piece.Owner == Player.White ? -1 : 1;
        int promotionRow = piece.Owner == Player.White ? 0 : 7;

        Position oneStep = new(pos.Row + direction, pos.Column);
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
            Position twoStep = new(pos.Row + 2 * direction, pos.Column);
            if (pos.Row == startRow && board[twoStep] == null)
                yield return new Move(pos, twoStep);
        }

        Position[] diagonals =
        {
            new(pos.Row + direction, pos.Column - 1),
            new(pos.Row + direction, pos.Column + 1)
        };

        foreach (var target in diagonals)
        {
            if (!IsInside(target)) continue;
            Piece? targetPiece = board[target];

            if (targetPiece != null && targetPiece.Value.Owner != piece.Owner)
            {
                if (target.Row == promotionRow)
                {
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Queen, true);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Rook, true);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Bishop, true);
                    yield return new Move(pos, target, MoveType.Promotion, PieceType.Knight, true);
                }
                else
                {
                    yield return new Move(pos, target, MoveType.Capture, null, true);
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
                    yield return new Move(pos, new Position(targetRow, targetCol), MoveType.EnPassant, null, true);
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

            if (targetPiece == null)
                yield return new Move(pos, new Position(row, col));
            else if (targetPiece.Value.Owner != piece.Owner)
                yield return new Move(pos, new Position(row, col), MoveType.Capture, null, true);
        }
    }

    private static readonly (int dr, int dc)[] _bishopDirections =
    {
        (-1, -1), (-1, 1),
        (1, -1), (1, 1)
    };

    private static readonly (int dr, int dc)[] _rookDirections =
    {
        (-1, 0), (1, 0),
        (0, -1), (0, 1)
    };

    private static readonly (int dr, int dc)[] _queenDirections =
    {
        (-1, 0), (1, 0),
        (0, -1), (0, 1),
        (-1, -1), (-1 ,1),
        (1, -1), (1, 1)
    };

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

            if (targetPiece == null)
                yield return new Move(pos, new Position(r, c));
            else if (targetPiece.Value.Owner != piece.Owner)
                yield return new Move(pos, new Position(r, c), MoveType.Capture, null, true);
        }

        int row = piece.Owner == Player.White ? 7 : 0;
        Player player = piece.Owner;

        bool canCastleKingSide = (player == Player.White)
            ? castlingRights.HasFlag(CastlingRights.WhiteKing)
            : castlingRights.HasFlag(CastlingRights.BlackKing);

        bool canCastleQueenSide = (player == Player.White)
                ? castlingRights.HasFlag(CastlingRights.WhiteQueen)
                : castlingRights.HasFlag(CastlingRights.BlackQueen);

        if (canCastleKingSide &&
            board[row, 5] == null && board[row, 6] == null &&
            !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
            !IsSquareAttacked(board, new Position(row, 5), player.Opponent()) &&
            !IsSquareAttacked(board, new Position(row, 6), player.Opponent()))
        {
            yield return new Move(pos, new Position(row, 6), MoveType.Castling);
        }

        if (canCastleQueenSide &&
            board[row, 1] == null && board[row, 2] == null && board[row, 3] == null &&
            !IsSquareAttacked(board, new Position(row, 4), player.Opponent()) &&
            !IsSquareAttacked(board, new Position(row, 3), player.Opponent()) &&
            !IsSquareAttacked(board, new Position(row, 2), player.Opponent()))
        {
            yield return new Move(pos, new Position(row, 2), MoveType.Castling);
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
                        yield return new Move(pos, new Position(row, col), MoveType.Capture, null, true);
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
            PieceType.Bishop => GenerateSlidingMoves(board, from, piece, _bishopDirections),
            PieceType.Rook => GenerateSlidingMoves(board, from, piece, _rookDirections),
            PieceType.Queen => GenerateSlidingMoves(board, from, piece, _queenDirections),
            PieceType.King => GeneratePseudoLegalKingMoves(board, from, piece, castlingRights),
            _ => Enumerable.Empty<Move>()
        };
}