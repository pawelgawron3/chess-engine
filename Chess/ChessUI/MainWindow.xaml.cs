using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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

    private Move _pendingPromotionMove;
    private bool _isAwaitingPromotion = false;

    public MainWindow()
    {
        InitializeComponent();
        DrawBoard();
        SetCursor(_gameState.CurrentPlayer);
        PromotionMenu.PieceSelected += OnPromotionPieceSelected;
        _gameState.MoveMade += OnMoveMade;
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
                        Source = new BitmapImage(new Uri($"/Assets/Images/{piece.Type}{(piece.Owner == Player.White ? "W" : "B")}.png", UriKind.Relative))
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
        if (_isAwaitingPromotion)
            return;

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

        HighlightSelectedSquare();

        var moves = _gameState.GetLegalMovesForPiece()
            .GroupBy(m => m.To)
            .Select(g => g.First())
            .ToList();

        const int squareSize = 75;

        foreach (var move in moves)
        {
            Piece? targetPiece = _gameState.Board[move.To];

            if (targetPiece == null)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new SolidColorBrush(Color.FromArgb(140, 120, 120, 120)),
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 8,
                        Color = Colors.Black,
                        Opacity = 0.3,
                        ShadowDepth = 0
                    },
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(ellipse, move.To.Column * squareSize + (squareSize - ellipse.Width) / 2);
                Canvas.SetTop(ellipse, move.To.Row * squareSize + (squareSize - ellipse.Height) / 2);

                HighlightLayer.Children.Add(ellipse);
            }
            else
            {
                Ellipse ring = new Ellipse
                {
                    Width = squareSize - 6,
                    Height = squareSize - 6,
                    Stroke = new SolidColorBrush(Color.FromArgb(180, 120, 120, 120)),
                    StrokeThickness = 6,
                    Fill = Brushes.Transparent,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 8,
                        Color = Colors.Black,
                        Opacity = 0.35,
                        ShadowDepth = 0
                    },
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(ring, move.To.Column * squareSize + 2);
                Canvas.SetTop(ring, move.To.Row * squareSize + 2);

                HighlightLayer.Children.Add(ring);
            }
        }
    }

    private void ClearHighlights()
    {
        HighlightLayer.Children.Clear();
    }

    private void HighlightSelectedSquare()
    {
        if (_gameState.SelectedPosition == null) return;

        const int squareSize = 75;
        Position pos = _gameState.SelectedPosition.Value;

        Rectangle rect = new Rectangle()
        {
            Width = squareSize,
            Height = squareSize,
            Fill = new SolidColorBrush(Color.FromArgb(90, 0, 255, 0)),
            IsHitTestVisible = false
        };

        Canvas.SetLeft(rect, pos.Column * squareSize);
        Canvas.SetTop(rect, pos.Row * squareSize);

        HighlightLayer.Children.Add(rect);
    }

    private void HandleBoardClick(Position pos)
    {
        if (_gameState.SelectedPosition == null)
        {
            TrySelectPiece(pos);
        }
        else
        {
            TryMakeMove(pos);
        }
    }

    private void TrySelectPiece(Position pos)
    {
        Piece? piece = _gameState.Board[pos];
        if (piece != null && piece.Owner == _gameState.CurrentPlayer)
        {
            _gameState.SelectPosition(pos);
            HighlightMovesForSelectedPiece();
        }
    }

    private void TryMakeMove(Position pos)
    {
        Move move = _gameState.GetLegalMovesForPiece()
               .FirstOrDefault(m => m.To.Equals(pos));

        if (move.Equals(default(Move)))
        {
            _gameState.ClearSelection();
            ClearHighlights();
            return;
        }

        if (move.Type == MoveType.Promotion)
        {
            _pendingPromotionMove = move;
            _isAwaitingPromotion = true;
            Piece piece = _gameState.Board[move.From]!;
            PromotionMenu.ShowForPlayer(piece.Owner);
            return;
        }

        if (_gameState.TryMakeMove(move))
        {
            ClearHighlights();
            DrawBoard();
            SetCursor(_gameState.CurrentPlayer);
        }
        else
        {
            _gameState.ClearSelection();
            ClearHighlights();
        }
    }

    private void SetCursor(Player player)
    {
        Cursor = (player == Player.White) ? ChessCursors.White : ChessCursors.Black;
    }

    private void OnPromotionPieceSelected(PieceType selectedPiece)
    {
        Move promotionMove = new Move(
            _pendingPromotionMove.From,
            _pendingPromotionMove.To,
            MoveType.Promotion,
            selectedPiece
        );

        if (_gameState.TryMakeMove(promotionMove))
        {
            _isAwaitingPromotion = false;
            ClearHighlights();
            DrawBoard();
            SetCursor(_gameState.CurrentPlayer);
        }

        PromotionMenu.Visibility = Visibility.Collapsed;
    }

    private void OnMoveMade(Move move, Piece? captured)
    {
        ChessSounds.PlaySoundForMove(move, captured);

        UpdateGameInfo(move);
    }

    private void UpdateGameInfo(Move move)
    {
        MoveCountText.Text = _gameState.FullMoveCounter.ToString();

        if (_gameState.MoveHistory.Count > 0)
            LastMoveText.Text = AttackUtils.ReturnChessNotation(_gameState.MoveHistory.Last());
        else
            LastMoveText.Text = "-";
    }
}