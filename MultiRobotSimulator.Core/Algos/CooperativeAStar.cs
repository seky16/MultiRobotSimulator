#nullable disable

using System;
using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;

namespace MultiRobotSimulator.Core.Algos
{
    public class CoopAStarRobot
    {
        private readonly Dictionary<AbstractTile, AbstractTile> _cameFrom = new Dictionary<AbstractTile, AbstractTile>();
        private readonly HashSet<AbstractTile> _closed = new HashSet<AbstractTile>();
        private readonly Dictionary<AbstractTile, double> _gScore = new Dictionary<AbstractTile, double>();
        private readonly Priority_Queue.SimplePriorityQueue<AbstractTile, double> _open = new Priority_Queue.SimplePriorityQueue<AbstractTile, double>();
        private readonly Robot _robot;
        private readonly Dictionary<AbstractTile, int> _time = new Dictionary<AbstractTile, int>();
        private readonly Dictionary<AbstractTile, int> _wait = new Dictionary<AbstractTile, int>();

        public CoopAStarRobot(Robot robot)
        {
            _robot = robot;
        }

        public List<AbstractTile> Path => _robot.Path;
        public AbstractTile Start => _robot.Start;
        public AbstractTile Target => _robot.Target;

        public virtual void Search(IGraph graph, ref HashSet<(int, int, int)> reservationTable, Func<AbstractTile, AbstractTile, double> h)
        {
            _gScore[Start] = 0;
            _open.Enqueue(Start, h(Start, Target));

            AbstractTile current;
            while (_open.Count > 0)
            {
                current = _open.Dequeue();
                _closed.Add(current);

                var time = _time.GetValueOrDefault(current, 0);

                // current tile is reserved, we can't be here
                if (current != Start && (reservationTable.Contains((current.X, current.Y, time))
                    || reservationTable.Contains((current.X, current.Y, time + 1))))
                {
                    continue;
                }

                if (current == Target)
                {
                    Path.AddRange(ReconstructPath(current));

                    for (var t = 0; t < Path.Count; t++)
                    {
                        reservationTable.Add((Path[t].X, Path[t].Y, t));
                    }

                    return;
                }

                var added = false;
                var tempG = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + 1;
                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (_closed.Contains(neighbor)
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time))
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time + 1)))
                    {
                        continue;
                    }

                    if (tempG < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = tempG;
                        _time[neighbor] = time + 1;
                        added = true;

                        var fScore = tempG + h(neighbor, Target);
                        if (!_open.TryUpdatePriority(neighbor, fScore))
                        {
                            _open.Enqueue(neighbor, fScore);
                        }
                    }
                }

                // wait
                if (!added)
                {
                    _gScore[current] = tempG;
                    _open.Enqueue(current, tempG + h(current, Target));
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
                _reservationTable.Add((robot.Start.X, robot.Start.Y, 0));
                _coopAStarRobots.Add(new CoopAStarRobot(robot));
            }
        }

        public override void RunSearch()
        {
            foreach (var robot in _coopAStarRobots)
            {
                robot.Search(Graph.Clone(), ref _reservationTable, Metrics.Octile);
            }
        }
    }
}
