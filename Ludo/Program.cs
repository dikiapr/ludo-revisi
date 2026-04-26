using Ludo.Backend.Controllers;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;
using Ludo.UI;

try
{
    IBoard board = new Board();
    IDice dice = new Dice();

    IGameController gameController = new GameController(board, dice);

    GameUI consoleApp = new GameUI(gameController);

    consoleApp.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine("Tekan sembarang tombol untuk keluar...");
    Console.ReadKey();
}
