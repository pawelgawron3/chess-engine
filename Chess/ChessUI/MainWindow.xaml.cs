using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ChessEngine;

namespace ChessUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private GameState _gameState = new GameState();

    public MainWindow()
    {
        InitializeComponent();
        DrawBoard();
    }

    private void DrawBoard()
    {
        PiecesLayer.Children.Clear();

        const int squareSize = 75;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = _gameState.Board[row, col];

                if (piece != null)
                {
                    var img = new Image
                    {
                        Width = squareSize,
                        Height = squareSize,
                        Source = new BitmapImage(new Uri($"/Assets/{piece.Type}{(piece.Owner == Player.White ? "W" : "B")}.png", UriKind.Relative))
                    };

                    Canvas.SetLeft(img, col * squareSize);
                    Canvas.SetTop(img, row * squareSize);

                    PiecesLayer.Children.Add(img);
                }
            }
        }
    }

    private void BoardGrid_MouseDown()
    {

    }
}