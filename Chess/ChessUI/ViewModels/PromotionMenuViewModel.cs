using System.ComponentModel;
using System.Windows.Input;
using ChessEngine;
using ChessEngine.Chessboard;
using ChessUI.Commands;

namespace ChessUI.ViewModels;

public class PromotionMenuViewModel : INotifyPropertyChanged
{
    private Player _currentPlayer;

    public Player CurrentPlayer
    {
        get => _currentPlayer;
        set
        {
            if (_currentPlayer != value)
            {
                _currentPlayer = value;
                Raise(nameof(CurrentPlayer));
                RefreshImagePaths();
            }
        }
    }

    public string? QueenImagePath { get; private set; }
    public string? RookImagePath { get; private set; }
    public string? BishopImagePath { get; private set; }
    public string? KnightImagePath { get; private set; }

    public ICommand PromotionCommand { get; init; }
    public Action<PieceType>? OnPieceSelected { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PromotionMenuViewModel()
    {
        PromotionCommand = new RelayCommand(param =>
        {
            if (param is string s && Enum.TryParse<PieceType>(s, out var type))
                OnPieceSelected?.Invoke(type);
        });
    }

    private void RefreshImagePaths()
    {
        string color = CurrentPlayer == Player.White ? "W" : "B";

        QueenImagePath = $"/Assets/Images/Queen{color}.png";
        RookImagePath = $"/Assets/Images/Rook{color}.png";
        BishopImagePath = $"/Assets/Images/Bishop{color}.png";
        KnightImagePath = $"/Assets/Images/Knight{color}.png";

        Raise(nameof(QueenImagePath));
        Raise(nameof(RookImagePath));
        Raise(nameof(BishopImagePath));
        Raise(nameof(KnightImagePath));
    }

    private void Raise(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}