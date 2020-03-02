using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.WPF
{
    public static class Extensions
    {
        public static Point CanvasToScreen(this Point canvasPos, double cellSize)
        {
            return new Point(canvasPos.X * cellSize, canvasPos.Y * cellSize);
        }

        public static Point Center(this Rect rect)
        {
            var x = rect.Left + (rect.Width / 2);
            var y = rect.Top + (rect.Height / 2);
            return new Point(x, y);
        }

        public static Rect GetRect(this Tile tile, double cellSize = 1)
        {
            return new Rect(tile.X * cellSize, tile.Y * cellSize, cellSize, cellSize);
        }

        public static Tile? GetTileOnScreenPos(this IEnumerable<Tile> tiles, Point pos, double cellSize)
        {
            return tiles.FirstOrDefault(t => t.GetRect(cellSize).Contains(pos));
        }

        public static Point ScreenToCanvas(this Point screenPos, double cellSize)
        {
            return new Point(screenPos.X / cellSize, screenPos.Y / cellSize);
        }
    }
}
