namespace ChessEngine;

public class Board
{
    public Piece?[,] Squares { get; } = new Piece?[8, 8];
    public Player CurrentPlayer { get; set; } = Player.White;

    public void Initialize()
    {
        // Pawns
        for (int col = 0; col < 8; col++)
        {
            Squares[6, col] = new Piece(PieceType.Pawn, Player.White);
            Squares[1, col] = new Piece(PieceType.Pawn, Player.Black);
        }

        PieceType[] majorPieceOrder = new[]
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        for (int col = 0; col < 8; col++)
        {
            Squares[7, col] = new Piece(majorPieceOrder[col], Player.White);
            Squares[0, col] = new Piece(majorPieceOrder[col], Player.Black);
        }
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