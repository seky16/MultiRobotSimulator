using System;
using System.IO;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Factories
{
    public class MapFactory : IMapFactory
    {
        public Map CreateMap(int width, int height) => new Map(width, height);

        public Map FromText(TextReader textReader)
        {
            static FormatException getInvalidHeaderEx() => new FormatException("Invalid header.");

            var line = textReader.ReadLine();
            if (line?.Equals(Map.TypeStr, StringComparison.Ordinal) != true)
            {
                throw getInvalidHeaderEx();
            }

            line = textReader.ReadLine();
            if (line?.StartsWith(Map.HeightStr, StringComparison.Ordinal) != true || !int.TryParse(line.Substring(Map.HeightStr.Length), out var height))
            {
                throw getInvalidHeaderEx();
            }

            line = textReader.ReadLine();
            if (line?.StartsWith(Map.WidthStr, StringComparison.Ordinal) != true || !int.TryParse(line.Substring(Map.WidthStr.Length), out var width))
            {
                throw getInvalidHeaderEx();
            }

            line = textReader.ReadLine();
            if (line?.Equals(Map.MapStr, StringComparison.Ordinal) != true)
            {
                throw getInvalidHeaderEx();
            }

            var map = CreateMap(width, height);

            for (var y = 0; y < height; y++)
            {
                line = textReader.ReadLine() ?? string.Empty;
                for (var x = 0; x < width; x++)
                {
                    var t = map.GetTileAtPos(x, y);

                    if (!(t is Tile tile))
                    {
                        throw new InvalidOperationException($"Couldn't find tile at [{x};{y}]");
                    }

                    var c = line[x];
                    switch (c)
                    {
                        case '.':
                        case 'G':
                        case 'S':
                            // passable
                            break;

                        case '@':
                        case 'O':
                        case 'T':
                        case 'W':
                            // obstacle
                            map.AddToTile(tile, Enums.DrawingMode.Obstacle);
                            break;

                        default:
                            throw new InvalidOperationException($"Character '{c}' wasn't recognized");
                    }
                }
            }

            return map;
        }
    }
}
