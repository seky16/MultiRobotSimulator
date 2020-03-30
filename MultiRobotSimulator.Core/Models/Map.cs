using System;
using System.Globalization;
using System.Linq;
using System.Text;
using QuickGraph;

namespace MultiRobotSimulator.Core.Models
{
    public class Map
    {
        public const string HeightStr = "height ";

        public const string MapStr = "map";

        public const string TypeStr = "type octile";

        public const string WidthStr = "width ";

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Graph = new UndirectedGraph<Tile, SEdge<Tile>>(false);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tile = new Tile() { X = x, Y = y };
                    Graph.AddVertex(tile);
                }
            }

            foreach (var tile in Graph.Vertices)
            {
                for (var yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (var xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        var neighbour = GetTileAtPos(tile.X + xOffset, tile.Y + yOffset);
                        if (neighbour != null && neighbour != tile)
                        {
                            Graph.AddEdge(new SEdge<Tile>(tile, neighbour));
                        }
                    }
                }
            }
        }

        public UndirectedGraph<Tile, SEdge<Tile>> Graph { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public bool EmptyTiles()
        {
            var result = false;
            foreach (var tile in Graph.Vertices)
            {
                result |= tile.Empty();
            }

            return result;
        }

        public string GetString()
        {
            var sb = new StringBuilder();

            // header
            sb.AppendLine(TypeStr);
            sb.Append(HeightStr).AppendLine(Height.ToString(CultureInfo.InvariantCulture));
            sb.Append(WidthStr).AppendLine(Width.ToString(CultureInfo.InvariantCulture));
            sb.AppendLine(MapStr);

            // map
            Tile? tile;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    tile = GetTileAtPos(x, y);

                    if (tile is null)
                    {
                        throw new InvalidOperationException($"Couldn't find tile at [{x};{y}]");
                    }

                    sb.Append(tile.Passable ? "." : "@");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public Tile? GetTileAtPos(int x, int y) => Graph.Vertices.SingleOrDefault(t => t.X == x && t.Y == y);
    }
}
