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

        this[move.To] = piece.Clone();
        this[move.From] = null;
    }
}