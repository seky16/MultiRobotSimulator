#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Algos
{
    public class DStarLite : AbstractMultiRobotAlgo
    {
        public override string Name => "D* Lite";

        public override void Initialize()
        {
        }

        public override Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new DStarRobot(start, target);
        }

        public override void RunSearch()
        {
            var dStarRobots = Robots.Cast<DStarRobot>();
            foreach (var robot in dStarRobots)
            {
                robot.Initialize(Graph);
            }

            var enRoute = dStarRobots.Where(r => r.Position != null && r.Position != r.Target).ToList();
            while (enRoute.Count > 0)
            {
                foreach (var robot in enRoute)
                {
                    var occupied = Graph.AdjacentVertices(robot.Position).Where(v => dStarRobots.Any(r => r.Position == v));
                    robot.Step(occupied);
                }

                enRoute = dStarRobots.Where(r => r.Position != null && r.Position != r.Target).ToList();
            }
        }
    }

    public class DStarRobot : Robot
    {
        private readonly Dictionary<AbstractTile, double> _gScore = new Dictionary<AbstractTile, double>();
        private readonly Dictionary<AbstractTile, double> _rhs = new Dictionary<AbstractTile, double>();
        private readonly SimplePriorityQueue<AbstractTile, (double, double)> _u = new SimplePriorityQueue<AbstractTile, (double, double)>();
        private IGraph _graph;
        private double _km;
        private AbstractTile _last;
        private IEnumerable<AbstractTile> _occupied = Enumerable.Empty<AbstractTile>();

        public DStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
            _last = Position = start;
            Path.Add(start);
        }

        public AbstractTile Position { get; set; }

        public void Initialize(IGraph graph)
        {
            _graph = graph;
            _km = 0;
            _rhs[Target] = 0;
            _u.Enqueue(Target, CalculateKey(Target));
            ComputeShortestPath();
        }

        public void Step(IEnumerable<AbstractTile> occupied)
        {
            if (Position != Target || Position != null)
            {
                if (_occupied.Except(occupied).Any() || occupied.Except(_occupied).Any())
                {
                    _occupied = occupied;

                    _km += Metrics.Octile(_last, Position);
                    _last = Position;

                    foreach (var u in occupied)
                    {
                        UpdateVertex(u);
                    }

                    ComputeShortestPath();
                }

                Position = GetNext();
                if (Position != null)
                {
                    Path.Add(Position);
                }
            }
        }

        private (double, double) CalculateKey(AbstractTile s)
        {
            var g = _gScore.GetValueOrDefault(s, double.PositiveInfinity);
            var rhs = _rhs.GetValueOrDefault(s, double.PositiveInfinity);
            var min = Math.Min(g, rhs);
            return (min + Metrics.Octile(Position, s) + _km, min);
        }

        private void ComputeShortestPath()
        {
            while (_u.Count > 0 && (_u.GetPriority(_u.First).CompareTo(CalculateKey(Position)) < 0 || _rhs.GetValueOrDefault(Position, double.PositiveInfinity) != _gScore.GetValueOrDefault(Position, double.PositiveInfinity)))
            {
                var kOld = _u.GetPriority(_u.First);
                var u = _u.Dequeue();
                var kU = CalculateKey(u);
                var rhs = _rhs.GetValueOrDefault(u, double.PositiveInfinity);
                if (kOld.CompareTo(kU) < 0)
                {
                    _u.Enqueue(u, kU);
                }
                else if (_gScore.GetValueOrDefault(u, double.PositiveInfinity) > rhs)
                {
                    _gScore[u] = rhs;
                    foreach (var s in _graph.AdjacentVertices(u))
                    {
                        UpdateVertex(s);
                    }
                }
                else
                {
                    _gScore[u] = double.PositiveInfinity;
                    UpdateVertex(u);
                    foreach (var s in _graph.AdjacentVertices(u))
                    {
                        UpdateVertex(s);
                    }
                }
            }
        }

        private double Cost(AbstractTile u, AbstractTile v)
        {
            if (_occupied.Contains(u) || _occupied.Contains(v))
            {
                return double.PositiveInfinity;
            }

            return Metrics.Octile(u, v);
        }

        private AbstractTile GetNext()
        {
            var min = double.PositiveInfinity;
            AbstractTile tile = null;
            foreach (var s in _graph.AdjacentVertices(Position))
            {
                var f = _gScore.GetValueOrDefault(s, double.PositiveInfinity) + Cost(Position, s);
                if (f < min)
                {
                    min = f;
                    tile = s;
                }
            }

            return tile;
        }

        private void UpdateVertex(AbstractTile u)
        {
            if (u != Target)
            {
                _rhs[u] = _graph.AdjacentVertices(u).Min(s => _gScore.GetValueOrDefault(s, double.PositiveInfinity) + Cost(u, s));
            }
            if (_u.Contains(u))
            {
                _u.Remove(u);
            }
            if (_gScore.GetValueOrDefault(u, double.PositiveInfinity) != _rhs.GetValueOrDefault(u, double.PositiveInfinity))
            {
                _u.Enqueue(u, CalculateKey(u));
            }
        }
    }
}
