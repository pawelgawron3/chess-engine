using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

    private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Point point = e.GetPosition(BoardGrid);
        Position pos = GetBoardPositionFromClick(point);

        HandleBoardClick(pos);
    }

    private Position GetBoardPositionFromClick(Point point)
    {
        const int squareSize = 75;
        int col = (int)(point.X / squareSize);
        int row = (int)(point.Y / squareSize);
        return new Position(row, col);
    }

    private void HighlightMovesForSelectedPiece()
    {
        ClearHighlights();
        if (_gameState.SelectedPosition == null) return;

        var moves = _gameState.GetPseudoLegalMovesForPiece().ToList();

        const int squareSize = 75;

        foreach (var move in moves)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = Brushes.Yellow,
                Opacity = 0.3,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(ellipse, move.To.Column * squareSize + (squareSize - 40) / 2);
            Canvas.SetTop(ellipse, move.To.Row * squareSize + (squareSize - 40) / 2);

            HighlightLayer.Children.Add(ellipse);
        }
    }

    private void ClearHighlights()
    {
        HighlightLayer.Children.Clear();
    }

    private void HandleBoardClick(Position pos)
    {
        if (_gameState.SelectedPosition == null)
        {
            Piece? piece = _gameState.Board[pos];
            if (piece != null && piece.Owner == _gameState.CurrentPlayer)
            {
                _gameState.SelectPosition(pos);
                HighlightMovesForSelectedPiece();
            }
        }
        else
        {
            Move move = new Move(_gameState.SelectedPosition.Value, pos);
            if (_gameState.TryMakeMove(move))
            {
                ClearHighlights();
                DrawBoard();
            }
            else
            {
                _gameState.ClearSelection();
                ClearHighlights();
            }
        }
    }
}