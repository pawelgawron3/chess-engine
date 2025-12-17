using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Players;
using ChessEngine.Evaluation.Tables;
using ChessEngine.Game;

namespace ChessEngine.Evaluation;

public sealed class Evaluator : IEvaluationFunction
{
    public int Evaluate(GameStateEngine state)
    {
        int score = 0;
        bool isEndgame = IsEndgame(state.Board);

        foreach (var (piece, pos) in state.Board.GetAllPiecesWithPosition())
        {
            int baseValue = PieceValues.Value[(int)piece.Type];
            int positionalBonus = GetPieceSquareValue(piece, pos, isEndgame);

            score += (int)piece.Owner * (baseValue + positionalBonus);
        }

        return score;
    }

    private static int GetPieceSquareValue(Piece piece, Position pos, bool isEndgame)
    {
        int index = pos.Row * 8 + pos.Column;
        if (piece.Owner == Player.Black)
            index = 63 - index;

        if (piece.Type != PieceType.King)
            return PieceTables[(int)piece.Type][index];

        return PieceSquareTables.King[isEndgame ? 1 : 0, index];
    }

    private static readonly int[][] PieceTables =
    {
        PieceSquareTables.Pawn,
        PieceSquareTables.Knight,
        PieceSquareTables.Bishop,
        PieceSquareTables.Rook,
        PieceSquareTables.Queen
    };

    private static bool IsEndgame(Board board)
    {
        bool whiteHasQueen = false;
        bool blackHasQueen = false;
        int whiteMinors = 0;
        int blackMinors = 0;

        foreach (var (piece, _) in board.GetAllPiecesWithPosition())
        {
            if (piece.Type == PieceType.Queen)
            {
                if (piece.Owner == Player.White) whiteHasQueen = true;
                else blackHasQueen = true;
            }
            else if (piece.Type == PieceType.Bishop || piece.Type == PieceType.Knight)
            {
                if (piece.Owner == Player.White) whiteMinors++;
                else blackMinors++;
            }

            if (whiteHasQueen && blackHasQueen &&
                whiteMinors > 1 && blackMinors > 1)
                return false;
        }

        return (!whiteHasQueen || whiteMinors <= 1)
            && (!blackHasQueen || blackMinors <= 1);
    }
}