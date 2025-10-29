namespace ChessEngine.Components;

public class MoveHistoryManager
{
    public List<MoveRecord> MoveHistory { get; } = new();

    public void Add(MoveRecord record) => MoveHistory.Add(record);

    public MoveRecord? Undo(Board board)
    {
        if (MoveHistory.Count == 0)
            return null;

        MoveRecord? last = MoveHistory[^1];
        MoveHistory.RemoveAt(MoveHistory.Count - 1);
        board.UndoMove(last);
        return last;
    }
}