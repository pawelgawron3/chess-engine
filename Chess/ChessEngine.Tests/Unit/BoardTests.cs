using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Players;

namespace ChessEngine.Tests.Unit;

public class BoardTests
{
    [Fact]
    public void Clear_ShouldRemovePieces()
    {
        Board board = new Board();

        board[0, 0] = new Piece(PieceType.Rook, Player.White);
        board[7, 7] = new Piece(PieceType.King, Player.Black);

        board.Clear();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Assert.Null(board[row, col]);
            }
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllPieces()
    {
        Board board = new Board();
        board.Initialize();

        board.Clear();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Assert.Null(board[row, col]);
            }
        }
    }
}