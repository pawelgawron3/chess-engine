namespace ChessEngine.Game.Commands;

public interface ICommand
{
    void Execute();

    void Undo();
}