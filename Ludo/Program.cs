using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;
using Ludo.UI;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    // Console sink with structured output - great for development
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] " +
        "{Message:lj} {Properties:j}{NewLine}{Exception}")
    
    // File sink with JSON format - perfect for log aggregation tools
    .WriteTo.File("logs/application-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] " +
                       "{Message:lj} {Properties:j}{NewLine}{Exception}")
    
    // File sink with JSON format for structured data analysis
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/application-json-.log",
        rollingInterval: RollingInterval.Day)
    
    // Enrich logs with additional context information
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
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