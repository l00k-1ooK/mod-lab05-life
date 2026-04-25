using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class BoardSettings
    {
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 20;
        public int CellSize { get; set; } = 1;
        public double LiveDensity { get; set; } = 0.5;
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize,
            double liveDensity = 0.1)
        {
            CellSize = cellSize;
            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();
            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }
        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        public int CountAlive()
        {
            int count = 0;
            foreach (var cell in Cells)
                if (cell.IsAlive) count++;
            return count;
        }
        public int CountGroups()
        {
            bool[,] visited = new bool[Columns, Rows];
            int groups = 0;
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        FloodFill(x, y, visited);
                        groups++;
                    }
                }
            }
            return groups;
        }
        private void FloodFill(int x, int y, bool[,] visited)
        {
            if (x < 0 || x >= Columns || y < 0 || y >= Rows)
                return;
            if (visited[x, y] || !Cells[x, y].IsAlive)
                return;
            visited[x, y] = true;
            FloodFill(x - 1, y, visited);
            FloodFill(x + 1, y, visited);
            FloodFill(x, y - 1, visited);
            FloodFill(x, y + 1, visited);
            FloodFill(x - 1, y - 1, visited);
            FloodFill(x + 1, y - 1, visited);
            FloodFill(x - 1, y + 1, visited);
            FloodFill(x + 1, y + 1, visited);
        }
        public void SaveToFile(string path)
        {
            var sb = new StringBuilder();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                    sb.Append(Cells[col, row].IsAlive ? '1' : '0');
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }
        public void LoadFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            for (int row = 0; row < Rows && row < lines.Length; row++)
            {
                for (int col = 0; col < Columns
                    && col < lines[row].Length; col++)
                {
                    Cells[col, row].IsAlive = lines[row][col] == '*';
                }
            }
        }
        public int GenerationsToStable(int windowSize = 10)
        {
            var history = new Queue<int>();
            int gen = 0;
            while (true)
            {
                int alive = CountAlive();
                history.Enqueue(alive);
                if (history.Count > windowSize)
                    history.Dequeue();
                if (history.Count == windowSize
                    && history.Max() == history.Min())
                    return gen;
                Advance();
                gen++;
                if (gen > 2000) return gen;
            }
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;
                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }
    class Program
    {
        static Board board;
        static BoardSettings settings;

        static readonly string DataDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "Data", "data"));

        static void LoadSettings()
        {
            string path = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "settings.json"));

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                settings = JsonSerializer.Deserialize<BoardSettings>(json)
                    ?? new BoardSettings();
            }
            else
            {
                settings = new BoardSettings();
                var json = JsonSerializer.Serialize(settings,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
        }
        static void Reset()
        {
            board = new Board(
                width: settings.Width,
                height: settings.Height,
                cellSize: settings.CellSize,
                liveDensity: settings.LiveDensity);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    Console.Write(cell.IsAlive ? '*' : ' ');
                }
                Console.Write('\n');
            }
        }
        static void RunResearch()
        {
            var densities = new double[]
            {
                0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9
            };
            int attempts = 5;
            var results = new List<string>();
            results.Add("density\tavg_generations");
            foreach (var density in densities)
            {
                int total = 0;
                for (int i = 0; i < attempts; i++)
                {
                    var b = new Board(50, 20, 1, density);
                    total += b.GenerationsToStable();
                }
                int avg = total / attempts;
                results.Add(
                    $"{density:F1}\t{avg}".Replace(",", "."));
                Console.WriteLine(
                    $"Density {density:F1} -> avg {avg} gen");
            }

            string dataDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "Data"));
            Directory.CreateDirectory(dataDir);

            string dataFile = Path.Combine(dataDir, "data.txt");
            File.WriteAllLines(dataFile, results);
            Console.WriteLine($"Saved: {dataFile}");

            var xs = new List<double>();
            var ys = new List<double>();
            foreach (var line in results.Skip(1))
            {
                var parts = line.Split('\t');
                if (parts.Length == 2)
                {
                    xs.Add(double.Parse(parts[0],
                        System.Globalization.CultureInfo.InvariantCulture));
                    ys.Add(double.Parse(parts[1],
                        System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            var plt = new ScottPlot.Plot(600, 400);
            plt.AddScatter(xs.ToArray(), ys.ToArray());
            plt.Title("Поколений до стабилизации");
            plt.XLabel("Плотность заполнения");
            plt.YLabel("Среднее число поколений");

            string plotPath = Path.Combine(dataDir, "plot.png");
            plt.SaveFig(plotPath);
            Console.WriteLine($"Plot saved to {plotPath}");
        }
        static void Main(string[] args)
        {
            LoadSettings();
            Directory.CreateDirectory(DataDir);
            if (args.Length > 0 && args[0] == "--research")
            {
                RunResearch();
                return;
            }
            if (args.Length > 0 && args[0] == "--load"
                && args.Length > 1)
            {
                board = new Board(
                    settings.Width, settings.Height,
                    settings.CellSize);
                board.LoadFromFile(args[1]);
            }
            else
            {
                Reset();
            }
            int generation = 0;
            int saveInterval = 10;
            int stableWindow = 10;
            var history = new Queue<int>();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
                    {
                        Console.Clear();
                        Console.WriteLine(
                            $"Stopped manually at generation {generation}.");
                        break;
                    }
                }

                Console.Clear();
                Console.WriteLine(
                    $"Generation: {generation} | " +
                    $"Alive: {board.CountAlive()} | " +
                    $"Groups: {board.CountGroups()} | " +
                    $"Press Q to stop");
                Render();

                if (generation % saveInterval == 0)
                {
                    string statePath = Path.Combine(
                        DataDir, $"state_{generation}.txt");
                    board.SaveToFile(statePath);
                }

                int alive = board.CountAlive();
                history.Enqueue(alive);
                if (history.Count > stableWindow)
                    history.Dequeue();

                if (history.Count == stableWindow
                    && history.Max() == history.Min())
                {
                    string finalPath = Path.Combine(
                        DataDir, $"state_{generation}_final.txt");
                    board.SaveToFile(finalPath);
                    Console.Clear();
                    Console.WriteLine(
                        $"Stabilized at generation {generation}! " +
                        $"Alive: {alive}");
                    Console.WriteLine(
                        $"Final state saved to {finalPath}");
                    break;
                }

                board.Advance();
                generation++;
                Thread.Sleep(100);
            }
        }
    }
}