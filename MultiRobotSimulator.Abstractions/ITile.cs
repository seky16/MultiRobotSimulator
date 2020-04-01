namespace MultiRobotSimulator.Abstractions
{
    public interface ITile
    {
        bool IsStart { get; set; }
        bool IsTarget { get; set; }
        bool Passable { get; set; }
        int X { get; set; }
        int Y { get; set; }
    }
}
