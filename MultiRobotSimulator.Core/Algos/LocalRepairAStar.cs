#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using SD.Tools.Algorithmia.PriorityQueues;

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
            foreach (var robot in _LRAStarRobots)
            {
                // run A* from start to target
                robot.Search(Graph, Helpers.Metrics.Euclidean);
            }

            var enRoute = _LRAStarRobots.Where(r => r.Position != r.Target);
            while (enRoute.Any())
            {
                foreach (var robot in enRoute)
                {
                    var next = robot.Step(); // get the next position
                    if (enRoute.Any(r => r != robot && r.Position == next))
                    {
                        // occupied - recalculate remainder of the route
                        robot.Search(Graph, Helpers.Metrics.Euclidean, next);
                    }
                    else
                    {
                        robot.Position = next; // move to the next position
                    }
                }

                enRoute = _LRAStarRobots.Where(r => r.Position != r.Target);
            }
        }
    }

    public class LRAStarRobot
    {
        private Dictionary<AbstractTile, AbstractTile> _cameFrom;

        private Dictionary<AbstractTile, double> _fScore;

        private Dictionary<AbstractTile, double> _gScore;

        private BinaryHeapPriorityQueue<AbstractTile> _open;

        private int _posIndex = 0;
        private readonly Robot _robot;

        public List<AbstractTile> Path => _robot.Path;
        public AbstractTile Start => _robot.Start;
        public AbstractTile Target => _robot.Target;


        public LRAStarRobot(Robot robot)
        {
            _robot = robot;
            Path.Add(Start);
            Position = Start;
        }

        public AbstractTile Position { get; set; }

        public void Search(IGraph graph, Func<AbstractTile, AbstractTile, double> h, AbstractTile restricted = null)
        {
            Path.RemoveRange(_posIndex, Path.Count - _posIndex);

            var tilesCount = graph.Vertices.Count();
            _cameFrom = new Dictionary<AbstractTile, AbstractTile>(tilesCount);
            _fScore = new Dictionary<AbstractTile, double>(tilesCount);
            _gScore = new Dictionary<AbstractTile, double>(tilesCount);
            _open = new BinaryHeapPriorityQueue<AbstractTile>((a, b) => _fScore.GetValueOrDefault(b, double.PositiveInfinity).CompareTo(_fScore.GetValueOrDefault(a, double.PositiveInfinity)))
            {
                Start
            };

            _gScore[Start] = 0;
            _fScore[Start] = h(Start, Target); // TODO add agitation noise (see Silver)

            AbstractTile current;
            while (_open.Count > 0)
            {
                current = _open.Remove();

                if (current == Target)
                {
                    Path.AddRange(ReconstructPath(_cameFrom, current));
                    return;
                }

                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (neighbor == restricted)
                    {
                        continue;
                    }

                    var gScore = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + h(current, neighbor); // TODO add agitation noise (see Silver)
                    if (gScore < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = gScore;
                        _fScore[neighbor] = gScore + h(neighbor, Target);

                        if (!_open.Contains(neighbor))
                        {
                            _open.Add(neighbor);
                        }
                    }
                }
            }

            throw new InvalidOperationException($"Cannot find path from {Position} to {Target}");
        }

        public AbstractTile Step()
        {
            return Path[++_posIndex];
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
