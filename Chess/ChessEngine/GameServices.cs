using ChessEngine.Components;

namespace ChessEngine;

public class GameServices
{
    public RuleManager Rules { get; }
    public MoveHistoryManager History { get; }
    public ZobristHasher Hasher { get; }
    public GameResultEvaluator Evaluator { get; }
    public int HalfMoveClock { get; private set; } = 0;
    public int FullMoveCounter { get; private set; } = 1;

    private readonly GameState _state;

    public GameServices(GameState state)
    {
        _state = state;

        var castlingRights = new Dictionary<Player, (bool, bool, bool)>
        {
            {Player.White, (false, false, false)},
            {Player.Black, (false, false, false)},
        };

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

        var castlingBefore = Rules.CastlingRights;
        int? enPassantBefore = Rules.EnPassantFile;

        Rules.UpdateCastlingRights(movedPiece, move);
        _state.Board.MakeMove(move);
        Rules.UpdateEnPassantFile(move, movedPiece);

        var castlingAfter = Rules.CastlingRights;
        int? enPassantAfter = Rules.EnPassantFile;

        Hasher.ApplyMove(move, movedPiece, capturedPiece, enPassantBefore, enPassantAfter, castlingBefore, castlingAfter);

        bool opponentKingInCheck = AttackUtils.IsKingInCheck(_state.Board, _state.CurrentPlayer.Opponent());
        MoveRecord record = new MoveRecord(move, movedPiece, capturedPiece, HalfMoveClock, move.PromotionPiece, opponentKingInCheck);

        History.Add(record);
        _state.RaiseMoveMade(record);

        UpdateClocks(movedPiece, capturedPiece);
        SwitchPlayer();
        _state.ClearSelection();

        _state.GameResult = Evaluator.Evaluate(Hasher.CurrentHash, Hasher.PositionCounts, HalfMoveClock);
        _state.RaiseGameEnded(_state.GameResult);

        return true;
    }

    public void UndoMove()
    {
        MoveRecord? last = History.Undo(_state.Board);
        if (last == null) return;

        HalfMoveClock = last.HalfMoveClockBefore;
        SwitchPlayer();
        _state.GameResult = null;
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
}