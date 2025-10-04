namespace ChessEngine;

public class Board
{
    public Piece?[,] Squares { get; } = new Piece?[8, 8];
    public Player CurrentPlayer { get; set; } = Player.White;

    public void Initialize()
    {
        // White pawns
        for (int col = 0; col < 8; col++)
        {
            Squares[6, col] = new Piece(PieceType.Pawn, Player.White);
        }

        // Black pawns
        for (int col = 0; col < 8; col++)
        {
            Squares[1, col] = new Piece(PieceType.Pawn, Player.Black);
        }

        // Rooks
        Squares[7, 0] = new Piece(PieceType.Rook, Player.White);
        Squares[7, 7] = new Piece(PieceType.Rook, Player.White);
        Squares[0, 0] = new Piece(PieceType.Rook, Player.Black);
        Squares[0, 7] = new Piece(PieceType.Rook, Player.Black);

        // Knights
        Squares[7, 1] = new Piece(PieceType.Knight, Player.White);
        Squares[7, 6] = new Piece(PieceType.Knight, Player.White);
        Squares[0, 1] = new Piece(PieceType.Knight, Player.Black);
        Squares[0, 6] = new Piece(PieceType.Knight, Player.Black);

        // Bishops
        Squares[7, 2] = new Piece(PieceType.Bishop, Player.White);
        Squares[7, 5] = new Piece(PieceType.Bishop, Player.White);
        Squares[0, 2] = new Piece(PieceType.Bishop, Player.Black);
        Squares[0, 5] = new Piece(PieceType.Bishop, Player.Black);

        // Queens
        Squares[7, 3] = new Piece(PieceType.Queen, Player.White);
        Squares[0, 3] = new Piece(PieceType.Queen, Player.Black);

        // Kings
        Squares[7, 4] = new Piece(PieceType.King, Player.White);
        Squares[0, 4] = new Piece(PieceType.King, Player.Black);
    }

    public void MakeMove(Move move)
    {
        Piece? piece = Squares[move.From.Row, move.From.Column];
        Squares[move.To.Row, move.To.Column] = piece;
        Squares[move.From.Row, move.From.Column] = null;
        CurrentPlayer = CurrentPlayer.Opponent();
    }

    public bool IsMoveLegal(Move move)
    {
        if (!move.From.IsValid || !move.To.IsValid)
        {
            return false;
        }

        Piece? piece = Squares[move.From.Row, move.From.Column];
        if (piece == null)
        {
            return false;
        }

        if (piece.Owner != CurrentPlayer)
        {
            return false;
        }

        Piece? targetPiece = Squares[move.To.Row, move.To.Column];

        if (targetPiece != null && targetPiece.Owner == CurrentPlayer)
        {
            return false;
        }

        return true;
    }
}