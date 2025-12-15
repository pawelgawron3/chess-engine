using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Players;
using ChessEngine.Evaluation.Tables;
using ChessEngine.Game;

namespace ChessEngine.Evaluation;

public class Evaluator : IEvaluationFunction
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

    private int GetPieceSquareValue(Piece piece, Position pos, bool isEndgame)
    {
        int index = pos.Row * 8 + pos.Column;
        if (piece.Owner == Player.Black)
            index = 63 - index;

        return piece.Type switch
        {
            PieceType.Pawn => PieceSquareTables.Pawn[index],
            PieceType.Knight => PieceSquareTables.Knight[index],
            PieceType.Bishop => PieceSquareTables.Bishop[index],
            PieceType.Rook => PieceSquareTables.Rook[index],
            PieceType.Queen => PieceSquareTables.Queen[index],
            PieceType.King => PieceSquareTables.King[isEndgame ? 1 : 0, index],
            _ => 0
        };
    }

    private bool IsEndgame(Board board)
    {
        bool whiteHasQueen = false;
        bool blackHasQueen = false;
        int whiteMinorPieces = 0;
        int blackMinorPieces = 0;

        foreach (var (piece, _) in board.GetAllPiecesWithPosition())
        {
            switch (piece.Type)
            {
                case PieceType.Queen:
                    if (piece.Owner == Player.White) whiteHasQueen = true;
                    else blackHasQueen = true;
                    break;

                case PieceType.Bishop:
                case PieceType.Knight:
                    if (piece.Owner == Player.White) whiteMinorPieces++;
                    else blackMinorPieces++;
                    break;
            }
        }

        bool whiteEndgame = !whiteHasQueen || whiteHasQueen && whiteMinorPieces <= 1;
        bool blackEndgame = !blackHasQueen || blackHasQueen && blackMinorPieces <= 1;

        return whiteEndgame && blackEndgame;
    }
}