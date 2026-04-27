using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;
using Ludo.UI;

try
{
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
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine("Tekan sembarang tombol untuk keluar...");
    Console.ReadKey();
}
