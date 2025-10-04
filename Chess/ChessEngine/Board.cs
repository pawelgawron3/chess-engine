namespace ChessEngine;

public class Board
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];
    public Player CurrentPlayer { get; set; } = Player.White;

    public Piece? this[int row, int col]
    {
        get => (row >= 0 && row < 8 && col >= 0 && col < 8) ? _squares[row, col] : null;
        set
        {
            if (row >= 0 && row < 8 && col >= 0 && col < 8)
            {
                _squares[row, col] = value;
            }
        }
    }

    public Piece? this[Position pos]
    {
        get => pos.IsValid ? this[pos.Row, pos.Column] : null;
        set { if (pos.IsValid) this[pos.Row, pos.Column] = value; }
    }

    public void Initialize()
    {
        // Pawns
        for (int col = 0; col < 8; col++)
        {
            this[6, col] = new Piece(PieceType.Pawn, Player.White);
            this[1, col] = new Piece(PieceType.Pawn, Player.Black);
        }

        PieceType[] majorPieceOrder = new[]
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

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
        this[move.To] = piece;
        this[move.From] = null;
        CurrentPlayer = CurrentPlayer.Opponent();
        return true;
    }

    public bool IsMoveLegal(Move move)
    {
        if (!move.From.IsValid || !move.To.IsValid)
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