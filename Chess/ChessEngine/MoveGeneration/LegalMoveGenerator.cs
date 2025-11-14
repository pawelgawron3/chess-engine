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
    public static IEnumerable<Move> GenerateLegalMovesForPiece(GameState state)
    {
        if (state.SelectedPosition == null)
            yield break;

        Position from = state.SelectedPosition.Value;
        foreach (var move in GeneratePseudoLegalMovesForPiece(state, from))
        {
            if (IsMoveLegal(state, move))
                yield return move;
        }
    }

    /// <summary>
    /// Generates all legal moves for the current player.
    /// </summary>
    public static IEnumerable<Move> GenerateLegalMoves(GameState state)
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
    public static bool IsMoveLegal(GameState state, Move move)
    {
        Piece movedPiece = state.Board[move.From]!;
        Piece? capturedPiece = GetCapturedPiece(state.Board, move);

        state.Board.MakeMove(move);
        bool kingInCheck = IsKingInCheck(state.Board, state.CurrentPlayer);
        state.Board.UndoMove(move, movedPiece, capturedPiece);

        return !kingInCheck;
    }
}