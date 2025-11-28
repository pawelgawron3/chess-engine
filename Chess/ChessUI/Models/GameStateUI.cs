using ChessEngine.Chessboard;
using ChessEngine.Game;
using ChessEngine.MoveGeneration;
using ChessEngine.Utils;

namespace ChessUI.Models;

public class GameStateUI
{
    public GameStateEngine GameStateEngine { get; }

    public Position? SelectedPosition { get; private set; }

    public event Action<MoveRecord>? OnMoveMade;

    internal void RaiseMoveMade(MoveRecord record) => OnMoveMade?.Invoke(record);

    public GameStateUI()
    {
        GameStateEngine = new GameStateEngine();
    }

    public IEnumerable<Move> GetLegalMoves() => GameStateEngine.GetLegalMoves();

    public IEnumerable<Move> GetLegalMovesForPiece() =>
        SelectedPosition != null ? GameStateEngine.GetLegalMovesForPiece(SelectedPosition.Value) : Enumerable.Empty<Move>();

    public void SelectPosition(Position pos) => SelectedPosition = pos;

    public void ClearSelection() => SelectedPosition = null;

    public bool TryMakeMove(Move move)
    {
        if (GameStateEngine.GameResult != null || !LegalMoveGenerator.IsMoveLegal(GameStateEngine, move))
            return false;

        Piece movedPiece = GameStateEngine.Board[move.From]!.Value;
        Piece? capturedPiece = AttackUtils.GetCapturedPiece(GameStateEngine.Board, move);

        MoveRecord record = new MoveRecord(
            Move: move,
            MovedPiece: movedPiece,
            CapturedPiece: capturedPiece,
            HalfMoveClockBefore: GameStateEngine.Services.HalfMoveClock,
            FullMoveCounterBefore: GameStateEngine.Services.FullMoveCounter,
            HalfMoveClockAfter: 0,
            FullMoveCounterAfter: 0,
            PreviousHash: GameStateEngine.Services.Hasher.CurrentHash,
            HashAfter: 0,
            CastlingRightsBefore: GameStateEngine.Services.Rules.CastlingRights,
            CastlingRightsAfter: default,
            PromotedPieceType: move.PromotionPiece,
            KingInCheck: false,
            EnPassantFileBefore: GameStateEngine.Services.Rules.EnPassantFile,
            EnPassantFileAfter: null
        );

        GameStateEngine.Services.Rules.UpdateCastlingRights(movedPiece, move);
        GameStateEngine.Board.MakeMove(move);
        GameStateEngine.Services.Rules.UpdateEnPassantFile(move, movedPiece);

        GameStateEngine.Services.Hasher.ApplyMove(move, movedPiece, capturedPiece,
                         record.EnPassantFileBefore,
                         GameStateEngine.Services.Rules.EnPassantFile,
                         record.CastlingRightsBefore,
                         GameStateEngine.Services.Rules.CastlingRights);

        GameStateEngine.Services.UpdateClocks(movedPiece, capturedPiece);
        GameStateEngine.Services.SwitchPlayer();
        GameStateEngine.GameResult = GameStateEngine.Services.Evaluator.Evaluate(GameStateEngine.Services.Hasher.CurrentHash,
                                                                                 GameStateEngine.Services.Hasher.PositionCounts,
                                                                                 GameStateEngine.Services.HalfMoveClock
        );

        record = record with
        {
            HalfMoveClockAfter = GameStateEngine.Services.HalfMoveClock,
            FullMoveCounterAfter = GameStateEngine.Services.FullMoveCounter,
            HashAfter = GameStateEngine.Services.Hasher.CurrentHash,
            CastlingRightsAfter = GameStateEngine.Services.Rules.CastlingRights,
            KingInCheck = AttackUtils.IsKingInCheck(GameStateEngine.Board, GameStateEngine.CurrentPlayer),
            EnPassantFileAfter = GameStateEngine.Services.Rules.EnPassantFile,
        };

        GameStateEngine.Services.History.Add(record);

        ClearSelection();
        RaiseMoveMade(record);

        return true;
    }

    public void UndoMove()
    {
        MoveRecord? last = GameStateEngine.Services.History.Undo();
        if (last == null) return;

        GameStateEngine.Board.UndoMove(last.Move, last.MovedPiece, last.CapturedPiece);

        if (GameStateEngine.Services.Hasher.PositionCounts.ContainsKey(GameStateEngine.Services.Hasher.CurrentHash))
        {
            GameStateEngine.Services.Hasher.PositionCounts[GameStateEngine.Services.Hasher.CurrentHash]--;
            if (GameStateEngine.Services.Hasher.PositionCounts[GameStateEngine.Services.Hasher.CurrentHash] <= 0)
                GameStateEngine.Services.Hasher.PositionCounts.Remove(GameStateEngine.Services.Hasher.CurrentHash);
        }

        GameStateEngine.Services.Hasher.CurrentHash = last.PreviousHash;
        GameStateEngine.Services.Rules.CastlingRights = last.CastlingRightsBefore;
        GameStateEngine.Services.Rules.EnPassantFile = last.EnPassantFileBefore;

        GameStateEngine.Services.RevertClocks(last.HalfMoveClockBefore);
        GameStateEngine.Services.SwitchPlayer();

        GameStateEngine.GameResult = null;

        RaiseMoveMade(last);
    }

    public void RedoMove()
    {
        MoveRecord? next = GameStateEngine.Services.History.Redo();
        if (next == null) return;

        GameStateEngine.Board.MakeMove(next.Move);

        GameStateEngine.Services.Hasher.CurrentHash = next.HashAfter;
        GameStateEngine.Services.Rules.CastlingRights = next.CastlingRightsAfter;
        GameStateEngine.Services.Rules.EnPassantFile = next.EnPassantFileAfter;

        if (!GameStateEngine.Services.Hasher.PositionCounts.ContainsKey(GameStateEngine.Services.Hasher.CurrentHash))
            GameStateEngine.Services.Hasher.PositionCounts.Add(GameStateEngine.Services.Hasher.CurrentHash, 1);
        else
            GameStateEngine.Services.Hasher.PositionCounts[GameStateEngine.Services.Hasher.CurrentHash]++;

        GameStateEngine.Services.HalfMoveClock = next.HalfMoveClockAfter;
        GameStateEngine.Services.FullMoveCounter = next.FullMoveCounterAfter;

        GameStateEngine.Services.SwitchPlayer();

        GameStateEngine.GameResult = GameStateEngine.Services.Evaluator.Evaluate(GameStateEngine.Services.Hasher.CurrentHash, GameStateEngine.Services.Hasher.PositionCounts, GameStateEngine.Services.HalfMoveClock);
        RaiseMoveMade(next);
    }
}