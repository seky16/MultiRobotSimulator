namespace MultiRobotSimulator.Core.Models
{
    public class Tile
    {
        public bool IsFinish { get; set; }
        public bool IsStart { get; set; }
        public bool Passable { get; set; } = false;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
    }
}
