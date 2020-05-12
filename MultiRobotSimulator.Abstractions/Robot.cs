using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public class Robot
    {
        public Robot(AbstractTile start, AbstractTile target)
        {
            Start = start;
            Target = target;
            Path = new List<AbstractTile>();
        }

        public List<AbstractTile> Path { get; }
        public AbstractTile Start { get; }
        public AbstractTile Target { get; }
    }
}
