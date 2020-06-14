#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using SD.Tools.Algorithmia.PriorityQueues;

namespace MultiRobotSimulator.Core.Algos
{
    public class CoopAStarRobot
    {
        private readonly Dictionary<AbstractTile, AbstractTile> _cameFrom = new Dictionary<AbstractTile, AbstractTile>();
        private readonly HashSet<AbstractTile> _closed = new HashSet<AbstractTile>();
        private readonly Dictionary<AbstractTile, double> _fScore = new Dictionary<AbstractTile, double>();
        private readonly Dictionary<AbstractTile, double> _gScore = new Dictionary<AbstractTile, double>();
        private readonly BinaryHeapPriorityQueue<AbstractTile> _open;
        private readonly Robot _robot;
        private readonly Dictionary<AbstractTile, int> _time = new Dictionary<AbstractTile, int>();
        private readonly Dictionary<AbstractTile, int> _wait = new Dictionary<AbstractTile, int>();

        public CoopAStarRobot(Robot robot)
        {
            _robot = robot;
            _open = new BinaryHeapPriorityQueue<AbstractTile>((a, b) => _fScore.GetValueOrDefault(b, double.PositiveInfinity).CompareTo(_fScore.GetValueOrDefault(a, double.PositiveInfinity)));
        }

        public List<AbstractTile> Path => _robot.Path;
        public AbstractTile Start => _robot.Start;
        public AbstractTile Target => _robot.Target;

        public void Search(IGraph graph, ref HashSet<(int, int, int)> reservationTable)
        {
            _open.Add(Start);
            _gScore[Start] = 0;
            _fScore[Start] = Helpers.Metrics.Manhattan(Start, Target);

            AbstractTile current;
            while (_open.Count > 0)
            {
                current = _open.Remove();
                _closed.Add(current);

                if (current == Target)
                {
                    Path.AddRange(ReconstructPath(current));

                    for (var t = 0; t < Path.Count; t++)
                    {
                        reservationTable.Add((Path[t].X, Path[t].Y, t));
                    }

                    return;
                }

                var time = _time.GetValueOrDefault(current, 0);

                var added = false;
                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (_closed.Contains(neighbor)
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time))
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time + 1)))
                    {
                        continue;
                    }

                    var gScore = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + 1;
                    if (gScore < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = gScore;
                        _fScore[neighbor] = gScore + Helpers.Metrics.Manhattan(neighbor, Target);
                        _time[neighbor] = time + 1;
                        added = true;

                        if (!_open.Contains(neighbor))
                        {
                            _open.Add(neighbor);
                        }
                    }
                }

                // wait
                if (!added)
                {
                    _gScore[current] = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + 1;
                    _fScore[current] = _gScore[current] + Helpers.Metrics.Manhattan(current, Target);
                    _open.Add(current);
                    _closed.Remove(current);
                    _time[current] = time + 1;
                    _wait[current] = _wait.GetValueOrDefault(current, 0) + 1;
                }
            }
        }

        private List<AbstractTile> ReconstructPath(AbstractTile current)
        {
            var path = new List<AbstractTile>() { current };

            for (var i = 0; i < _wait.GetValueOrDefault(current, 0); i++)
            {
                path.Add(current);
            }

            while (_cameFrom.TryGetValue(current, out var prev))
            {
                if (path.Count > 100)
                {
                    break;
                }

                current = prev;
                path.Add(current);
            }

            path.Reverse();

            return path;
        }
    }

    public class CooperativeAStar : AbstractMultiRobotAlgo
    {
        private List<CoopAStarRobot> _coopAStarRobots;
        private HashSet<(int, int, int)> _reservationTable;
        public override string Name => "Cooperative A*";

        public override void Initialize()
        {
            _reservationTable = new HashSet<(int, int, int)>();
            _coopAStarRobots = new List<CoopAStarRobot>(Robots.Count);
            foreach (var robot in Robots)
            {
                _coopAStarRobots.Add(new CoopAStarRobot(robot));
            }
        }

        public override void RunSearch()
        {
            foreach (var robot in _coopAStarRobots)
            {
                robot.Search(Graph.Clone(), ref _reservationTable);
            }
        }
    }
}
