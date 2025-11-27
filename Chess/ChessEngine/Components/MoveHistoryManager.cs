using ChessEngine.Chessboard;

namespace ChessEngine.Components;

public class MoveHistoryManager
{
    public List<MoveRecord> MoveHistory { get; } = new();
    public Stack<MoveRecord> RedoStack { get; } = new();

    public void Add(MoveRecord record)
    {
        MoveHistory.Add(record);
        RedoStack.Clear();
    }

    public MoveRecord? Undo()
    {
        if (MoveHistory.Count == 0)
            return null;

        MoveRecord last = MoveHistory[^1];
        MoveHistory.RemoveAt(MoveHistory.Count - 1);
        RedoStack.Push(last);
        return last;
    }

    public MoveRecord? Redo()
    {
        if (RedoStack.Count == 0)
            return null;

        MoveRecord next = RedoStack.Pop();
        MoveHistory.Add(next);
        return next;
    }
}