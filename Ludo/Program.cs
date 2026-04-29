using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;
using Ludo.UI;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/game-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger(); 

try
{
    Log.Information("Starting up the application...");
    IBoard board = new Board();
    IDice dice = new Dice();
    List<IPlayer> players = new List<IPlayer>();
    Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

    IGameController gameController = new GameController(board, dice, players, pieces);

    GameUI consoleApp = new GameUI(gameController);

    consoleApp.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine("Tekan sembarang tombol untuk keluar...");
    Console.ReadKey();
}
finally
{
    Log.CloseAndFlush();
}