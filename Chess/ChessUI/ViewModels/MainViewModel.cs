using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine;
using ChessEngine.AI;
using ChessEngine.Chessboard;
using ChessEngine.Game;
using ChessUI.Commands;
using ChessUI.Helpers;
using ChessUI.Services;

namespace ChessUI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly GameState _gameState = new GameState();
    private Move _pendingPromotionMove;
    private bool _isAwaitingPromotion = false;
    private bool _isPromotionVisible = false;
    private string _moveCountText = "0";
    private string _lastMoveText = "—";
    private string _gameStatusText = "Game in progress...";
    private const int _squareSize = 75;

    public GameState GameState => _gameState;

    public bool IsPromotionVisible
    {
        get => _isPromotionVisible;
        set { _isPromotionVisible = value; Raise(nameof(IsPromotionVisible)); }
    }

    public string GameStatusText
    {
        get => _gameStatusText;
        set { _gameStatusText = value; Raise(nameof(GameStatusText)); }
    }

    public string LastMoveText
    {
        get => _lastMoveText;
        set { _lastMoveText = value; Raise(nameof(LastMoveText)); }
    }

    public string MoveCountText
    {
        get => _moveCountText;
        set { _moveCountText = value; Raise(nameof(MoveCountText)); }
    }

    public ObservableCollection<PieceViewModel> Pieces { get; } = new();
    public ObservableCollection<HighlightViewModel> Highlights { get; } = new();

    public PromotionMenuViewModel PromotionVM { get; } = new PromotionMenuViewModel();

    public ICommand AiMoveCommand { get; }

    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        AiMoveCommand = new RelayCommand(_ => DoAiMove(), _ => true);
        UndoCommand = new RelayCommand(_ => { _gameState.TryUndoMove(); RefreshUI(); }, _ => true);
        RedoCommand = new RelayCommand(_ => { _gameState.TryRedoMove(); RefreshUI(); }, _ => true);

        PromotionVM.OnPieceSelected = OnPromotionPieceSelected;
        _gameState.OnMoveMade += OnMoveMade;

        RefreshUI();
    }

    public void OnBoardClick(Point point)
    {
        if (_isAwaitingPromotion) return;

        Position pos = GetBoardPositionFromClick(point);
        HandleBoardClick(pos);
    }

    private void DrawBoard()
    {
        Pieces.Clear();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = _gameState.Board[row, col];
                if (piece == null) continue;

                string suffix = (piece.Value.Owner == Player.White) ? "W" : "B";
                string name = $"{piece.Value.Type}{suffix}.png";
                var uri = new Uri($"/Assets/Images/{name}", UriKind.Relative);
                var bmp = new BitmapImage(uri);

                var vm = new PieceViewModel
                {
                    Image = bmp,
                    Left = col * _squareSize,
                    Top = row * _squareSize,
                    Size = _squareSize
                };

                Pieces.Add(vm);
            }
        }
    }

    private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static Position GetBoardPositionFromClick(Point point)
    {
        int col = (int)(point.X / _squareSize);
        int row = (int)(point.Y / _squareSize);
        return new Position(row, col);
    }

    private void HandleBoardClick(Position pos)
    {
        if (_gameState.SelectedPosition == null)
            TrySelectPiece(pos);
        else
            TryMakeMove(pos);
    }

    private void TrySelectPiece(Position pos)
    {
        Piece? piece = _gameState.Board[pos];
        if (piece != null && piece.Value.Owner == _gameState.CurrentPlayer)
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
            ShowPromotionForPlayer(_gameState.CurrentPlayer);
            return;
        }

        if (_gameState.TryMakeMove(move))
            RefreshUI();
        else
        {
            _gameState.ClearSelection();
            ClearHighlights();
        }
    }

    private void HighlightMovesForSelectedPiece()
    {
        ClearHighlights();
        HighlightSelectedSquare();

        var moves = _gameState.GetLegalMovesForPiece()
                              .GroupBy(m => m.To)
                              .Select(g => g.First())
                              .ToList();

        foreach (var move in moves)
        {
            Piece? targetPiece = _gameState.Board[move.To];

            if (targetPiece == null)
            {
                var ellipse = new HighlightViewModel
                {
                    Size = 30,
                    Left = move.To.Column * _squareSize + (_squareSize - 30) / 2.0,
                    Top = move.To.Row * _squareSize + (_squareSize - 30) / 2.0,
                    FillBrush = new SolidColorBrush(Color.FromArgb(140, 120, 120, 120)),
                };

                Highlights.Add(ellipse);
            }
            else
            {
                var ring = new HighlightViewModel
                {
                    Size = _squareSize - 6,
                    Left = move.To.Column * _squareSize + 2,
                    Top = move.To.Row * _squareSize + 2,
                    StrokeBrush = new SolidColorBrush(Color.FromArgb(180, 120, 120, 120)),
                    StrokeThickness = 6,
                    FillBrush = Brushes.Transparent,
                };

                Highlights.Add(ring);
            }
        }
    }

    private void HighlightSelectedSquare()
    {
        if (_gameState.SelectedPosition == null) return;

        Position pos = _gameState.SelectedPosition.Value;

        var ellipse = new HighlightViewModel
        {
            Size = _squareSize,
            Left = pos.Column * _squareSize,
            Top = pos.Row * _squareSize,
            FillBrush = new SolidColorBrush(Color.FromArgb(90, 0, 255, 0)),
        };

        Highlights.Add(ellipse);
    }

    private void ShowPromotionForPlayer(Player player)
    {
        PromotionVM.CurrentPlayer = player;
        IsPromotionVisible = true;
    }

    private void HidePromotionMenu()
    {
        IsPromotionVisible = false;
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
            RefreshUI();
        }

        HidePromotionMenu();
    }

    private void ClearHighlights() => Highlights.Clear();

    private void UpdateGameInfo()
    {
        if (_gameState.CurrentPlayer == Player.Black)
            MoveCountText = _gameState.Services.FullMoveCounter.ToString();

        if (_gameState.Services.History.MoveHistory.Count > 0)
            LastMoveText = MoveNotationFormatter.ReturnChessNotation(_gameState.Services.History.MoveHistory.Last());
        else
        {
            LastMoveText = "-";
            MoveCountText = "0";
        }

        GameStatusText = _gameState.Services.Evaluator.ToDisplayString(_gameState.GameResult);
    }

    private void OnMoveMade(MoveRecord lastMove)
    {
        ChessSounds.PlaySoundForMove(lastMove.Move, lastMove.CapturedPiece, lastMove.KingInCheck);
        UpdateGameInfo();
    }

    private void RefreshUI()
    {
        DrawBoard();
        ClearHighlights();
        UpdateGameInfo();
    }

    private async void DoAiMove()
    {
        int depth = 6;

        Evaluator evaluator = new Evaluator();
        Negamax engine = new Negamax(evaluator);

        var (bestMove, score) = await Task.Run(() => engine.Search(_gameState, depth));
        if (bestMove != null)
        {
            _gameState.TryMakeMove(bestMove.Value);
            RefreshUI();
        }
        else
        {
            MessageBox.Show("No legal moves found for AI", "AI Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}