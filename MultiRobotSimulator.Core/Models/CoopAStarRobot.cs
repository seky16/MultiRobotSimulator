#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Models
{
    public class CoopAStarRobot : Robot
    {
        private readonly Dictionary<SpaceTimeNode, SpaceTimeNode> _cameFrom = new Dictionary<SpaceTimeNode, SpaceTimeNode>();
        private readonly HashSet<SpaceTimeNode> _closed = new HashSet<SpaceTimeNode>();
        private readonly Dictionary<SpaceTimeNode, double> _gScore = new Dictionary<SpaceTimeNode, double>();
        private readonly SimplePriorityQueue<SpaceTimeNode, double> _open = new SimplePriorityQueue<SpaceTimeNode, double>();
        private SpaceTimeGraph _graph;

        public CoopAStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
        }

        public virtual void Initialize(SpaceTimeGraph spaceTimeGraph)
        {
            _graph = spaceTimeGraph;
        }

        public virtual void Search(ref HashSet<SpaceTimeNode> reservationTable)
        {
            var startNode = new SpaceTimeNode(Start.X, Start.Y, 0);
            _gScore[startNode] = 0;
            _open.Enqueue(startNode, Heuristic(Start));

            SpaceTimeNode current;
            while (_open.Count > 0)
            {
                current = _open.Dequeue();
                _closed.Add(current);

                // current tile is reserved, we can't be here
                if (!current.Equals(Start) && (reservationTable.Contains(current)
                    || reservationTable.Contains(current.Next())))
                {
                    continue;
                }

                if (current.Equals(Target))
                {
                    Path.AddRange(ReconstructPath(current));

                    for (var t = 0; t < Path.Count; t++)
                    {
                        reservationTable.Add(new SpaceTimeNode(Path[t].X, Path[t].Y, t));
                    }

                    return;
                }

                foreach (var neighbor in _graph.GetNeighbours(current))
                {
                    if (_closed.Contains(neighbor)
                        || reservationTable.Contains(neighbor)
                        || reservationTable.Contains(neighbor.Next()))
                    {
                        continue;
                    }

                    var tentative_gScore = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + Cost(current, neighbor);
                    if (tentative_gScore < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = tentative_gScore;

                        var fScore = tentative_gScore + Heuristic(_graph.GetTile(neighbor));
                        if (!_open.TryUpdatePriority(neighbor, fScore))
                        {
                            _open.Enqueue(neighbor, fScore);
                        }
                    }
                }
            }
        }

        protected virtual double Heuristic(AbstractTile p)
        {
            return Metrics.Octile(p, Target);
        }

        private double Cost(SpaceTimeNode p, SpaceTimeNode q)
        {
            if (q == p.Next())
            {
                // wait node
                return 1;
            }

            return Metrics.Octile(_graph.GetTile(p), _graph.GetTile(q));
        }

        private List<AbstractTile> ReconstructPath(SpaceTimeNode current)
        {
            var path = new List<AbstractTile>() { _graph.GetTile(current) };

            while (_cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(_graph.GetTile(current));
            }

            path.Reverse();

            return path;
        }
    }
}
