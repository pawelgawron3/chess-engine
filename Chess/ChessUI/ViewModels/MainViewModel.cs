using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;
using ChessEngine.Evaluation;
using ChessEngine.Search;
using ChessUI.Commands;
using ChessUI.Helpers;
using ChessUI.Models;
using ChessUI.Services;

namespace ChessUI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly GameStateUI _gameState = new();
    private Move _pendingPromotionMove;
    private bool _isAwaitingPromotion = false;
    private bool _isPromotionVisible = false;
    private bool _isBusy = false;
    private string _moveCountText = "0";
    private string _lastMoveText = "—";
    private string _gameStatusText = "Game in progress...";
    private string _sideToMove = "White";
    private const int _squareSize = 75;

    public GameStateUI GameState => _gameState;

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; }
    }

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

    public string SideToMove
    {
        get => _sideToMove;
        set { _sideToMove = value; Raise(nameof(SideToMove)); }
    }

    public ObservableCollection<string> MoveHistoryDisplay { get; } = new();
    public ObservableCollection<PieceViewModel> Pieces { get; } = new();
    public ObservableCollection<HighlightViewModel> Highlights { get; } = new();

    public PromotionMenuViewModel PromotionVM { get; } = new PromotionMenuViewModel();

    public ICommand AiMoveCommand { get; }

    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        AiMoveCommand = new RelayCommand(_ => DoAiMove(), _ => !IsBusy);
        UndoCommand = new RelayCommand(_ => { _gameState.UndoMove(); RefreshUI(); }, _ => !IsBusy);
        RedoCommand = new RelayCommand(_ => { _gameState.RedoMove(); RefreshUI(); }, _ => !IsBusy);

        PromotionVM.OnPieceSelected = OnPromotionPieceSelected;
        _gameState.OnMoveMade += OnMoveMade;

        RefreshUI();
    }

    private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void OnBoardClick(Point point)
    {
        if (IsBusy || _isAwaitingPromotion) return;

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
                Piece? piece = _gameState.GameStateEngine.Board[row, col];
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
        Piece? piece = _gameState.GameStateEngine.Board[pos];
        if (piece != null && piece.Value.Owner == _gameState.GameStateEngine.CurrentPlayer)
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
            ShowPromotionForPlayer(_gameState.GameStateEngine.CurrentPlayer);
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
            Piece? targetPiece = _gameState.GameStateEngine.Board[move.To];

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
        if (_gameState.GameStateEngine.CurrentPlayer == Player.Black)
            MoveCountText = _gameState.GameStateEngine.Services.FullMoveCounter.ToString();

        if (_gameState.GameStateEngine.Services.History.MoveHistory.Count > 0)
            LastMoveText = MoveNotationFormatter.ReturnChessNotation(_gameState.GameStateEngine.Services.History.MoveHistory.Last());
        else
        {
            LastMoveText = "-";
            MoveCountText = "0";
        }

        GameStatusText = _gameState.GameStateEngine.Services.Evaluator.ToDisplayString(_gameState.GameStateEngine.GameResult);
        SideToMove = _gameState.GameStateEngine.CurrentPlayer.ToString();
    }

    private void OnMoveMade(MoveRecord lastMove)
    {
        ChessSounds.PlaySoundForMove(lastMove.Move, lastMove.CapturedPiece, lastMove.KingInCheck);
    }

    private void SetBusy(bool value)
    {
        IsBusy = value;

        (AiMoveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (UndoCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RedoCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void UpdateMoveHistoryDisplay()
    {
        MoveHistoryDisplay.Clear();

        var moveHistory = _gameState.GameStateEngine.Services.History.MoveHistory;
        for (int i = 0; i < moveHistory.Count; i += 2)
        {
            string whiteMove = MoveNotationFormatter.ReturnChessNotation(moveHistory[i]);
            string blackMove = (i + 1 < moveHistory.Count) ?
                MoveNotationFormatter.ReturnChessNotation(moveHistory[i + 1]) :
                "";

            int moveNumber = (i / 2) + 1;
            string line = (string.IsNullOrEmpty(blackMove)) ?
                $"{moveNumber}. {whiteMove}" :
                $"{moveNumber}. {whiteMove} {blackMove}";

            MoveHistoryDisplay.Add(line);
        }
    }

    private void RefreshUI()
    {
        DrawBoard();
        ClearHighlights();
        UpdateGameInfo();
        UpdateMoveHistoryDisplay();
    }

    private async void DoAiMove()
    {
        SetBusy(true);

        int maxDepth = 6;
        Evaluator evaluator = new Evaluator();
        Negamax engine = new Negamax(evaluator);

        var (bestMove, score) = await Task.Run(() => engine.IterativeDeepeningSearch(_gameState.GameStateEngine, maxDepth));
        if (bestMove != null)
        {
            _gameState.TryMakeMove(bestMove.Value);
            RefreshUI();
        }
        else
        {
            MessageBox.Show("No legal moves found for AI", "AI Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        SetBusy(false);
    }
}