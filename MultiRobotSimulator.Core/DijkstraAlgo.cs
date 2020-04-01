using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.Core
{
    public class DijkstraAlgo : AbstractSingleRobotAlgo
    {
        private readonly Dictionary<ITile, double> dist = new Dictionary<ITile, double>();
        private readonly Dictionary<ITile, ITile?> prev = new Dictionary<ITile, ITile?>();
        private readonly List<ITile> Q = new List<ITile>();

        public override string Name => "Dijkstra's algorithm";

        public override void RunSearch()
        {
            if (Graph is null) throw new InvalidOperationException();

            foreach (var v in Graph.Vertices)
            {
                dist[v] = double.PositiveInfinity;
                prev[v] = null;
                Q.Add(v);
            }

            dist[Start] = 0;

            ITile? u = null;
            while (Q.Count > 0)
            {
                u = Q.OrderBy(t => dist[t]).First();
                Q.Remove(u);

                if (u.IsTarget)
                {
                    break;
                }

                foreach (var v in Graph.AdjacentVertices(u))
                {
                    if (!Q.Contains(v)) continue;

                    var alt = dist[u] + 1;

                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }

            if (u is null) throw new InvalidOperationException();

            if (prev[u] != null || u.IsStart)
            {
                while (u != null)
                {
                    Path.Add(u);
                    u = prev[u];
                }
            }

            Path.Reverse();
        }
    }
}
