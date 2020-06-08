#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.Core.Algos
{
    public class DijkstraAlgo : AbstractSingleRobotAlgo
    {
        private Dictionary<AbstractTile, double> dist;
        private Dictionary<AbstractTile, AbstractTile> prev;
        private List<AbstractTile> Q;

        public override string Name => "Dijkstra's algorithm";

        public override void Initialize()
        {
            var tilesCount = Graph.Vertices.Count();
            dist = new Dictionary<AbstractTile, double>(tilesCount);
            prev = new Dictionary<AbstractTile, AbstractTile>(tilesCount);
            Q = new List<AbstractTile>(tilesCount);

            foreach (var v in Graph.Vertices)
            {
                dist[v] = double.PositiveInfinity;
                prev[v] = null;
                Q.Add(v);
            }

            dist[Robot.Start] = 0;
        }

        public override void RunSearch()
        {
            AbstractTile u = null;
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

                    var alt = dist[u] + Helpers.Metrics.Euclidean(u, v);

                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }

            if (u is null) throw new InvalidOperationException();

            if (prev[u] != null || u == Robot.Start)
            {
                while (u != null)
                {
                    Robot.Path.Add(u);
                    u = prev[u];
                }
            }

            Robot.Path.Reverse();
        }
    }
}
