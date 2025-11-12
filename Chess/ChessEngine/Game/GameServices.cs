using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.Game;
using ChessEngine.MoveGeneration;
using ChessEngine.Utils;

namespace ChessEngine;

public class GameServices
{
    public RuleManager Rules { get; }
    public MoveHistoryManager History { get; }
    public ZobristHasher Hasher { get; }
    public GameResultEvaluator Evaluator { get; }
    public int HalfMoveClock { get; private set; } = 0;
    public int FullMoveCounter { get; private set; } = 1;
    public bool SimulationMode { get; set; } = false;

    private readonly GameState _state;

    public GameServices(GameState state)
    {
        _state = state;

        var castlingRights = new CastlingRights((false, false, false), (false, false, false));

        Rules = new RuleManager(castlingRights);
        History = new MoveHistoryManager();
        Hasher = new ZobristHasher(state.Board, () => state.CurrentPlayer, () => castlingRights, () => Rules.EnPassantFile);
        Evaluator = new GameResultEvaluator(state);
    }

    public bool MakeMove(Move move)
    {
        if (_state.GameResult != null || !LegalMoveGenerator.IsMoveLegal(_state, move))
            return false;

        Piece movedPiece = _state.Board[move.From]!;
        Piece? capturedPiece = AttackUtils.GetCapturedPiece(_state.Board, move);

        CastlingRights castlingBefore = Rules.CastlingRights.Clone();
        int? enPassantBefore = Rules.EnPassantFile;
        ulong previousHash = Hasher.CurrentHash;

        Rules.UpdateCastlingRights(movedPiece, move);
        _state.Board.MakeMove(move);
        Rules.UpdateEnPassantFile(move, movedPiece);

        var castlingAfter = Rules.CastlingRights.Clone();
        int? enPassantAfter = Rules.EnPassantFile;

        Hasher.ApplyMove(move, movedPiece, capturedPiece, enPassantBefore, enPassantAfter, castlingBefore, castlingAfter);

        bool opponentKingInCheck = AttackUtils.IsKingInCheck(_state.Board, _state.CurrentPlayer.Opponent());
        MoveRecord record = new MoveRecord(
            move,
            movedPiece,
            capturedPiece,
            HalfMoveClock,
            previousHash,
            castlingBefore,
            move.PromotionPiece,
            opponentKingInCheck,
            enPassantBefore
        );

        History.Add(record);
        UpdateClocks(movedPiece, capturedPiece);
        SwitchPlayer();

        if (!SimulationMode)
        {
            _state.ClearSelection();
            _state.RaiseMoveMade(record);
            _state.GameResult = Evaluator.Evaluate(Hasher.CurrentHash, Hasher.PositionCounts, HalfMoveClock);
            _state.RaiseGameEnded(_state.GameResult);
        }

        return true;
    }

    public void UndoMove()
    {
        MoveRecord? last = History.Undo(_state.Board);
        if (last == null) return;

        _state.Board.UndoMove(last.Move, last.MovedPiece, last.CapturedPiece);

        if (Hasher.PositionCounts.ContainsKey(Hasher.CurrentHash))
        {
            Hasher.PositionCounts[Hasher.CurrentHash]--;
            if (Hasher.PositionCounts[Hasher.CurrentHash] <= 0)
                Hasher.PositionCounts.Remove(Hasher.CurrentHash);
        }

        Hasher.CurrentHash = last.PreviousHash;
        Rules.CastlingRights = last.CastlingRightsBefore;
        Rules.EnPassantFile = last.EnPassantFileBefore;

        RevertClocks(last.HalfMoveClockBefore);
        SwitchPlayer();

        if (!SimulationMode)
        {
            _state.GameResult = null;
            _state.RaiseMoveMade(last);
        }
    }

    private void SwitchPlayer() => _state.CurrentPlayer = _state.CurrentPlayer.Opponent();

    private void UpdateClocks(Piece movedPiece, Piece? capturedPiece)
    {
        HalfMoveClock = (movedPiece.Type == PieceType.Pawn || capturedPiece != null)
            ? 0
            : HalfMoveClock + 1;

        if (_state.CurrentPlayer == Player.Black)
            FullMoveCounter++;
    }

    private void RevertClocks(int halfMoveClockBefore)
    {
        HalfMoveClock = halfMoveClockBefore;

        if (_state.CurrentPlayer == Player.White && FullMoveCounter >= 1)
            FullMoveCounter--;
    }
}