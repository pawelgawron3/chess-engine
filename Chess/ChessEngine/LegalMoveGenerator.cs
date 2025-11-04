using static ChessEngine.AttackUtils;
using static ChessEngine.PositionUtils;
using static ChessEngine.PseudoLegalMoveGenerator;

namespace ChessEngine;

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
        if (!IsMovePseudoLegal(state, move))
            return false;

        Piece movedPiece = state.Board[move.From]!;
        Piece? capturedPiece = GetCapturedPiece(state.Board, move);

        state.Board.MakeMove(move);
        bool kingInCheck = IsKingInCheck(state.Board, state.CurrentPlayer);
        state.Board.UndoMove(move, movedPiece, capturedPiece);

        return !kingInCheck;
    }

    /// <summary>
    /// Checks whether a given move is pseudo-legal on the board.
    /// </summary>
    private static bool IsMovePseudoLegal(GameState state, Move move)
    {
        if (!IsInside(move.From) || !IsInside(move.To))
            return false;

        Piece? piece = state.Board[move.From];
        if (piece?.Owner != state.CurrentPlayer)
            return false;

        Piece? targetPiece = state.Board[move.To];
        if (targetPiece?.Owner == state.CurrentPlayer)
            return false;

        if (!GeneratePseudoLegalMovesForPiece(state, move.From)
            .Any(m => m.To.Equals(move.To)))
            return false;

        return true;
    }
}