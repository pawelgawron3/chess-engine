using System.Windows;
using System.Windows.Controls;
using ChessEngine;

namespace ChessUI;

/// <summary>
/// Interaction logic for PromotionMenu.xaml
/// </summary>
public partial class PromotionMenu : UserControl
{
    public event Action<PieceType>? PieceSelected;

    public PromotionMenu()
    {
        InitializeComponent();
    }

    private void OnSelectPiece(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            var piece = tag switch
            {
                "Queen" => PieceType.Queen,
                "Rook" => PieceType.Rook,
                "Bishop" => PieceType.Bishop,
                "Knight" => PieceType.Knight,
                _ => PieceType.Queen
            };

            PieceSelected?.Invoke(piece);
            this.Visibility = Visibility.Collapsed;
        }
    }

    public void ShowForPlayer(Player player)
    {
        // TODO: Dynamic piece color selection
        this.Visibility = Visibility.Visible;
    }
}