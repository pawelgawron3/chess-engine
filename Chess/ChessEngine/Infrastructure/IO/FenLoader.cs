using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Players;
using ChessEngine.Game;

namespace ChessEngine.Infrastructure.IO;

public static class FenLoader
{
    public static void LoadFen(GameStateEngine state, string fen)
    {
        string[] parts = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string board = parts[0];
        string sideToMove = parts[1];
        string castling = parts[2];
        string enPassant = parts[3];
        int halfMoveClock = parts.Length > 4 ? int.Parse(parts[4]) : 0;
        int fullMoveNumber = parts.Length > 5 ? int.Parse(parts[5]) : 1;

        int row = 0;
        int col = 0;

        state.Board.Clear();

        foreach (char c in board)
        {
            if (c == '/')
            {
                row++;
                col = 0;
            }
            else if (char.IsDigit(c))
            {
                col += c - '0';
            }
            else
            {
                Player owner = char.IsUpper(c) ? Player.White : Player.Black;
                PieceType type = char.ToLower(c) switch
                {
                    'p' => PieceType.Pawn,
                    'n' => PieceType.Knight,
                    'b' => PieceType.Bishop,
                    'r' => PieceType.Rook,
                    'q' => PieceType.Queen,
                    'k' => PieceType.King,
                    _ => throw new Exception("Unknown piece in FEN")
                };

                state.Board[row, col] = new Piece(type, owner);
                col++;
            }
        }

        state.CurrentPlayer = sideToMove == "w" ? Player.White : Player.Black;

        var rights = new CastlingRights(
            white: (!castling.Contains('K'), !castling.Contains('Q'), !castling.Contains('K')
        ),
            black: (!castling.Contains('k'), !castling.Contains('q'), !castling.Contains('k')
        )
        );

        state.Services.Rules.CastlingRights = rights;

        if (enPassant == "-")
            state.Services.Rules.EnPassantFile = null;
        else
            state.Services.Rules.EnPassantFile = enPassant[0] - 'a';

        state.Services.HalfMoveClock = halfMoveClock;
        state.Services.FullMoveCounter = fullMoveNumber;

        state.Services.Hasher.ComputeZobristHash();
    }
}