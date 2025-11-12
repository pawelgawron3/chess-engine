using ChessEngine.Chessboard;

namespace ChessEngine.Utils;

public static class UI_Utils
{
    public static string ReturnChessNotation(MoveRecord moveRecord)
    {
        Move move = moveRecord.Move;
        Piece movedPiece = moveRecord.MovedPiece;
        PieceType pieceType = movedPiece.Type;

        if (move.Type == MoveType.Castling)
        {
            return move.To.Column == 6 ? "O-O" : "O-O-O";
        }

        string pieceLetter = GetPieceLetter(pieceType);

        bool isCapture = moveRecord.CapturedPiece != null;
        string captureSign = isCapture ? "x" : "";

        string fromFile = ((char)('a' + move.From.Column)).ToString();
        string toSquare = ReturnAlgebraic(move.To);

        string notation;

        if (pieceType == PieceType.Pawn)
        {
            if (isCapture)
                notation = $"{fromFile}{captureSign}{toSquare}";
            else
                notation = $"{toSquare}";
        }
        else
        {
            notation = $"{pieceLetter}{captureSign}{toSquare}";
        }

        if (moveRecord.PromotedPieceType is PieceType promoted)
            notation += $"={GetPieceLetter(promoted)}";

        if (moveRecord.KingInCheck)
            notation += "+";

        return notation;
    }

    private static string ReturnAlgebraic(Position pos, bool isPawn = false)
    {
        if (!isPawn)
        {
            char file = (char)('a' + pos.Column);
            int rank = 8 - pos.Row;
            return $"{file}{rank}";
        }
        else
        {
            return $"{(char)('a' + pos.Column)}";
        }
    }

    private static string GetPieceLetter(PieceType type) => type switch
    {
        PieceType.Queen => "Q",
        PieceType.Rook => "R",
        PieceType.Bishop => "B",
        PieceType.Knight => "N",
        _ => ""
    };
}