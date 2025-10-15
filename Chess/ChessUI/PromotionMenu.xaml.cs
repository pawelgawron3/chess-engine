using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        string colorSuffix = (player == Player.White) ? "W" : "B";

        QueenImage.Source = LoadPieceImage("Queen", colorSuffix);
        RookImage.Source = LoadPieceImage("Rook", colorSuffix);
        BishopImage.Source = LoadPieceImage("Bishop", colorSuffix);
        KnightImage.Source = LoadPieceImage("Knight", colorSuffix);

        this.Visibility = Visibility.Visible;

        static ImageSource LoadPieceImage(string pieceName, string colorSuffix)
        {
            var uri = new Uri($"/Assets/{pieceName}{colorSuffix}.png", UriKind.Relative);
            return new BitmapImage(uri);
        }
    }
}