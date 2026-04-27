using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.UI;

public class GameUI
{
    private readonly IGameController _gameController;

    public GameUI(IGameController gameController)
    {
        _gameController = gameController ?? throw new ArgumentNullException(nameof(gameController));
    }

    private static readonly Dictionary<PlayerColor, ConsoleColor> ColorMap = new()
    {
        { PlayerColor.Red,    ConsoleColor.Red     },
        { PlayerColor.Blue,   ConsoleColor.Blue    },
        { PlayerColor.Yellow, ConsoleColor.Yellow  },
        { PlayerColor.Green,  ConsoleColor.Green   },
    };

    private static readonly Dictionary<PlayerColor, char> ColorSymbol = new()
    {
        { PlayerColor.Red,    'R' },
        { PlayerColor.Blue,   'B' },
        { PlayerColor.Yellow, 'Y' },
        { PlayerColor.Green,  'G' },
    };

    private static readonly Dictionary<PlayerColor, ConsoleColor> BaseBgMap = new()
    {
        { PlayerColor.Red,    ConsoleColor.DarkRed    },
        { PlayerColor.Blue,   ConsoleColor.DarkBlue   },
        { PlayerColor.Yellow, ConsoleColor.DarkYellow },
        { PlayerColor.Green,  ConsoleColor.DarkGreen  },
    };

    public void Run()
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            DisplayWelcome();

            int numPlayers = AskNumberOfPlayers();
            List<IPlayer> players = CreatePlayers(numPlayers);

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

            _gameController.StartGame(players);

