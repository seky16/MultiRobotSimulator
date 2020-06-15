using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.Core.Models
{
    public class AlgoResult
    {
        public AlgoResult(IAlgo dotnetAlgo, long initTime, long searchTime)
        {
            InitTime = initTime;
            SearchTime = searchTime;
            Robots = dotnetAlgo.Robots;
            Paths = dotnetAlgo.Robots.Select(r => r.Path).ToList();

            // TODO better validation and collision check
            foreach (var robot in Robots)
            {
                var success = robot.Path.Count > 1 && robot.Path[0] == robot.Start && robot.Path[^1] == robot.Target;

                if (success)
                {
                    Succesful++;
                }
            }

            ShortestPathLength = Paths.Min(p => p.Distinct().Count());
        }

        public long InitTime { get; }
        public IReadOnlyCollection<IReadOnlyCollection<AbstractTile>> Paths { get; }
        public IReadOnlyCollection<Robot> Robots { get; }
        public long SearchTime { get; }
        public int ShortestPathLength { get; }
        public int Succesful { get; }
    }
}
