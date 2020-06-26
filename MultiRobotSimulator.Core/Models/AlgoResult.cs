using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;

namespace MultiRobotSimulator.Core.Models
{
    public class AlgoResult
    {
        public AlgoResult(Map map, IAlgo algo, long initTime, long searchTime)
        {
            InitTime = initTime;
            SearchTime = searchTime;

            Robots = algo.Robots.Select(r => new RobotResult(r, map)).ToList();
        }

        public double AveragePathLength => Robots.Where(r => r.Success).Sum(r => r.PathLength) / Robots.Count(r => r.Success);
        public int Expanded => Robots.Where(r => r.Success).Sum(r => r.Expanded) / Robots.Count(r => r.Success);
        public long InitTime { get; }
        public IReadOnlyCollection<RobotResult> Robots { get; }
        public long SearchTime { get; }
        public double ShortestPathLength => Robots.Where(r => r.Success).Min(r => r.PathLength);
        public int Successful => Robots.Count(r => r.Success);
    }

    public class RobotResult
    {
        public RobotResult(Robot robot, Map map)
        {
            if (robot.Path is null || robot.Path.Count == 0 || robot.Path[0] != robot.Start || robot.Path[^1] != robot.Target)
            {
                return;
            }

            double length = 0;
            var first = robot.Path[0];
            for (var i = 1; i < robot.Path.Count; i++)
            {
                var second = robot.Path[i];

                if (!map.ContainsEdge((first, second)) && !first.Equals(second))
                {
                    return;
                }

                length += Metrics.Octile(first, second);

                first = second;
            }

            Success = true;
            Path = robot.Path;
            PathLength = length;
        }

        public int Expanded { get; }
        public IReadOnlyCollection<AbstractTile> Path { get; } = new List<AbstractTile>();
        public double PathLength { get; }
        public bool Success { get; }
    }
}
