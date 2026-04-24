using Ludo.Backend.Enums;
using Ludo.Backend.Models;

var board = new Board();

Console.WriteLine("=== LUDO BOARD (15x15) ===");
Console.WriteLine("R=Base  N=Normal  F=Finish  .=Sudut");
Console.WriteLine();

// Header kolom
Console.Write("   ");
for (int col = 0; col < Board.Size; col++)
    Console.Write($"{col,3}");
Console.WriteLine();

for (int row = 0; row < Board.Size; row++)
{
    Console.Write($"{row,2} ");
    for (int col = 0; col < Board.Size; col++)
    {
        var tile = board.Grid[row, col];
        char symbol = tile.Type switch
        {
            TileTypes.Base   => 'B',
            TileTypes.Finish => 'F',
            TileTypes.Normal => 'N',
            _                => '.'
        };
        Console.Write($"  {symbol}");
    }
    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine($"Grid[6,0] type = {board.Grid[6, 0].Type}");   // Normal (jalur kiri)
Console.WriteLine($"Grid[1,1] type = {board.Grid[1, 1].Type}");   // Base  (Red)
Console.WriteLine($"Grid[7,7] type = {board.Grid[7, 7].Type}");   // Finish (tengah)
Console.WriteLine($"Grid[0,0] type = {board.Grid[0, 0].Type}");   // sudut
