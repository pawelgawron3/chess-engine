using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.MoveGeneration;
using ChessEngine.Utils;

namespace ChessEngine.Game;

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

        var initialRights = new CastlingRights((false, false, false), (false, false, false));

        Rules = new RuleManager(initialRights);
        History = new MoveHistoryManager();
        Hasher = new ZobristHasher(state.Board, () => state.CurrentPlayer, () => Rules.CastlingRights, () => Rules.EnPassantFile);
        Evaluator = new GameResultEvaluator(state);
    }

    public bool MakeMove(Move move)
    {
        if (_state.GameResult != null || !LegalMoveGenerator.IsMoveLegal(_state, move))
            return false;

        Piece movedPiece = _state.Board[move.From]!.Value;
        Piece? capturedPiece = AttackUtils.GetCapturedPiece(_state.Board, move);

        MoveRecord record = new MoveRecord(
            Move: move,
            MovedPiece: movedPiece,
            CapturedPiece: capturedPiece,
            HalfMoveClockBefore: HalfMoveClock,
            FullMoveCounterBefore: FullMoveCounter,
            HalfMoveClockAfter: 0,                          // updated after the move
            FullMoveCounterAfter: 0,                        // updated after the move
            PreviousHash: Hasher.CurrentHash,
            HashAfter: 0,                                   // updated after the move
            CastlingRightsBefore: Rules.CastlingRights,
            CastlingRightsAfter: default,                   // updated after the move
            PromotedPieceType: move.PromotionPiece,
            KingInCheck: false,                             // updated after the move
            EnPassantFileBefore: Rules.EnPassantFile,
            EnPassantFileAfter: null                        // updated after the move
        );

        Rules.UpdateCastlingRights(movedPiece, move);
        _state.Board.MakeMove(move);
        Rules.UpdateEnPassantFile(move, movedPiece);

        Hasher.ApplyMove(move, movedPiece, capturedPiece,
                         record.EnPassantFileBefore,
                         Rules.EnPassantFile,
                         record.CastlingRightsBefore,
                         Rules.CastlingRights);

        UpdateClocks(movedPiece, capturedPiece);
        SwitchPlayer();
        _state.GameResult = Evaluator.Evaluate(Hasher.CurrentHash, Hasher.PositionCounts, HalfMoveClock);

        record = record with
        {
            HalfMoveClockAfter = HalfMoveClock,
            FullMoveCounterAfter = FullMoveCounter,
            HashAfter = Hasher.CurrentHash,
            CastlingRightsAfter = Rules.CastlingRights,
            KingInCheck = AttackUtils.IsKingInCheck(_state.Board, _state.CurrentPlayer),
            EnPassantFileAfter = Rules.EnPassantFile,
        };

        History.Add(record);

        if (!SimulationMode)
        {
            _state.ClearSelection();
            _state.RaiseMoveMade(record);
        }

        return true;
    }

    public void UndoMove()
    {
        MoveRecord? last = History.Undo();
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

        _state.GameResult = null;
        if (!SimulationMode)
        {
            _state.RaiseMoveMade(last);
        }
    }

    public void RedoMove()
    {
        MoveRecord? next = History.Redo();
        if (next == null) return;

        _state.Board.MakeMove(next.Move);

        Hasher.CurrentHash = next.HashAfter;
        Rules.CastlingRights = next.CastlingRightsAfter;
        Rules.EnPassantFile = next.EnPassantFileAfter;

        if (!Hasher.PositionCounts.ContainsKey(Hasher.CurrentHash))
            Hasher.PositionCounts.Add(Hasher.CurrentHash, 1);
        else
            Hasher.PositionCounts[Hasher.CurrentHash]++;

        HalfMoveClock = next.HalfMoveClockAfter;
        FullMoveCounter = next.FullMoveCounterAfter;
        SwitchPlayer();

        _state.GameResult = Evaluator.Evaluate(Hasher.CurrentHash, Hasher.PositionCounts, HalfMoveClock);
        _state.RaiseMoveMade(next);
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