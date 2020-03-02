using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiRobotSimulator.Core.Models
{
    public class Map
    {
        private const string heightStr = "height ";
        private const string mapStr = "map";
        private const string typeStr = "type octile";
        private const string widthStr = "width ";

        //private const string logMsg = "{Function} failed: {Reason}";
        //private readonly ILogger _logger;
        private readonly Func<Tile> _tileFactory;

        public Map(Func<Tile> tileFactory/*, ILogger<Map> logger*/)
        {
            _tileFactory = tileFactory;
            //_logger = logger;
        }

        public List<Tile> Finishes { get; } = new List<Tile>();

        public int Height { get; set; } = -1;

        public List<Tile> Starts { get; } = new List<Tile>();

        public List<Tile> Tiles { get; } = new List<Tile>();

        public int Width { get; set; } = -1;

        public bool AddFinish(Tile tile)
        {
            if (Finishes.Contains(tile))
            {
                return false;
            }

            tile.IsStart = false;
            tile.IsFinish = true;
            tile.Passable = true;

            Finishes.Add(tile);

            if (Starts.Contains(tile))
            {
                Starts.Remove(tile);
            }

            return true;
        }

        public bool AddStart(Tile tile)
        {
            if (Starts.Contains(tile))
            {
                return false;
            }

            tile.IsStart = true;
            tile.IsFinish = false;
            tile.Passable = true;

            Starts.Add(tile);

            if (Finishes.Contains(tile))
            {
                Finishes.Remove(tile);
            }

            return true;
        }

        public void EmptyTiles()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tile = Tiles.SingleOrDefault(t => t.X == x && t.Y == y);
                    if (tile is null)
                    {
                        tile = _tileFactory();
                        tile.X = x;
                        tile.Y = y;
                        Tiles.Add(tile);
                    }

                    tile.Passable = true;
                }
            }
        }

        public bool ReadHeader(TextReader textReader)
        {
            var line = textReader.ReadLine();
            if (line?.Equals(typeStr, StringComparison.Ordinal) != true)
            {
                //_logger.LogError(logMsg, nameof(ReadHeader), "type");
                return false;
            }

            line = textReader.ReadLine();
            if (line?.StartsWith(heightStr, StringComparison.Ordinal) != true || !int.TryParse(line.Substring(heightStr.Length), out var height))
            {
                //_logger.LogError(logMsg, nameof(ReadHeader), "height");
                return false;
            }

            line = textReader.ReadLine();
            if (line?.StartsWith(widthStr, StringComparison.Ordinal) != true || !int.TryParse(line.Substring(widthStr.Length), out var width))
            {
                //_logger.LogError(logMsg, nameof(ReadHeader), "width");
                return false;
            }

            line = textReader.ReadLine();
            if (line?.Equals(mapStr, StringComparison.Ordinal) != true)
            {
                //_logger.LogError(logMsg, nameof(ReadHeader), "map");
                return false;
            }

            Height = height;
            Width = width;
            return true;
        }

        public bool ReadMap(TextReader textReader)
        {
            string line;

            try
            {
                for (var y = 0; y < Height; y++)
                {
                    line = textReader.ReadLine() ?? string.Empty;
                    for (var x = 0; x < Width; x++)
                    {
                        var tile = _tileFactory();
                        tile.X = x;
                        tile.Y = y;

                        var c = line[x];
                        switch (c)
                        {
                            case '.':
                            case 'G':
                            case 'S':
                                tile.Passable = true;
                                break;

                            case '@':
                            case 'O':
                            case 'T':
                            case 'W':
                                tile.Passable = false;
                                break;

                            default:
                                throw new InvalidOperationException($"Character '{c}' wasn't recognized");
                        }

                        Tiles.Add(tile);
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, logMsg, nameof(ReadMap), null);
                return false;
            }

            return true;
        }

        public bool RemoveFinish(Tile tile)
        {
            if (!Finishes.Contains(tile))
            {
                return false;
            }

            tile.IsFinish = false;

            Finishes.Remove(tile);

            return true;
        }

        public bool RemoveStart(Tile tile)
        {
            if (!Starts.Contains(tile))
            {
                return false;
            }

            tile.IsStart = false;

            Starts.Remove(tile);

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            // header
            sb.AppendLine(typeStr);
            sb.Append(heightStr).AppendLine(Height.ToString());
            sb.Append(widthStr).AppendLine(Width.ToString());
            sb.AppendLine(mapStr);

            // map
            Tile tile;
            for (var row = 0; row < Height; row++)
            {
                for (var col = 0; col < Width; col++)
                {
                    tile = Tiles[(row * Width) + col];
                    sb.Append(tile.Passable ? "." : "@");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
