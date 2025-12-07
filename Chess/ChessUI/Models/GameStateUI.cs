using ChessEngine;
using ChessEngine.Chessboard;
using ChessEngine.Game;

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
        var command = new MoveCommand(GameStateEngine, move);
        command.Execute();

        if (GameStateEngine.Services.History.MoveHistory.LastOrDefault()?.Move == move)
        {
            ClearSelection();
            RaiseMoveMade(GameStateEngine.Services.History.MoveHistory.Last());
            return true;
        }

        return false;
    }

    public void UndoMove()
    {
        MoveRecord? last = GameStateEngine.Services.History.Undo();
        if (last == null) return;

        var command = new MoveCommand(GameStateEngine, last.Move, last);
        command.Undo();

        RaiseMoveMade(last);
    }

    public void RedoMove()
    {
        MoveRecord? next = GameStateEngine.Services.History.Redo();
        if (next == null) return;

        GameStateEngine.Board.MakeMove(next.Move);

        if (next.MovedPiece.Type == PieceType.King)
        {
            if (next.MovedPiece.Owner == Player.White)
                GameStateEngine.WhiteKingPos = next.Move.To;
            else
                GameStateEngine.BlackKingPos = next.Move.To;
        }

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

        GameStateEngine.GameResult = GameStateEngine
                                    .Services
                                    .Evaluator
                                    .Evaluate(GameStateEngine.Services.Hasher.CurrentHash,
                                              GameStateEngine.Services.Hasher.PositionCounts,
                                              GameStateEngine.Services.HalfMoveClock
                                    );
        RaiseMoveMade(next);
    }
}