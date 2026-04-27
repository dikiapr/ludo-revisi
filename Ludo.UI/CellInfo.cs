namespace Ludo.UI;

public class CellInfo
{
    public string Symbol;
    public ConsoleColor Foreground;
    public ConsoleColor Background;

    public CellInfo(string symbol, ConsoleColor foreground, ConsoleColor background)
    {
        Symbol = symbol;
        Foreground = foreground;
        Background = background;
    }
}
