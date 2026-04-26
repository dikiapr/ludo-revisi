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

    // Background gelap untuk area base (kontras dengan teks bidak)
    private static readonly Dictionary<PlayerColor, ConsoleColor> BaseBgMap = new()
    {
        { PlayerColor.Red,    ConsoleColor.DarkRed    },
        { PlayerColor.Blue,   ConsoleColor.DarkBlue   },
        { PlayerColor.Yellow, ConsoleColor.DarkYellow },
        { PlayerColor.Green,  ConsoleColor.DarkGreen  },
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
            List<IPlayer> players = CreatePlayers(numPlayers);

            // Subscribe events (backend sudah di-inject via constructor)
            _gameController.OnPieceCaptured += OnPieceCapturedHandler;
            _gameController.onGameFinished += OnGameFinishedHandler;

            void OnPieceCapturedHandler(IPiece attacker, IPiece victim)
            {
                ShowCapture(attacker, victim);
            }

            void OnGameFinishedHandler()
            {
                ShowVictory(_gameController);
            }

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

            IPlayer currentPlayer = gameController.GetCurrentPlayer();
            PrintLine();
            WriteColored($"  Giliran: {currentPlayer.Name} ", ColorMap[currentPlayer.Color]);
            Console.WriteLine("— tekan [ENTER] untuk lempar dadu...");
            WaitEnter();

            // Roll dadu
            int roll = gameController.RollDice();
            DrawDice(roll);

            // Dapatkan bidak yang bisa bergerak
            IList<IPiece> movable = gameController.GetMovablePieces();

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
                string stepInfo;

                if (chosen.State == PieceState.Finished)
                {
                    stepInfo = "FINISH! 🎉";
                }
                else
                {
                    stepInfo = $"step {chosen.CurrentStep} → ({chosen.CurrentPosition.X},{chosen.CurrentPosition.Y})";
                }

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
            IPiece p = movable[i];
            string stateStr = p.State == PieceState.Base
                ? "Di Base"
                : $"Step {p.CurrentStep} ({p.CurrentPosition.X},{p.CurrentPosition.Y})";
            WriteColored($"    [{i + 1}] Piece #{PieceIndex(gameController, p) + 1} — {stateStr}\n",
                ColorMap[player.Color]);
        }

        while (true)
        {
            Console.Write("  Pilihan (angka): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int idx) && idx >= 1 && idx <= movable.Count)
            {
                IPiece selectedPiece = movable[idx - 1];
                return selectedPiece;
            }
            Console.WriteLine("  Input tidak valid, coba lagi.");
        }
    }

    private int AskNumberOfPlayers()
    {
        while (true)
        {
            Console.Write("  Jumlah pemain (2-4): ");
            if (int.TryParse(Console.ReadLine(), out int numPlayers) && numPlayers >= 2 && numPlayers <= 4)
            {
                return numPlayers;
            }
            Console.WriteLine("  Masukkan angka 2-4.");
        }
    }

    private List<IPlayer> CreatePlayers(int count)
    {
        PlayerColor[] colors = Enum.GetValues<PlayerColor>();
        List<IPlayer> players = new List<IPlayer>();

        for (int i = 0; i < count; i++)
        {
            PlayerColor color = colors[i];
            string defaultName = color.ToString();

            WriteColored($"  Nama pemain {i + 1} [{color}] (Enter = {defaultName}): ", ColorMap[color]);
            string? input = Console.ReadLine();

            if (input != null)
            {
                input = input.Trim();
            }
            else
            {
                input = "";
            }

            string name;

            if (string.IsNullOrEmpty(input))
            {
                name = defaultName;
            }
            else
            {
                name = input;
            }

            players.Add(new Player(name, color));
            WriteColored($"  ✓ {name} ({color})\n", ColorMap[color]);
        }

        return players;
    }

    // ── Board Rendering ───────────────────────────────────────────────────────

    /// <summary>Gambar papan Ludo 15×15 dengan posisi semua bidak saat ini.</summary>
    private void DrawBoard(IGameController gameController)
    {
        Dictionary<(int X, int Y), List<(string display, ConsoleColor color)>> pieceMap = BuildPieceMap(gameController);

        // Header kolom: setiap nomor menempati 3 char ("XX ") supaya lurus dengan sel.
        string colHeader = "";
        for (int col = 0; col < Board.Size; col++)
        {
            colHeader += $"{col,2} ";
        }

        Console.WriteLine();
        WriteColored("  ╔════════════════ ★ PAPAN LUDO ★ ════════════════╗\n", ConsoleColor.Cyan);
        WriteColored($"  ║   {colHeader}║\n", ConsoleColor.Cyan);
        WriteColored("  ╠════════════════════════════════════════════════╣\n", ConsoleColor.Cyan);

        for (int row = 0; row < Board.Size; row++)
        {
            WriteColored("  ║", ConsoleColor.Cyan);
            Console.Write($"{row,2} ");

            for (int col = 0; col < Board.Size; col++)
            {
                DrawCell(row, col, pieceMap);
            }

            WriteColored("║\n", ConsoleColor.Cyan);
        }

        WriteColored("  ╚════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);
        DrawLegend();
    }

    /// <summary>Render satu sel: bidak (jika ada) atau tile kosong dengan style sesuai tipenya.</summary>
    private void DrawCell(int row, int col, Dictionary<(int X, int Y), List<(string display, ConsoleColor color)>> pieceMap)
    {
        (string symbol, ConsoleColor foreground, ConsoleColor background) tile = RenderEmptyCell(row, col);

        if (pieceMap.TryGetValue((col, row), out List<(string display, ConsoleColor color)>? pieces) && pieces.Count > 0)
        {
            // Bidak digambar di atas bg tile (mis. bidak di base tetap berlatar warna basenya).
            if (pieces.Count == 1)
            {
                WriteCell($"{pieces[0].display} ", pieces[0].color, tile.background);
            }
            else
            {
                WriteCell($"+{pieces.Count} ", ConsoleColor.White, tile.background);
            }
        }
        else
        {
            WriteCell(tile.symbol, tile.foreground, tile.background);
        }
    }

    /// <summary>Tulis teks dengan warna foreground + background, lalu reset.</summary>
    private static void WriteCell(string text, ConsoleColor foreground, ConsoleColor background)
    {
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        Console.Write(text);
        Console.ResetColor();
    }

    /// <summary>Bangun map posisi → daftar (simbol, warna) dari semua bidak aktif/finished.</summary>
    private Dictionary<(int X, int Y), List<(string display, ConsoleColor color)>> BuildPieceMap(IGameController gameController)
    {
        Dictionary<(int, int), List<(string, ConsoleColor)>> map = new Dictionary<(int, int), List<(string, ConsoleColor)>>();

        foreach ((PlayerColor color, IList<IPiece>? pieces) in gameController.GetAllPieces())
        {
            char sym = ColorSymbol[color];
            ConsoleColor cc = ColorMap[color];

            for (int i = 0; i < pieces.Count; i++)
            {
                IPiece piece = pieces[i];
                if (piece.State == PieceState.Base || piece.State == PieceState.Finished)
                {
                    // Bidak di base tetap ditampilkan di posisinya
                }
                (int X, int Y) pos = (piece.CurrentPosition.X, piece.CurrentPosition.Y);
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

        IDictionary<PlayerColor, IList<IPiece>> allPieces = gameController.GetAllPieces();
        foreach (IPlayer player in gameController.GetPlayers())
        {
            bool isCurrent = gameController.GetCurrentPlayer().Color == player.Color;
            string marker;

            if (isCurrent)
            {
                marker = "▶ ";
            }
            else
            {
                marker = "  ";
            }

            IList<IPiece> pieces = allPieces[player.Color];

            // Hitung berapa bidak finished
            int finished = 0;
            int active = 0;
            int inBase = 0;

            foreach (IPiece p in pieces)
            {
                if (p.State == PieceState.Finished)
                {
                    finished++;
                }
                else if (p.State == PieceState.Active)
                {
                    active++;
                }
                else if (p.State == PieceState.Base)
                {
                    inBase++;
                }
            }

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
        foreach (string diceLine in faces[roll].Split('\n'))
        {
            Console.WriteLine("    " + diceLine);
        }
        if (roll == 6)
        {
            WriteColored("  ★ Dadu 6! Giliran bonus!\n", ConsoleColor.Yellow);
        }
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

        foreach (string line in banner)
        {
            WriteColored(line + "\n", ConsoleColor.Yellow);
            Thread.Sleep(80);
        }

        Console.WriteLine();
        Console.WriteLine("  🏆 PEMENANG: ");

        // Cari pemenang = pemain yang semua bidaknya Finished
        foreach (IPlayer player in gameController.GetPlayers())
        {
            IList<IPiece> pieces = gameController.GetAllPieces()[player.Color];
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

    private static void PrintLine()
    {
        Console.WriteLine("  " + new string('─', 54));
    }

    private static void WaitEnter()
    {
        Console.ReadLine();
    }

    // ── Tile Layout Helpers ──────────────────────────────────────────────────

    /// <summary>Apakah sel termasuk area finish 3×3 di pusat papan.</summary>
    private static bool IsCenterFinish(int row, int col)
    {
        bool isCenterFinish = row >= 6 && row <= 8 && col >= 6 && col <= 8;
        return isCenterFinish;
    }

    /// <summary>Pemilik area base 4×4 di sudut, atau null bila bukan base.</summary>
    private static PlayerColor? GetBaseOwner(int row, int col)
    {
        if (row >= 1 && row <= 4 && col >= 1 && col <= 4)
        {
            PlayerColor redOwner = PlayerColor.Red;
            return redOwner;
        }
        if (row >= 1 && row <= 4 && col >= 10 && col <= 13)
        {
            PlayerColor blueOwner = PlayerColor.Blue;
            return blueOwner;
        }
        if (row >= 10 && row <= 13 && col >= 10 && col <= 13)
        {
            PlayerColor yellowOwner = PlayerColor.Yellow;
            return yellowOwner;
        }
        if (row >= 10 && row <= 13 && col >= 1 && col <= 4)
        {
            PlayerColor greenOwner = PlayerColor.Green;
            return greenOwner;
        }
        PlayerColor? noOwner = null;
        return noOwner;
    }

    /// <summary>Apakah sel ini adalah salah satu dari 4 slot bidak di dalam area base (inner 2×2 di tengah).</summary>
    private static bool IsBaseSlot(int row, int col)
    {
        if (!GetBaseOwner(row, col).HasValue)
        {
            bool isNotBase = false;
            return isNotBase;
        }
        bool innerRow = row == 2 || row == 3 || row == 11 || row == 12;
        bool innerCol = col == 2 || col == 3 || col == 11 || col == 12;
        bool isInnerSlot = innerRow && innerCol;
        return isInnerSlot;
    }

    /// <summary>Pemilik kotak start (safe square) atau null.</summary>
    private static PlayerColor? GetStartOwner(int row, int col)
    {
        if (col == 1 && row == 6)
        {
            PlayerColor redOwner = PlayerColor.Red;
            return redOwner;
        }
        if (col == 8 && row == 1)
        {
            PlayerColor blueOwner = PlayerColor.Blue;
            return blueOwner;
        }
        if (col == 13 && row == 8)
        {
            PlayerColor yellowOwner = PlayerColor.Yellow;
            return yellowOwner;
        }
        if (col == 6 && row == 13)
        {
            PlayerColor greenOwner = PlayerColor.Green;
            return greenOwner;
        }
        PlayerColor? noOwner = null;
        return noOwner;
    }

    /// <summary>4 star square netral (8 langkah setelah tiap start) — juga safe.</summary>
    private static bool IsNeutralSafeStar(int row, int col)
    {
        if (col == 6 && row == 2)
        {
            bool isRedSafeStar = true;  // 8 dari Red
            return isRedSafeStar;
        }
        if (col == 12 && row == 6)
        {
            bool isBlueSafeStar = true;  // 8 dari Blue
            return isBlueSafeStar;
        }
        if (col == 8 && row == 12)
        {
            bool isYellowSafeStar = true;  // 8 dari Yellow
            return isYellowSafeStar;
        }
        if (col == 2 && row == 8)
        {
            bool isGreenSafeStar = true;  // 8 dari Green
            return isGreenSafeStar;
        }
        bool isNotSafeStar = false;
        return isNotSafeStar;
    }

    /// <summary>Pemilik tile home column (5 sel sebelum finish), atau null.</summary>
    private static PlayerColor? GetHomeOwner(int row, int col)
    {
        if (row == 7 && col >= 1 && col <= 5)
        {
            PlayerColor redOwner = PlayerColor.Red;
            return redOwner;
        }
        if (col == 7 && row >= 1 && row <= 5)
        {
            PlayerColor blueOwner = PlayerColor.Blue;
            return blueOwner;
        }
        if (row == 7 && col >= 9 && col <= 13)
        {
            PlayerColor yellowOwner = PlayerColor.Yellow;
            return yellowOwner;
        }
        if (col == 7 && row >= 9 && row <= 13)
        {
            PlayerColor greenOwner = PlayerColor.Green;
            return greenOwner;
        }
        PlayerColor? noOwner = null;
        return noOwner;
    }

    /// <summary>Apakah sel ini path normal di cross-shape (di luar finish/base/home/start).</summary>
    private static bool IsPath(int row, int col)
    {
        bool inCross = (row >= 6 && row <= 8) || (col >= 6 && col <= 8);
        bool isPath = inCross && !IsCenterFinish(row, col);
        return isPath;
    }

    /// <summary>Tentukan simbol + warna fg/bg untuk sel kosong berdasarkan tipenya.</summary>
    private static (string symbol, ConsoleColor foreground, ConsoleColor background) RenderEmptyCell(int row, int col)
    {
        if (IsCenterFinish(row, col))
        {
            (string symbol, ConsoleColor foreground, ConsoleColor background) finishCell = RenderFinishCell(row, col);
            return finishCell;
        }

        PlayerColor? baseOwner = GetBaseOwner(row, col);
        if (baseOwner.HasValue)
        {
            ConsoleColor background = BaseBgMap[baseOwner.Value];
            string symbol = IsBaseSlot(row, col) ? " ○ " : "   ";
            (string symbol, ConsoleColor White, ConsoleColor background) baseCell = (symbol, ConsoleColor.White, background);
            return baseCell;
        }

        PlayerColor? startOwner = GetStartOwner(row, col);
        if (startOwner.HasValue)
        {
            (string, ConsoleColor, ConsoleColor Black) startCell = (" ★ ", ColorMap[startOwner.Value], ConsoleColor.Black);
            return startCell;
        }

        if (IsNeutralSafeStar(row, col))
        {
            (string, ConsoleColor White, ConsoleColor Black) safeStarCell = (" ★ ", ConsoleColor.White, ConsoleColor.Black);
            return safeStarCell;
        }

        PlayerColor? homeOwner = GetHomeOwner(row, col);
        if (homeOwner.HasValue)
        {
            (string, ConsoleColor, ConsoleColor Black) homeCell = (" ▪ ", ColorMap[homeOwner.Value], ConsoleColor.Black);
            return homeCell;
        }

        if (IsPath(row, col))
        {
            (string, ConsoleColor DarkGray, ConsoleColor Black) pathCell = (" · ", ConsoleColor.DarkGray, ConsoleColor.Black);
            return pathCell;
        }

        // Di luar cross / base — biarkan kosong supaya cross-shape menonjol.
        (string, ConsoleColor, ConsoleColor) emptyCell = ("   ", ConsoleColor.Black, ConsoleColor.Black);
        return emptyCell;
    }

    /// <summary>Pusat 3×3: panah masuk per warna + bintang finish di tengah.</summary>
    private static (string symbol, ConsoleColor foreground, ConsoleColor background) RenderFinishCell(int row, int col)
    {
        if (col == 7 && row == 7)
        {
            (string, ConsoleColor Magenta, ConsoleColor Black) centerStarCell = (" ★ ", ConsoleColor.Magenta, ConsoleColor.Black);
            return centerStarCell;
        }
        if (col == 7 && row == 6)
        {
            (string, ConsoleColor Blue, ConsoleColor Black) blueArrowCell = (" ▼ ", ConsoleColor.Blue, ConsoleColor.Black);
            return blueArrowCell;
        }
        if (col == 7 && row == 8)
        {
            (string, ConsoleColor Green, ConsoleColor Black) greenArrowCell = (" ▲ ", ConsoleColor.Green, ConsoleColor.Black);
            return greenArrowCell;
        }
        if (col == 6 && row == 7)
        {
            (string, ConsoleColor Red, ConsoleColor Black) redArrowCell = (" ▶ ", ConsoleColor.Red, ConsoleColor.Black);
            return redArrowCell;
        }
        if (col == 8 && row == 7)
        {
            (string, ConsoleColor Yellow, ConsoleColor Black) yellowArrowCell = (" ◀ ", ConsoleColor.Yellow, ConsoleColor.Black);
            return yellowArrowCell;
        }
        (string, ConsoleColor DarkMagenta, ConsoleColor Black) defaultFinishCell = (" ◇ ", ConsoleColor.DarkMagenta, ConsoleColor.Black);
        return defaultFinishCell;
    }

    /// <summary>Cetak baris keterangan simbol di bawah papan.</summary>
    private void DrawLegend()
    {
        Console.Write("  ");
        WriteColored("★", ConsoleColor.Red);
        Console.Write(" Start (safe)   ");
        WriteColored("★", ConsoleColor.White);
        Console.Write(" Star (safe)   ");
        WriteColored("▪", ConsoleColor.White);
        Console.Write(" Home   ");
        WriteColored("·", ConsoleColor.DarkGray);
        Console.WriteLine(" Path");
        Console.Write("  ");
        WriteColored("○", ConsoleColor.White);
        Console.Write(" Slot base   ");
        WriteColored("★", ConsoleColor.Magenta);
        Console.WriteLine(" Finish");
        Console.WriteLine();
    }

    /// <summary>Dapatkan index bidak dalam daftar milik warnanya.</summary>
    private static int PieceIndex(IGameController gameController, IPiece piece)
    {
        IList<IPiece> list = gameController.GetAllPieces()[piece.Color];
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], piece))
            {
                int foundIndex = i;
                return foundIndex;
            }
        }
        int defaultIndex = 0;
        return defaultIndex;
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
