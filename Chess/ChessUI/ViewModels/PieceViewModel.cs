using System.Windows.Media;

namespace ChessUI.ViewModels;

public class PieceViewModel
{
    public required ImageSource Image { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Size { get; set; }
}