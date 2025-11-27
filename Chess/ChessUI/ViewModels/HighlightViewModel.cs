using System.Windows.Media;

namespace ChessUI.ViewModels;

public class HighlightViewModel
{
    public double Left { get; set; }

    public double Top { get; set; }
    public double Size { get; set; }
    public Brush? FillBrush { get; set; }

    public Brush? StrokeBrush { get; set; }
    public double StrokeThickness { get; set; }
}