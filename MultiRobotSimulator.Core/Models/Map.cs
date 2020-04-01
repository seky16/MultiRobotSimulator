using System;
using System.Globalization;
using System.Text;
using MultiRobotSimulator.Abstractions;

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
            Graph = new Graph(width, height);
        }

        public Graph Graph { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public bool EmptyTiles()
        {
            var result = false;
            foreach (Tile tile in Graph.Vertices)
            {
                result |= tile.SetToDefault();
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
            ITile? iTile;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    iTile = Graph.GetTileAtPos(x, y);

                    if (!(iTile is Tile tile))
                    {
                        throw new InvalidOperationException($"Couldn't find tile at [{x};{y}]");
                    }

                    sb.Append(tile.Passable ? "." : "@");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
