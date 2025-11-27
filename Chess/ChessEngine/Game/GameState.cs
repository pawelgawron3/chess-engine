using System.ComponentModel;
using ChessEngine.Chessboard;
using ChessEngine.Components;
using ChessEngine.MoveGeneration;

namespace ChessEngine.Game;

public class GameState : INotifyPropertyChanged
{
    private Player _currentPlayer = Player.White;
    public Board Board { get; }

    public Player CurrentPlayer
    {
        get => _currentPlayer;
        set
        {
            if (_currentPlayer != value)
            {
                _currentPlayer = value;
                OnPropertyChanged(nameof(CurrentPlayer));
            }
        }
    }

    public Position? SelectedPosition { get; private set; }
    public GameResult? GameResult { get; internal set; }
    public GameServices Services { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<MoveRecord>? OnMoveMade;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal void RaiseMoveMade(MoveRecord record) => OnMoveMade?.Invoke(record);

    public GameState()
    {
        Board = new Board();
        Board.Initialize();
        Services = new GameServices(this);
    }

    public IEnumerable<Move> GetLegalMoves() => LegalMoveGenerator.GenerateLegalMoves(this);

    public IEnumerable<Move> GetLegalMovesForPiece() =>
        SelectedPosition != null ? LegalMoveGenerator.GenerateLegalMovesForPiece(this) : Enumerable.Empty<Move>();

    public void SelectPosition(Position pos) => SelectedPosition = pos;

    public void ClearSelection() => SelectedPosition = null;

    public bool TryMakeMove(Move move) => Services.MakeMove(move);

    public void TryUndoMove() => Services.UndoMove();

    public void TryRedoMove() => Services.RedoMove();
}