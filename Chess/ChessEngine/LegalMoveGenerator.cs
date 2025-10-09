using static ChessEngine.AttackUtils;
using static ChessEngine.PositionUtils;
using static ChessEngine.PseudoLegalMoveGenerator;

namespace ChessEngine;

public static class LegalMoveGenerator
{
    /// <summary>
    /// Generates all legal moves for the currently selected piece in the provided board state.
    /// </summary>
    public static IEnumerable<Move> GenerateLegalMovesForPiece(Board board, Position from, Player player)
    {
        foreach (var move in GeneratePseudoLegalMovesForPiece(board, from))
        {
            if (IsMoveLegal(board, move, player))
                yield return move;
        }
    }

    /// <summary>
    /// Generates all legal moves for a given player in the provided board state.
    /// </summary>
    public static IEnumerable<Move> GenerateLegalMoves(Board board, Player player)
    {
        foreach (var move in GeneratePseudoLegalMoves(board, player))
        {
            if (IsMoveLegal(board, move, player))
                yield return move;
        }
    }

    /// <summary>
    /// Tests if a move is legal without modifying the actual game history.
    /// </summary>
    public static bool IsMoveLegal(Board board, Move move, Player player)
    {
        if (!IsMovePseudoLegal(board, move, player))
            return false;

        Piece? movedPiece = board[move.From];
        Piece? capturedPiece = board[move.To];

        board.MakeMove(move);

        Position kingPosition = GetKingPosition(board, player);
        bool isKingInCheck = IsSquareAttacked(board, kingPosition, player.Opponent());

        board[move.From] = movedPiece;
        board[move.To] = capturedPiece;

        return !isKingInCheck;
    }

    /// <summary>
    /// Checks whether a given move is pseudo-legal on the board.
    /// </summary>
    private static bool IsMovePseudoLegal(Board board, Move move, Player player)
    {
        if (!IsInside(move.From) || !IsInside(move.To))
            return false;

        Piece? piece = board[move.From];
        if (piece?.Owner != player)
            return false;

        Piece? targetPiece = board[move.To];
        if (targetPiece?.Owner == player)
            return false;

        if (!GeneratePseudoLegalMovesForPiece(board, move.From)
            .Any(m => m.To.Equals(move.To)))
            return false;

        return true;
    }
}