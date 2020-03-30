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
            static FormatException getInvalidHeaderEx(Exception? innerException = null) => new FormatException("Invalid header.", innerException);

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
                    var tile = map.GetTileAtPos(x, y);

                    if (tile is null)
                    {
                        throw new InvalidOperationException($"Couldn't find tile at [{x};{y}]");
                    }

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
                }
            }

            return map;
        }
    }
}
