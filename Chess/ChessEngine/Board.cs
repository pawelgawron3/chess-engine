using static ChessEngine.PositionUtils;

namespace ChessEngine;

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
        if (piece == null) return;

        switch (move.Type)
        {
            case MoveType.Normal:
                this[move.To] = piece.Clone();
                this[move.From] = null;
                break;

            case MoveType.EnPassant:
                this[move.From.Row, move.To.Column] = null;
                this[move.To] = piece.Clone();
                this[move.From] = null;
                break;

            case MoveType.Castling:
                int fromRow = move.From.Row;

                if (move.To.Column == 6)
                {
                    this[fromRow, 6] = piece.Clone();
                    this[fromRow, 5] = this[fromRow, 7]?.Clone();
                    this[fromRow, 7] = null;
                }
                else if (move.To.Column == 2)
                {
                    this[fromRow, 2] = piece.Clone();
                    this[fromRow, 3] = this[fromRow, 0]?.Clone();
                    this[fromRow, 0] = null;
                }

                this[move.From] = null;
                break;

            case MoveType.Promotion:
                this[move.To] = new Piece(move.PromotionPiece!.Value, piece.Owner);
                this[move.From] = null;
                break;
        }
    }

    /// <summary>
    /// Reverts a move previously made on the board, restoring the pieces to their original positions.
    /// </summary>
    public void UndoMove(MoveRecord last)
    {
        switch (last.Move.Type)
        {
            case MoveType.Normal:
                this[last.Move.From] = last.MovedPiece.Clone();
                this[last.Move.To] = last.CapturedPiece?.Clone();
                break;

            case MoveType.EnPassant:
                this[last.Move.From] = last.MovedPiece.Clone();
                this[last.Move.To] = null;
                this[last.Move.From.Row, last.Move.To.Column] = last.CapturedPiece!.Clone();
                break;

            case MoveType.Castling:
                int fromRow = last.Move.From.Row;
                int toCol = last.Move.To.Column;

                this[last.Move.From] = last.MovedPiece.Clone();
                this[last.Move.To] = null;

                if (toCol == 6)
                {
                    this[fromRow, 7] = this[fromRow, 5]?.Clone();
                    this[fromRow, 5] = null;
                }
                else if (toCol == 2)
                {
                    this[fromRow, 0] = this[fromRow, 3]?.Clone();
                    this[fromRow, 3] = null;
                }
                break;

            case MoveType.Promotion:
                this[last.Move.From] = last.MovedPiece?.Clone();
                this[last.Move.To] = last.CapturedPiece?.Clone();
                break;
        }
    }
}