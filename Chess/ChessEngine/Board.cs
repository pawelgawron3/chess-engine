using static ChessEngine.PositionUtils;

namespace ChessEngine;

public class Board
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];
    public Player CurrentPlayer { get; private set; } = Player.White;

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

    public Piece? this[Position pos]
    {
        get => IsInside(pos) ? this[pos.Row, pos.Column] : null;
        set { if (IsInside(pos)) this[pos.Row, pos.Column] = value; }
    }

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

    public bool MakeMove(Move move)
    {
        if (!IsMoveLegal(move)) return false;

        Piece? piece = this[move.From];
        if (piece != null)
        {
            this[move.To] = piece.Clone();
            this[move.From] = null;
            CurrentPlayer = CurrentPlayer.Opponent();
            return true;
        }
        return false;
    }

    public bool IsMoveLegal(Move move)
    {
        if (!IsInside(move.From) || !IsInside(move.To))
        {
            return false;
        }

        Piece? piece = this[move.From];
        if (piece == null)
        {
            return false;
        }

        if (piece.Owner != CurrentPlayer)
        {
            return false;
        }

        Piece? targetPiece = this[move.To];

        if (targetPiece != null && targetPiece.Owner == CurrentPlayer)
        {
            return false;
        }

        return true;
    }
}