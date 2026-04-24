using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.UI;

/// <summary>
/// GameUI — antarmuka konsol interaktif untuk permainan Ludo.
///
/// LAYOUT PAPAN 15×15 (X=col, Y=row):
///   Setiap sel digambar 3 karakter lebar: "XX " atau simbol warna.
///   Area base di sudut, jalur normal (+), area finish (F) di tengah.
/// </summary>
public class GameUI
{
    // ── Dependencies (Dependency Injection) ─────────────────────────────────
    private readonly IGameController _gameController;

    public GameUI(IGameController gameController)
    {
        _gameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
    }

    // ── Konstanta warna ──────────────────────────────────────────────────────
    private static readonly Dictionary<PlayerColor, ConsoleColor> ColorMap = new()
    {
        { PlayerColor.Red,    ConsoleColor.Red     },
        { PlayerColor.Blue,   ConsoleColor.Blue    },
        { PlayerColor.Yellow, ConsoleColor.Yellow  },
        { PlayerColor.Green,  ConsoleColor.Green   },
    };

    // Simbol inisial per warna
    private static readonly Dictionary<PlayerColor, char> ColorSymbol = new()
    {
        { PlayerColor.Red,    'R' },
        { PlayerColor.Blue,   'B' },
        { PlayerColor.Yellow, 'Y' },
        { PlayerColor.Green,  'G' },
    };

    // ── Entry point ──────────────────────────────────────────────────────────

    /// <summary>Mulai UI, buat pemain, jalankan game loop.</summary>
    public void Run()
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            DisplayWelcome();

            // Setup pemain (input dari user)
            int numPlayers = AskNumberOfPlayers();
            var players = CreatePlayers(numPlayers);

            // Subscribe events (backend sudah di-inject via constructor)
            _gameController.OnPieceCaptured += (attacker, victim) =>
                ShowCapture(attacker, victim);
            _gameController.onGameFinished += () =>
                ShowVictory(_gameController);

            // Mulai game dengan players yang sudah dibuat
            _gameController.StartGame(players);

