using System.Windows;
using System.Windows.Input;
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
    }

    private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Point pt = e.GetPosition(BoardGrid);
        _vm.OnBoardClick(pt);
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