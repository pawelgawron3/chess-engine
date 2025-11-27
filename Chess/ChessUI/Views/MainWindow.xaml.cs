using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ChessEngine;
using ChessEngine.Game;
using ChessUI.Services;
using ChessUI.ViewModels;

namespace ChessUI.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = (MainViewModel)DataContext;

        Cursor = ChessCursors.White;
        _vm.GameState.PropertyChanged += OnGameStatePropertyChanged;
    }

    private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Point pt = e.GetPosition(BoardGrid);
        _vm.OnBoardClick(pt);
    }

    private void OnGameStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameState.CurrentPlayer))
        {
            Player player = _vm.GameState.CurrentPlayer;

            Cursor = (player == Player.White) ? ChessCursors.White : ChessCursors.Black;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Left)
        {
            _vm.UndoCommand.Execute(null);
        }
        else if (e.Key == Key.Right)
        {
            _vm.RedoCommand.Execute(null);
        }
    }
}