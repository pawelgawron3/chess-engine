namespace ChessEngine;

public class Board
{
    public Piece?[,] Squares { get; } = new Piece?[8, 8];
    public Player CurrentPlayer { get; set; } = Player.White;
}