#nullable disable

using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Algos
{
    public class LocalRepairAStar : AbstractMultiRobotAlgo
    {
        private List<LRAStarRobot> _LRAStarRobots;

        public override string Name => "Local Repair A*";

        public override void Initialize()
        {
            _LRAStarRobots = new List<LRAStarRobot>(Robots.Count);
            foreach (var robot in Robots)
            {
                _LRAStarRobots.Add(new LRAStarRobot(robot));
            }
        }

        public override void RunSearch()
        {
            System.Threading.Tasks.Parallel.ForEach(_LRAStarRobots, robot =>
            {
                robot.Search(Graph);
            });

            var enRoute = _LRAStarRobots.Where(r => r.Position != null && r.Position != r.Target);
            while (enRoute.Any())
            {
                foreach (var robot in enRoute)
                {
                    var next = robot.Step(); // get the next position
                    if (next != null && enRoute.Any(r => r != robot && r.Position == next))
                    {
                        // occupied - recalculate remainder of the route
                        var graph = Graph.Clone();
                        graph.RemoveVertex(next);
                        robot.Search(graph);
                    }
                    else
                    {
                        robot.Position = next; // move to the next position
                    }
                }

                enRoute = _LRAStarRobots.Where(r => r.Position != null && r.Position != r.Target);
            }
        }
    }

    public class LRAStarRobot
    {
        private readonly Robot _robot;
        private int _posIndex = 0;

        public LRAStarRobot(Robot robot)
        {
            _robot = robot;
            Path.Add(Start);
            Position = Start;
        }

        public List<AbstractTile> Path => _robot.Path;
        public AbstractTile Position { get; set; }
        public AbstractTile Start => _robot.Start;
        public AbstractTile Target => _robot.Target;

        public void Search(IGraph graph)
        {
            Path.RemoveRange(_posIndex, Path.Count - _posIndex);

            var cameFrom = new Dictionary<AbstractTile, AbstractTile>();
            var gScore = new Dictionary<AbstractTile, double>();
            var closed = new HashSet<AbstractTile>();
            var open = new SimplePriorityQueue<AbstractTile, double>();

            gScore[Position] = 0;
            open.Enqueue(Position, Helpers.Metrics.Octile(Position, Target)); // TODO add agitation noise (see Silver)

            AbstractTile current;
            while (open.Count > 0)
            {
                current = open.Dequeue();
                closed.Add(current);

                if (current == Target)
                {
                    Path.AddRange(ReconstructPath(cameFrom, current));
                    return;
                }

                var tempG = gScore.GetValueOrDefault(current, double.PositiveInfinity) + 1; // TODO add agitation noise (see Silver)
                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (closed.Contains(neighbor))
                    {
                        continue;
                    }

                    if (tempG < gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tempG;

                        var fScore = tempG + Helpers.Metrics.Octile(neighbor, Target);
                        if (!open.TryUpdatePriority(neighbor, fScore))
                        {
                            open.Enqueue(neighbor, fScore);
                        }
                    }
                }
            }
        }

        public AbstractTile Step()
        {
            if (_posIndex < Path.Count)
            {
                return Path[++_posIndex];
            }

            return null;
        }

        private static List<AbstractTile> ReconstructPath(Dictionary<AbstractTile, AbstractTile> cameFrom, AbstractTile current)
        {
            var path = new List<AbstractTile>() { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