            GameLoop(_gameController);
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
    }

    // ── Game Loop ─────────────────────────────────────────────────────────────

    private void GameLoop(IGameController gameController)
    {
        while (!gameController.IsGameOver)
        {
            Console.Clear();
            DrawBoard(gameController);
            DrawStatus(gameController);

            var currentPlayer = gameController.GetCurrentPlayer();
            PrintLine();
            WriteColored($"  Giliran: {currentPlayer.Name} ", ColorMap[currentPlayer.Color]);
            Console.WriteLine("— tekan [ENTER] untuk lempar dadu...");
            WaitEnter();

            // Roll dadu
            int roll = gameController.RollDice();
            DrawDice(roll);

            // Dapatkan bidak yang bisa bergerak
            var movable = gameController.GetMovablePieces();

            if (movable.Count == 0)
            {
                WriteColored($"  Tidak ada bidak yang bisa bergerak (dadu: {roll}).\n", ConsoleColor.DarkGray);
                Thread.Sleep(1200);
                gameController.NextTurn();
                continue;
            }

            // Pilih bidak
            IPiece chosen;
            if (movable.Count == 1)
            {
                chosen = movable[0];
                WriteColored($"  Bidak otomatis dipilih: Piece #{PieceIndex(gameController, chosen) + 1}\n", ConsoleColor.Cyan);
                Thread.Sleep(600);
            }
            else
            {
                chosen = PickPiece(gameController, movable, currentPlayer);
            }

            // Gerakkan
            gameController.MovePiece(currentPlayer, chosen, roll);

            if (!gameController.IsGameOver)
            {
                // Tampilkan info gerak
                string stepInfo = chosen.State == PieceState.Finished
                    ? "FINISH! 🎉"
                    : $"step {chosen.CurrentStep} → ({chosen.CurrentPosition.X},{chosen.CurrentPosition.Y})";
                WriteColored($"  ➜ Bidak pindah ke {stepInfo}\n", ConsoleColor.White);
                Thread.Sleep(800);

                gameController.NextTurn();
            }
        }
    }

    // ── Input Helper ─────────────────────────────────────────────────────────

    private IPiece PickPiece(IGameController gameController, IList<IPiece> movable, IPlayer player)
    {
        Console.WriteLine();
        Console.WriteLine("  Pilih bidak yang ingin digerakkan:");
        for (int i = 0; i < movable.Count; i++)
        {
            var p = movable[i];
            string stateStr = p.State == PieceState.Base
                ? "Di Base"
                : $"Step {p.CurrentStep} ({p.CurrentPosition.X},{p.CurrentPosition.Y})";
            WriteColored($"    [{i + 1}] Piece #{PieceIndex(gameController, p) + 1} — {stateStr}\n",
                ColorMap[player.Color]);
        }

        while (true)
        {
            Console.Write("  Pilihan (angka): ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out int idx) && idx >= 1 && idx <= movable.Count)
                return movable[idx - 1];
            Console.WriteLine("  Input tidak valid, coba lagi.");
        }
    }

    private int AskNumberOfPlayers()
    {
        while (true)
        {
            Console.Write("  Jumlah pemain (2-4): ");
            if (int.TryParse(Console.ReadLine(), out int n) && n >= 2 && n <= 4)
                return n;
            Console.WriteLine("  Masukkan angka 2-4.");
        }
    }

    private List<IPlayer> CreatePlayers(int count)
    {
        var colors = Enum.GetValues<PlayerColor>();
        var players = new List<IPlayer>();

        for (int i = 0; i < count; i++)
        {
            PlayerColor color = colors[i];
            string defaultName = color.ToString();

            WriteColored($"  Nama pemain {i + 1} [{color}] (Enter = {defaultName}): ", ColorMap[color]);
            string input = Console.ReadLine()?.Trim() ?? "";
            string name = string.IsNullOrEmpty(input) ? defaultName : input;

            players.Add(new Player(name, color));
            WriteColored($"  ✓ {name} ({color})\n", ColorMap[color]);
        }

        return players;
    }

    // ── Board Rendering ───────────────────────────────────────────────────────

    /// <summary>Gambar papan Ludo 15×15 dengan posisi semua bidak saat ini.</summary>
    private void DrawBoard(IGameController gameController)
    {
        // Bangun lookup: (X,Y) → list simbol bidak
        var pieceMap = BuildPieceMap(gameController);

        Console.WriteLine("  ╔═══ LUDO ════════════════════════════════════════════╗");
        Console.WriteLine("  ║    0  1  2  3  4  5  6  7  8  9 10 11 12 13 14     ║");
        Console.WriteLine("  ╠════════════════════════════════════════════════════╣");

        for (int row = 0; row < Board.Size; row++)
        {
            Console.Write($"  ║{row,2} ");
            for (int col = 0; col < Board.Size; col++)
            {
                var tileType = DetermineTileType(row, col);
                var key = (col, row);

                if (pieceMap.TryGetValue(key, out var symbols) && symbols.Count > 0)
                {
                    // Tampilkan bidak (satu atau lebih)
                    if (symbols.Count == 1)
                    {
                        WriteColored(symbols[0].display.PadLeft(2), symbols[0].color);
                        Console.Write(" ");
                    }
                    else
                    {
                        // Beberapa bidak di kotak yang sama
                        WriteColored($"#{symbols.Count} ", ConsoleColor.White);
                    }
                }
                else
                {
                    // Tile kosong
                    string cell = tileType switch
                    {
                        TileTypes.Base => "██ ",
                        TileTypes.Finish => " F ",
                        TileTypes.Normal => " · ",
                        _ => "   ",
                    };
                    ConsoleColor tileColor = tileType switch
                    {
                        TileTypes.Finish => ConsoleColor.Magenta,
                        _ => ConsoleColor.DarkGray,
                    };
                    WriteColored(cell, tileColor);
                }
            }
            Console.WriteLine("║");
        }

        Console.WriteLine("  ╚════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    /// <summary>Bangun map posisi → daftar (simbol, warna) dari semua bidak aktif/finished.</summary>
    private Dictionary<(int X, int Y), List<(string display, ConsoleColor color)>> BuildPieceMap(IGameController gameController)
    {
        var map = new Dictionary<(int, int), List<(string, ConsoleColor)>>();

        foreach (var (color, pieces) in gameController.GetAllPieces())
        {
            char sym = ColorSymbol[color];
            ConsoleColor cc = ColorMap[color];

            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                if (piece.State == PieceState.Base || piece.State == PieceState.Finished)
                {
                    // Bidak di base tetap ditampilkan di posisinya
                }
                var pos = (piece.CurrentPosition.X, piece.CurrentPosition.Y);
                if (!map.ContainsKey(pos)) map[pos] = new();
                map[pos].Add(($"{sym}{i + 1}", cc));
            }
        }

        return map;
    }

    // ── Status Panel ──────────────────────────────────────────────────────────

    private void DrawStatus(IGameController gameController)
    {
        Console.WriteLine("  ┌─── STATUS PEMAIN ────────────────────────────────────┐");

        var allPieces = gameController.GetAllPieces();
        foreach (var player in gameController.GetPlayers())
        {
            bool isCurrent = gameController.GetCurrentPlayer().Color == player.Color;
            string marker = isCurrent ? "▶ " : "  ";
            var pieces = allPieces[player.Color];

            // Hitung berapa bidak finished
            int finished = pieces.Count(p => p.State == PieceState.Finished);
            int active = pieces.Count(p => p.State == PieceState.Active);
            int inBase = pieces.Count(p => p.State == PieceState.Base);

            string progress = $"Finish:{finished} Aktif:{active} Base:{inBase}";
            string line = $"  │ {marker}{player.Name,-10} [{player.Color,-6}] {progress}";

            WriteColored(line.PadRight(58), isCurrent ? ConsoleColor.Cyan : ColorMap[player.Color]);
            Console.WriteLine("│");
        }

        Console.WriteLine("  └──────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }

    // ── Dice Display ──────────────────────────────────────────────────────────

    private void DrawDice(int roll)
    {
        string[] faces =
        {
            "", // index 0 unused
            "┌─────┐\n│     │\n│  ●  │\n│     │\n└─────┘",
            "┌─────┐\n│ ●   │\n│     │\n│   ● │\n└─────┘",
            "┌─────┐\n│ ●   │\n│  ●  │\n│   ● │\n└─────┘",
            "┌─────┐\n│ ● ● │\n│     │\n│ ● ● │\n└─────┘",
            "┌─────┐\n│ ● ● │\n│  ●  │\n│ ● ● │\n└─────┘",
            "┌─────┐\n│ ● ● │\n│ ● ● │\n│ ● ● │\n└─────┘",
        };

        Console.WriteLine();
        WriteColored($"  🎲 Hasil dadu: {roll}\n", ConsoleColor.Yellow);
        foreach (var diceLine in faces[roll].Split('\n'))
            Console.WriteLine("    " + diceLine);
        if (roll == 6)
            WriteColored("  ★ Dadu 6! Giliran bonus!\n", ConsoleColor.Yellow);
        Console.WriteLine();
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void ShowCapture(IPiece attacker, IPiece victim)
    {
        Console.WriteLine();
        WriteColored($"  💥 TANGKAP! Bidak {attacker.Color} menangkap bidak {victim.Color}! Kembali ke base.\n",
            ConsoleColor.Magenta);
        Thread.Sleep(1000);
    }

    private void ShowVictory(IGameController gameController)
    {
        Console.Clear();
        Console.WriteLine();

        string[] banner =
        {
            @"  ██╗     ██╗   ██╗██████╗  ██████╗ ██╗",
            @"  ██║     ██║   ██║██╔══██╗██╔═══██╗██║",
            @"  ██║     ██║   ██║██║  ██║██║   ██║██║",
            @"  ██║     ██║   ██║██║  ██║██║   ██║╚═╝",
            @"  ███████╗╚██████╔╝██████╔╝╚██████╔╝██╗",
            @"  ╚══════╝ ╚═════╝ ╚═════╝  ╚═════╝ ╚═╝",
        };

        foreach (var line in banner)
        {
            WriteColored(line + "\n", ConsoleColor.Yellow);
            Thread.Sleep(80);
        }

        Console.WriteLine();
        Console.WriteLine("  🏆 PEMENANG: ");

        // Cari pemenang = pemain yang semua bidaknya Finished
        foreach (var player in gameController.GetPlayers())
        {
            var pieces = gameController.GetAllPieces()[player.Color];
            if (pieces.All(p => p.State == PieceState.Finished))
            {
                WriteColored($"      ★ {player.Name} ({player.Color}) ★\n",
                    ColorMap[player.Color]);
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Tekan [ENTER] untuk keluar...");
        Console.ReadLine();
    }

    // ── Utilities ────────────────────────────────────────────────────────────

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static void PrintLine() =>
        Console.WriteLine("  " + new string('─', 54));

    private static void WaitEnter()
    {
        Console.ReadLine();
    }

    /// <summary>
    /// Tentukan tipe tile berdasarkan koordinat — layout Ludo selalu tetap.
    /// Tidak perlu akses Board/IBoard sama sekali.
    /// </summary>
    private static TileTypes DetermineTileType(int row, int col)
    {
        // Area tengah 3×3 = Finish
        if (row >= 6 && row <= 8 && col >= 6 && col <= 8)
            return TileTypes.Finish;

        // Area base di 4 sudut (4×4 tiap warna)
        if (row >= 1 && row <= 4 && col >= 1 && col <= 4) return TileTypes.Base; // Red
        if (row >= 1 && row <= 4 && col >= 10 && col <= 13) return TileTypes.Base; // Blue
        if (row >= 10 && row <= 13 && col >= 1 && col <= 4) return TileTypes.Base; // Green
        if (row >= 10 && row <= 13 && col >= 10 && col <= 13) return TileTypes.Base; // Yellow

        // Jalur utama (cross shape)
        if (row >= 6 && row <= 8) return TileTypes.Normal;
        if (col >= 6 && col <= 8) return TileTypes.Normal;

        return TileTypes.Normal;
    }

    /// <summary>Dapatkan index bidak dalam daftar milik warnanya.</summary>
    private static int PieceIndex(IGameController gameController, IPiece piece)
    {
        var list = gameController.GetAllPieces()[piece.Color];
        for (int i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], piece)) return i;
        return 0;
    }

    /// <summary>Helper untuk welcome screen.</summary>
    private static void DisplayWelcome()
    {
        Console.Clear();
        WriteColored("  ╔══════════════════════════════╗\n", ConsoleColor.Cyan);
        WriteColored("  ║     SELAMAT DATANG DI LUDO   ║\n", ConsoleColor.Cyan);
        WriteColored("  ╚══════════════════════════════╝\n", ConsoleColor.Cyan);
        Console.WriteLine();
        Console.WriteLine("  Aturan:");
        Console.WriteLine("  - Dadu 6 → keluarkan bidak dari base / giliran bonus");
        Console.WriteLine("  - Bidak mencapai step 57 → FINISH");
        Console.WriteLine("  - Semua 4 bidak Finish → MENANG");
        Console.WriteLine("  - Landing di bidak lawan → lawan kembali ke base");
        Console.WriteLine();
    }
}
