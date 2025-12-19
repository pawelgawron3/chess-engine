using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;
using ChessEngine.Core.Rules;

namespace ChessEngine.Game.Commands;

public class MoveCommand : ICommand
{
    private readonly GameStateEngine _state;
    private readonly Move _move;
    private MoveRecord? _record;

    public MoveCommand(GameStateEngine state, Move move, MoveRecord? record = null)
    {
        _state = state;
        _move = move;
        _record = record;
    }

    public void Execute()
    {
        Piece movedPiece = _state.Board[_move.From]!.Value;
        Piece? capturedPiece = AttackUtils.GetCapturedPiece(_state.Board, _move);

        _record = new MoveRecord(
            Move: _move,
            MovedPiece: movedPiece,
            CapturedPiece: capturedPiece,
            HalfMoveClockBefore: _state.Services.HalfMoveClock,
            FullMoveCounterBefore: _state.Services.FullMoveCounter,
            HalfMoveClockAfter: 0,
            FullMoveCounterAfter: 0,
            PreviousHash: _state.Services.Hasher.CurrentHash,
            HashAfter: 0,
            CastlingRightsBefore: _state.Services.Rules.CastlingRights,
            CastlingRightsAfter: default,
            PromotedPieceType: _move.PromotionPiece,
            KingInCheck: false,
            IsCheckmate: false,
            EnPassantFileBefore: _state.Services.Rules.EnPassantFile,
            EnPassantFileAfter: null
        );

        _state.Services.Rules.UpdateCastlingRights(movedPiece, _move);
        if (movedPiece.Type == PieceType.King)
        {
            if (movedPiece.Owner == Player.White)
                _state.WhiteKingPos = _move.To;
            else
                _state.BlackKingPos = _move.To;
        }
        _state.Board.MakeMove(_move);
        _state.Services.Rules.UpdateEnPassantFile(_move, movedPiece);

        _state.Services.Hasher.ApplyMove(
            _move,
            movedPiece,
            capturedPiece,
            _record.EnPassantFileBefore,
            _state.Services.Rules.EnPassantFile,
            _record.CastlingRightsBefore,
            _state.Services.Rules.CastlingRights
        );

        _state.Services.UpdateClocks(movedPiece, capturedPiece);
        _state.Services.SwitchPlayer();

        _state.GameResult = _state.Services.Evaluator.Evaluate(
            _state.Services.Hasher.CurrentHash,
            _state.Services.Hasher.PositionCounts,
            _state.Services.HalfMoveClock
        );

        _record = _record with
        {
            HalfMoveClockAfter = _state.Services.HalfMoveClock,
            FullMoveCounterAfter = _state.Services.FullMoveCounter,
            HashAfter = _state.Services.Hasher.CurrentHash,
            CastlingRightsAfter = _state.Services.Rules.CastlingRights,
            KingInCheck = AttackUtils.IsKingInCheck(_state, _state.CurrentPlayer),
            IsCheckmate = _state.GameResult?.Reason == GameEndReason.Checkmate,
            EnPassantFileAfter = _state.Services.Rules.EnPassantFile,
        };

        _state.Services.History.Add(_record);
    }

    public void Undo()
    {
        if (_record == null) return;

        _state.Board.UndoMove(_record.Move, _record.MovedPiece, _record.CapturedPiece);

        if (_record.MovedPiece.Type == PieceType.King)
        {
            if (_record.MovedPiece.Owner == Player.White)
                _state.WhiteKingPos = _move.From;
            else
                _state.BlackKingPos = _move.From;
        }

        if (_state.Services.Hasher.PositionCounts.ContainsKey(_state.Services.Hasher.CurrentHash))
        {
            _state.Services.Hasher.PositionCounts[_state.Services.Hasher.CurrentHash]--;
            if (_state.Services.Hasher.PositionCounts[_state.Services.Hasher.CurrentHash] <= 0)
                _state.Services.Hasher.PositionCounts.Remove(_state.Services.Hasher.CurrentHash);
        }

        _state.Services.Hasher.CurrentHash = _record.PreviousHash;
        _state.Services.Rules.CastlingRights = _record.CastlingRightsBefore;
        _state.Services.Rules.EnPassantFile = _record.EnPassantFileBefore;

        _state.Services.RevertClocks(_record.HalfMoveClockBefore);
        _state.Services.SwitchPlayer();

        _state.GameResult = null;
    }

    public void Redo()
    {
        if (_record == null) return;

        _state.Board.MakeMove(_record.Move);

        if (_record.MovedPiece.Type == PieceType.King)
        {
            if (_record.MovedPiece.Owner == Player.White)
                _state.WhiteKingPos = _record.Move.To;
            else
                _state.BlackKingPos = _record.Move.To;
        }

        _state.Services.Hasher.CurrentHash = _record.HashAfter;
        _state.Services.Rules.CastlingRights = _record.CastlingRightsAfter;
        _state.Services.Rules.EnPassantFile = _record.EnPassantFileAfter;

        if (!_state.Services.Hasher.PositionCounts.ContainsKey(_state.Services.Hasher.CurrentHash))
            _state.Services.Hasher.PositionCounts.Add(_state.Services.Hasher.CurrentHash, 1);
        else
            _state.Services.Hasher.PositionCounts[_state.Services.Hasher.CurrentHash]++;

        _state.Services.HalfMoveClock = _record.HalfMoveClockAfter;
        _state.Services.FullMoveCounter = _record.FullMoveCounterAfter;

        _state.Services.SwitchPlayer();

        _state.GameResult = _state
                                  .Services
                                  .Evaluator
                                  .Evaluate(_state.Services.Hasher.CurrentHash,
                                            _state.Services.Hasher.PositionCounts,
                                            _state.Services.HalfMoveClock
                                  );
    }
}