using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ChessUI;

public static class ChessCursors
{
    private static Cursor? _white;
    private static Cursor? _black;

    public static Cursor White => _white ??= LoadCursor("pack://application:,,,/ChessUI;component/Assets/Images/CursorW.cur");

    public static Cursor Black => _black ??= LoadCursor("pack://application:,,,/ChessUI;component/Assets/Images/CursorB.cur");

    private static Cursor LoadCursor(string uri)
    {
        var resourceInfo = Application.GetResourceStream(new Uri(uri, UriKind.Absolute))
            ?? throw new FileNotFoundException($"Error: {uri}");

        return new Cursor(resourceInfo.Stream, true);
    }
}