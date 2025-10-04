namespace ChessEngine;

public class Board
{
    public Piece?[,] Squares { get; } = new Piece?[8, 8];
    public Player CurrentPlayer { get; set; } = Player.White;

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