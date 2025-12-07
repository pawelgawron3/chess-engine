using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.Utils;

namespace ChessEngine.Game;

public class GameServices
{
    public RuleManager Rules { get; }
    public MoveHistoryManager History { get; }
    public ZobristHasher Hasher { get; }
    public GameResultEvaluator Evaluator { get; }
    public int HalfMoveClock { get; set; } = 0;
    public int FullMoveCounter { get; set; } = 1;

    private readonly GameStateEngine _state;

    public GameServices(GameStateEngine state)
    {
        _state = state;

        var initialRights = new CastlingRights((false, false, false), (false, false, false));

        Rules = new RuleManager(initialRights);
        History = new MoveHistoryManager();
        Hasher = new ZobristHasher(state.Board, () => state.CurrentPlayer, () => Rules.CastlingRights, () => Rules.EnPassantFile);
        Evaluator = new GameResultEvaluator(state);
    }

    public void EngineMakeMove(Move move, out MoveStruct undo)
    {
        undo = new MoveStruct
        {
            MovedPiece = _state.Board[move.From]!.Value,
            CapturedPiece = AttackUtils.GetCapturedPiece(_state.Board, move),
            HalfMoveClockBefore = HalfMoveClock,
            HashBefore = Hasher.CurrentHash,
            CastlingRightsBefore = Rules.CastlingRights,
            PromotedPieceType = move.PromotionPiece,
            EnPassantFileBefore = Rules.EnPassantFile,
        };

        Rules.UpdateCastlingRights(undo.MovedPiece, move);
        if (undo.MovedPiece.Type == PieceType.King)
        {
            if (undo.MovedPiece.Owner == Player.White)
                _state.WhiteKingPos = move.To;
            else
                _state.BlackKingPos = move.To;
        }
        _state.Board.MakeMove(move);
        Rules.UpdateEnPassantFile(move, undo.MovedPiece);

        UpdateClocks(undo.MovedPiece, undo.CapturedPiece);

        Hasher.ApplyMove(
            move,
            undo.MovedPiece,
            undo.CapturedPiece,
            undo.EnPassantFileBefore,
            Rules.EnPassantFile,
            undo.CastlingRightsBefore,
            Rules.CastlingRights
        );

        SwitchPlayer();
        _state.GameResult = Evaluator.Evaluate(Hasher.CurrentHash, Hasher.PositionCounts, HalfMoveClock);
    }

    public void EngineUndoMove(Move move, MoveStruct undo)
    {
        _state.Board.UndoMove(move, undo.MovedPiece, undo.CapturedPiece);

        if (undo.MovedPiece.Type == PieceType.King)
        {
            if (undo.MovedPiece.Owner == Player.White)
                _state.WhiteKingPos = move.From;
            else
                _state.BlackKingPos = move.From;
        }

        if (Hasher.PositionCounts.ContainsKey(Hasher.CurrentHash))
        {
            Hasher.PositionCounts[Hasher.CurrentHash]--;
            if (Hasher.PositionCounts[Hasher.CurrentHash] <= 0)
                Hasher.PositionCounts.Remove(Hasher.CurrentHash);
        }

        Hasher.CurrentHash = undo.HashBefore;
        Rules.CastlingRights = undo.CastlingRightsBefore;
        Rules.EnPassantFile = undo.EnPassantFileBefore;

        RevertClocks(undo.HalfMoveClockBefore);
        SwitchPlayer();
        _state.GameResult = null;
    }

    public void SwitchPlayer() => _state.CurrentPlayer = _state.CurrentPlayer.Opponent();

    public void UpdateClocks(Piece movedPiece, Piece? capturedPiece)
    {
        HalfMoveClock = (movedPiece.Type == PieceType.Pawn || capturedPiece != null)
            ? 0
            : HalfMoveClock + 1;

        if (_state.CurrentPlayer == Player.Black)
            FullMoveCounter++;
    }

    public void RevertClocks(int halfMoveClockBefore)
    {
        HalfMoveClock = halfMoveClockBefore;

        if (_state.CurrentPlayer == Player.White && FullMoveCounter >= 1)
            FullMoveCounter--;
    }
}