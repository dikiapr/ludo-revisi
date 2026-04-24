using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Board : IBoard
{
    public const int Size = 15;

    public ITile[,] Grid { get; }
    public IList<IPiece> FinishedPieces { get; } = new List<IPiece>();

    public Board()
    {
        Grid = new ITile[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                Grid[row, col] = new Tile(new Position(col, row), DetermineTileType(row, col));
    }

    private TileTypes DetermineTileType(int row, int col)
    {
        // Area tengah 3x3 = Finish
        if (row >= 6 && row <= 8 && col >= 6 && col <= 8)
            return TileTypes.Finish;

        // Area base di 4 sudut (4x4 tiap warna)
        if (row >= 1 && row <= 4 && col >= 1 && col <= 4)   return TileTypes.Base; // Red
        if (row >= 1 && row <= 4 && col >= 10 && col <= 13) return TileTypes.Base; // Blue
        if (row >= 10 && row <= 13 && col >= 1 && col <= 4) return TileTypes.Base; // Green
        if (row >= 10 && row <= 13 && col >= 10 && col <= 13) return TileTypes.Base; // Yellow

        // Jalur utama: strip horizontal (baris 6-8) dan vertikal (kolom 6-8)
        if (row >= 6 && row <= 8) return TileTypes.Normal;
        if (col >= 6 && col <= 8) return TileTypes.Normal;

        return TileTypes.Normal;
    }
}
