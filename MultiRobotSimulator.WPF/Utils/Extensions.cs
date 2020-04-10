using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Enums;

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

        public static EditorAction GetEditorAction(this MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    return EditorAction.Remove;
                }
                else
                {
                    return EditorAction.Nothing;
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.RightButton == MouseButtonState.Released)
                {
                    return EditorAction.Add;
                }
                else
                {
                    return EditorAction.Nothing;
                }
            }

            return EditorAction.Nothing;
        }

        public static Rect GetRect(this AbstractTile tile, double cellSize = 1)
        {
            return new Rect(tile.X * cellSize, tile.Y * cellSize, cellSize, cellSize);
        }

        public static AbstractTile? GetTileOnScreenPos(this IEnumerable<AbstractTile> tiles, Point pos, double cellSize)
        {
            return tiles.FirstOrDefault(t => t.GetRect(cellSize).Contains(pos));
        }

        public static Point ScreenToCanvas(this Point screenPos, double cellSize)
        {
            return new Point(screenPos.X / cellSize, screenPos.Y / cellSize);
        }
    }
}
