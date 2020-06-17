#nullable disable

using System;
using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Models
{
    public class CoopAStarRobot : Robot
    {
        private readonly Dictionary<AbstractTile, AbstractTile> _cameFrom = new Dictionary<AbstractTile, AbstractTile>();
        private readonly HashSet<AbstractTile> _closed = new HashSet<AbstractTile>();
        private readonly Dictionary<AbstractTile, double> _gScore = new Dictionary<AbstractTile, double>();
        private readonly SimplePriorityQueue<AbstractTile, double> _open = new SimplePriorityQueue<AbstractTile, double>();
        private readonly Dictionary<AbstractTile, int> _time = new Dictionary<AbstractTile, int>();
        private readonly Dictionary<AbstractTile, int> _wait = new Dictionary<AbstractTile, int>();

        public CoopAStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
        }

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
                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (_closed.Contains(neighbor)
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time))
                        || reservationTable.Contains((neighbor.X, neighbor.Y, time + 1)))
                    {
                        continue;
                    }

                    var tentative_gScore = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + Metrics.Octile(current, neighbor);
                    if (tentative_gScore < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = tentative_gScore;
                        _time[neighbor] = time + 1;
                        added = true;

                        var fScore = tentative_gScore + h(neighbor, Target);
                        if (!_open.TryUpdatePriority(neighbor, fScore))
                        {
                            _open.Enqueue(neighbor, fScore);
                        }
                    }
                }

                // wait
                if (!added)
                {
                    _gScore[current] = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + 1;
                    _open.Enqueue(current, _gScore[current] + h(current, Target));
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
}
