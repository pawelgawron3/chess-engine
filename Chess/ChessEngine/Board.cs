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
}