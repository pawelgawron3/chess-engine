using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Game;
using ChessEngine.Game.Commands;

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

    public IEnumerable<Move> GetLegalMovesForPiece() =>
        SelectedPosition != null ? GameStateEngine.GetLegalMovesForPiece(SelectedPosition.Value) : Enumerable.Empty<Move>();

    public void SelectPosition(Position pos) => SelectedPosition = pos;

    public void ClearSelection() => SelectedPosition = null;

    public void MakeMove(Move move)
    {
        var command = new MoveCommand(GameStateEngine, move);
        command.Execute();

        ClearSelection();
        RaiseMoveMade(GameStateEngine.Services.History.MoveHistory.Last());
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

        var command = new MoveCommand(GameStateEngine, next.Move, next);
        command.Redo();

        RaiseMoveMade(next);
    }
}