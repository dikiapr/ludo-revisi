using Ludo.Backend.Controllers;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;
using Ludo.UI;

try
{
    // Instantiation: Membuat objek-objek model (backend)
    IBoard board = new Board();
    IDice dice = new Dice();

    // Backend Creation & Injection: Membuat GameController dengan DI
    IGameController gameController = new GameController(board, dice);

    // Frontend Creation & Injection: Membuat UI dengan DI
    GameUI consoleApp = new GameUI(gameController);

    // Run Application
    consoleApp.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine("Tekan sembarang tombol untuk keluar...");
    Console.ReadKey();
}
