#nullable disable

using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Algos;
using MultiRobotSimulator.Helpers;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Models
{
    public class WHCAStarRobot : Robot
    {
        private Dictionary<SpaceTimeNode, SpaceTimeNode> _cameFrom;
        private HashSet<SpaceTimeNode> _closed;
        private SpaceTimeGraph _graph;
        private Dictionary<SpaceTimeNode, double> _gScore;
        private SimplePriorityQueue<SpaceTimeNode, double> _open;
        private int _pathIndex = 0;
        private RRAStar _rra;
        private int _w;

        public WHCAStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
        }

        public AbstractTile Position { get; private set; }

        public void Initialize(SpaceTimeGraph graph, int w)
        {
            _rra = new RRAStar(graph.Graph, Start, Target);
            _graph = graph;
            _w = w;
            Position = Start;
            Path.Add(Start);
        }

        internal void Step(ref HashSet<SpaceTimeNode> reservationTable)
        {
            if (Position == Path.Last())
            {
                Search(ref reservationTable);
            }

            if (_pathIndex + 1 < Path.Count)
            {
                Position = Path[++_pathIndex];
            }
        }

        private double Cost(SpaceTimeNode p, SpaceTimeNode q)
        {
            var pTile = _graph.GetTile(p);
            if (p.T == _w)
            {
                return _rra.AbstractDist(pTile);
            }
            if (p == q && q.Equals(Target))
            {
                return 0;
            }

            if (q == p.Next())
            {
                return 1;
            }

            var qTile = _graph.GetTile(q);
            return Metrics.Octile(pTile, qTile);
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
            path.RemoveAt(0); // current position

            return path;
        }

        private void Search(ref HashSet<SpaceTimeNode> reservationTable)
        {
            _cameFrom = new Dictionary<SpaceTimeNode, SpaceTimeNode>();
            _closed = new HashSet<SpaceTimeNode>();
            _gScore = new Dictionary<SpaceTimeNode, double>();
            _open = new SimplePriorityQueue<SpaceTimeNode, double>();

            var pos = new SpaceTimeNode(Position.X, Position.Y, 0);
            _gScore[pos] = 0;
            _open.Enqueue(pos, _rra.AbstractDist(Position));

            SpaceTimeNode current;
            while (_open.Count > 0)
            {
                current = _open.Dequeue();
                _closed.Add(current);

                // current tile is reserved, we can't be here
                if (!current.Equals(Position) && (reservationTable.Contains(new SpaceTimeNode(current.X, current.Y, current.T + _pathIndex))
                    || reservationTable.Contains(new SpaceTimeNode(current.X, current.Y, current.T + _pathIndex + 1))))
                {
                    continue;
                }

                if (current.T == _w)
                {
                    Path.AddRange(ReconstructPath(current));

                    for (var t = _pathIndex; t < Path.Count; t++)
                    {
                        reservationTable.Add(new SpaceTimeNode(Path[t].X, Path[t].Y, t));
                    }

                    return;
                }

                foreach (var neighbor in _graph.GetNeighbours(current))
                {
                    if (_closed.Contains(neighbor)
                        || reservationTable.Contains(new SpaceTimeNode(neighbor.X, neighbor.Y, neighbor.T + _pathIndex))
                        || reservationTable.Contains(new SpaceTimeNode(neighbor.X, neighbor.Y, neighbor.T + _pathIndex + 1)))
                    {
                        continue;
                    }

                    var tentative_gScore = _gScore.GetValueOrDefault(current, double.PositiveInfinity) + Cost(current, neighbor);
                    if (tentative_gScore < _gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = tentative_gScore;

                        var fScore = tentative_gScore + _rra.AbstractDist(_graph.GetTile(neighbor));
                        if (!_open.TryUpdatePriority(neighbor, fScore))
                        {
                            _open.Enqueue(neighbor, fScore);
                        }
                    }
                }
            }
        }
    }
}
