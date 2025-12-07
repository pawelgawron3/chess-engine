namespace ChessEngine.Game;

public interface ICommand
{
    void Execute();

    void Undo();
}