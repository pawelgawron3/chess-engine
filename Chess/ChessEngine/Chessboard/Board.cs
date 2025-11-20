using static ChessEngine.Utils.PositionUtils;

namespace ChessEngine.Chessboard;

/// <summary>
/// Represents the chess board and manages the state of all squares and pieces.
/// Provides methods for initialization and updating piece positions during the game.
/// </summary>
public class Board
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    /// <summary>
    /// Accesses or modifies a piece at the specified row and column on the board.
    /// Returns <c>null</c> if the coordinates are out of bounds.
    /// </summary>
    public Piece? this[int row, int col]
    {
        get => IsInside(row, col) ? _squares[row, col] : null;
        set
        {
            if (IsInside(row, col))
            {
                _squares[row, col] = value;
            }
        }
    }

    /// <summary>
    /// Accesses or modifies a piece at the specified row and column on the board.
    /// Returns <c>null</c> if the coordinates are out of bounds.
    /// </summary>
    public Piece? this[Position pos]
    {
        get => IsInside(pos) ? this[pos.Row, pos.Column] : null;
        set { if (IsInside(pos)) this[pos.Row, pos.Column] = value; }
    }

    /// <summary>
    /// Initializes the board to the standard starting position for a chess game.
    /// </summary>
    public void Initialize()
    {
        // Pawns
        for (int col = 0; col < 8; col++)
        {
            this[6, col] = new Piece(PieceType.Pawn, Player.White);
            this[1, col] = new Piece(PieceType.Pawn, Player.Black);
        }

        PieceType[] majorPieceOrder =
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        // Major pieces
        for (int col = 0; col < 8; col++)
        {
            this[7, col] = new Piece(majorPieceOrder[col], Player.White);
            this[0, col] = new Piece(majorPieceOrder[col], Player.Black);
        }
    }

    /// <summary>
    /// Executes a move on the board by transferring a piece from the source to the destination square.
    /// </summary>
    public void MakeMove(Move move)
    {
        Piece? piece = this[move.From];
        if (!piece.HasValue) return;

        switch (move.Type)
        {
            case MoveType.Normal:
                this[move.To] = piece;
                this[move.From] = null;
                break;

            case MoveType.EnPassant:
                this[move.From.Row, move.To.Column] = null;
                this[move.To] = piece;
                this[move.From] = null;
                break;

            case MoveType.Castling:
                int fromRow = move.From.Row;

                if (move.To.Column == 6)
                {
                    this[fromRow, 6] = piece;
                    this[fromRow, 5] = this[fromRow, 7];
                    this[fromRow, 7] = null;
                }
                else if (move.To.Column == 2)
                {
                    this[fromRow, 2] = piece;
                    this[fromRow, 3] = this[fromRow, 0];
                    this[fromRow, 0] = null;
                }

                this[move.From] = null;
                break;

            case MoveType.Promotion:
                this[move.To] = new Piece(move.PromotionPiece!.Value, piece.Value.Owner);
                this[move.From] = null;
                break;
        }
    }

    /// <summary>
    /// Reverts a move previously made on the board, restoring the pieces to their original positions.
    /// </summary>
    public void UndoMove(Move move, Piece movedPiece, Piece? capturedPiece)
    {
        switch (move.Type)
        {
            case MoveType.Normal:
                this[move.From] = movedPiece;
                this[move.To] = capturedPiece;
                break;

            case MoveType.EnPassant:
                this[move.From] = movedPiece;
                this[move.To] = null;
                this[move.From.Row, move.To.Column] = capturedPiece;
                break;

            case MoveType.Castling:
                int fromRow = move.From.Row;
                int toCol = move.To.Column;

                this[move.From] = movedPiece;
                this[move.To] = null;

                if (toCol == 6)
                {
                    this[fromRow, 7] = this[fromRow, 5];
                    this[fromRow, 5] = null;
                }
                else if (toCol == 2)
                {
                    this[fromRow, 0] = this[fromRow, 3];
                    this[fromRow, 3] = null;
                }
                break;

            case MoveType.Promotion:
                this[move.From] = movedPiece;
                this[move.To] = capturedPiece;
                break;
        }
    }

    public IEnumerable<(Piece piece, Position pos)> GetAllPiecesWithPosition()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (this[row, col] is Piece piece)
                    yield return (piece, new Position(row, col));
            }
        }
    }
}