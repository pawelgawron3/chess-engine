using ChessEngine.Chessboard;
using ChessEngine.Game;
using static ChessEngine.MoveGeneration.PseudoLegalMoveGenerator;
using static ChessEngine.Utils.AttackUtils;

namespace ChessEngine.MoveGeneration;

public static class LegalMoveGenerator
{
    /// <summary>
    /// Generates all legal moves for the currently selected piece.
    /// </summary>
    public static IEnumerable<Move> GenerateLegalMovesForPiece(GameStateEngine state, Position pos)
    {
        foreach (var move in GeneratePseudoLegalMovesForPiece(state, pos))
        {
            if (IsMoveLegal(state, move))
                yield return move;
        }
    }

    /// <summary>
    /// Generates all legal moves for the current player.
    /// </summary>
    public static IEnumerable<Move> GenerateLegalMoves(GameStateEngine state)
    {
        foreach (var move in GeneratePseudoLegalMoves(state))
        {
            if (IsMoveLegal(state, move))
                yield return move;
        }
    }

    /// <summary>
    /// Tests if a move is legal without modifying the actual game history.
    /// </summary>
    public static bool IsMoveLegal(GameStateEngine state, Move move)
    {
        Piece movedPiece = state.Board[move.From]!.Value;
        Piece? capturedPiece = GetCapturedPiece(state.Board, move);

        if (movedPiece.Type == PieceType.King)
        {
            if (movedPiece.Owner == Player.White)
                state.WhiteKingPos = move.To;
            else
                state.BlackKingPos = move.To;
        }

        state.Board.MakeMove(move);
        bool kingInCheck = IsKingInCheck(state, state.CurrentPlayer);
        state.Board.UndoMove(move, movedPiece, capturedPiece);

        if (movedPiece.Type == PieceType.King)
        {
            if (movedPiece.Owner == Player.White)
                state.WhiteKingPos = move.From;
            else
                state.BlackKingPos = move.From;
        }

        return !kingInCheck;
    }
}