            GameLoop(_gameController);
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
    }

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

            int roll = gameController.RollDice();
            DrawDice(roll);

            IList<IPiece> movable = gameController.GetMovablePieces();

            if (movable.Count == 0)
            {
                WriteColored($"  Tidak ada bidak yang bisa bergerak (dadu: {roll}).\n", ConsoleColor.DarkGray);
                Thread.Sleep(1200);
                gameController.NextTurn();
                continue;
            }

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

            gameController.MovePiece(currentPlayer, chosen, roll);

            if (!gameController.IsGameOver)
            {
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

    private void DrawBoard(IGameController gameController)
    {
        Dictionary<string, List<PieceDisplay>> pieceMap = BuildPieceMap(gameController);

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

    private void DrawCell(int row, int col, Dictionary<string, List<PieceDisplay>> pieceMap)
    {
        CellInfo tile = RenderEmptyCell(row, col);

        string key = col + "," + row;

        if (pieceMap.ContainsKey(key) && pieceMap[key].Count > 0)
        {
            List<PieceDisplay> pieces = pieceMap[key];

            if (pieces.Count == 1)
            {
                WriteCell(pieces[0].Display + " ", pieces[0].Color, tile.Background);
            }
            else
            {
                WriteCell("+" + pieces.Count + " ", ConsoleColor.White, tile.Background);
            }
        }
        else
        {
            WriteCell(tile.Symbol, tile.Foreground, tile.Background);
        }
    }

    private static void WriteCell(string text, ConsoleColor foreground, ConsoleColor background)
    {
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        Console.Write(text);
        Console.ResetColor();
    }

    private Dictionary<string, List<PieceDisplay>> BuildPieceMap(IGameController gameController)
    {
        Dictionary<string, List<PieceDisplay>> map = new Dictionary<string, List<PieceDisplay>>();

        foreach (KeyValuePair<PlayerColor, IList<IPiece>> entry in gameController.GetAllPieces())
        {
            PlayerColor color = entry.Key;
            IList<IPiece> pieces = entry.Value;

            char sym = ColorSymbol[color];
            ConsoleColor cc = ColorMap[color];

            for (int i = 0; i < pieces.Count; i++)
            {
                IPiece piece = pieces[i];
                if (piece.State == PieceState.Base || piece.State == PieceState.Finished)
                {
                    // Bidak di base tetap ditampilkan di posisinya
                }

                int posX = piece.CurrentPosition.X;
                int posY = piece.CurrentPosition.Y;
                string key = posX + "," + posY;

                if (!map.ContainsKey(key))
                {
                    map[key] = new List<PieceDisplay>();
                }

                map[key].Add(new PieceDisplay($"{sym}{i + 1}", cc));
            }
        }

        return map;
    }

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

    private static bool IsCenterFinish(int row, int col)
    {
        bool isCenterFinish = row >= 6 && row <= 8 && col >= 6 && col <= 8;
        return isCenterFinish;
    }

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

    private static bool IsNeutralSafeStar(int row, int col)
    {
        if (col == 6 && row == 2)
        {
            bool isRedSafeStar = true;
            return isRedSafeStar;
        }
        if (col == 12 && row == 6)
        {
            bool isBlueSafeStar = true;
            return isBlueSafeStar;
        }
        if (col == 8 && row == 12)
        {
            bool isYellowSafeStar = true;
            return isYellowSafeStar;
        }
        if (col == 2 && row == 8)
        {
            bool isGreenSafeStar = true;
            return isGreenSafeStar;
        }
        bool isNotSafeStar = false;
        return isNotSafeStar;
    }

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

    private static bool IsPath(int row, int col)
    {
        bool inCross = (row >= 6 && row <= 8) || (col >= 6 && col <= 8);
        bool isPath = inCross && !IsCenterFinish(row, col);
        return isPath;
    }

    private static CellInfo RenderEmptyCell(int row, int col)
    {
        if (IsCenterFinish(row, col))
        {
            CellInfo finishCell = RenderFinishCell(row, col);
            return finishCell;
        }

        PlayerColor? baseOwner = GetBaseOwner(row, col);
        if (baseOwner.HasValue)
        {
            ConsoleColor background = BaseBgMap[baseOwner.Value];
            string symbol = IsBaseSlot(row, col) ? " ○ " : "   ";
            CellInfo baseCell = new CellInfo(symbol, ConsoleColor.White, background);
            return baseCell;
        }

        PlayerColor? startOwner = GetStartOwner(row, col);
        if (startOwner.HasValue)
        {
            CellInfo startCell = new CellInfo(" ★ ", ColorMap[startOwner.Value], ConsoleColor.Black);
            return startCell;
        }

        if (IsNeutralSafeStar(row, col))
        {
            CellInfo safeStarCell = new CellInfo(" ★ ", ConsoleColor.White, ConsoleColor.Black);
            return safeStarCell;
        }

        PlayerColor? homeOwner = GetHomeOwner(row, col);
        if (homeOwner.HasValue)
        {
            CellInfo homeCell = new CellInfo(" ▪ ", ColorMap[homeOwner.Value], ConsoleColor.Black);
            return homeCell;
        }

        if (IsPath(row, col))
        {
            CellInfo pathCell = new CellInfo(" · ", ConsoleColor.DarkGray, ConsoleColor.Black);
            return pathCell;
        }

        CellInfo emptyCell = new CellInfo("   ", ConsoleColor.Black, ConsoleColor.Black);
        return emptyCell;
    }

    private static CellInfo RenderFinishCell(int row, int col)
    {
        if (col == 7 && row == 7)
        {
            CellInfo centerStarCell = new CellInfo(" ★ ", ConsoleColor.Magenta, ConsoleColor.Black);
            return centerStarCell;
        }
        if (col == 7 && row == 6)
        {
            CellInfo blueArrowCell = new CellInfo(" ▼ ", ConsoleColor.Blue, ConsoleColor.Black);
            return blueArrowCell;
        }
        if (col == 7 && row == 8)
        {
            CellInfo greenArrowCell = new CellInfo(" ▲ ", ConsoleColor.Green, ConsoleColor.Black);
            return greenArrowCell;
        }
        if (col == 6 && row == 7)
        {
            CellInfo redArrowCell = new CellInfo(" ▶ ", ConsoleColor.Red, ConsoleColor.Black);
            return redArrowCell;
        }
        if (col == 8 && row == 7)
        {
            CellInfo yellowArrowCell = new CellInfo(" ◀ ", ConsoleColor.Yellow, ConsoleColor.Black);
            return yellowArrowCell;
        }
        CellInfo defaultFinishCell = new CellInfo(" ◇ ", ConsoleColor.DarkMagenta, ConsoleColor.Black);
        return defaultFinishCell;
    }

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
