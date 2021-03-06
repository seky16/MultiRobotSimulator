﻿#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Helpers;

namespace MultiRobotSimulator.Core.Algos
{
    public class RRAStar
    {
        private readonly HashSet<AbstractTile> _closed = new HashSet<AbstractTile>();
        private readonly IGraph _graph;
        private readonly Dictionary<AbstractTile, double> _gScore = new Dictionary<AbstractTile, double>();
        private readonly Priority_Queue.SimplePriorityQueue<AbstractTile, double> _open;
        private readonly AbstractTile _start;

        public RRAStar(IGraph graph, AbstractTile start, AbstractTile target)
        {
            _graph = graph;
            _start = start;
            _open = new Priority_Queue.SimplePriorityQueue<AbstractTile, double>();

            _gScore[target] = 0;
            _open.Enqueue(target, Metrics.Octile(target, start));

            Resume(start);
        }

        public double AbstractDist(AbstractTile n)
        {
            if (_closed.Contains(n))
            {
                return _gScore.GetValueOrDefault(n, double.PositiveInfinity);
            }

            if (Resume(n))
            {
                return _gScore.GetValueOrDefault(n, double.PositiveInfinity);
            }

            return double.PositiveInfinity;
        }

        private bool Resume(AbstractTile n)
        {
            while (_open.Count > 0)
            {
                var p = _open.Dequeue();
                _closed.Add(p);

                if (p == n)
                {
                    return true;
                }

                foreach (var q in _graph.AdjacentVertices(p).Reverse())
                {
                    var gScore = _gScore.GetValueOrDefault(p, double.PositiveInfinity) + Metrics.Octile(p, q);
                    var fScore = gScore + Metrics.Octile(q, _start);

                    if (!_open.Contains(q) && !_closed.Contains(q))
                    {
                        _open.Enqueue(q, double.PositiveInfinity);
                    }

                    if (_open.Contains(q) && fScore < _open.GetPriority(q))
                    {
                        _gScore[q] = gScore;
                        _open.UpdatePriority(q, fScore);
                    }
                }
            }

            return false;
        }
    }
}
