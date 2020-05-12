using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.Core
{
    public class NaiveMultiDijkstra : AbstractMultiRobotAlgo
    {
        private List<Dictionary<AbstractTile, double>> dist;
        private List<Dictionary<AbstractTile, AbstractTile?>> prev;
        private List<List<AbstractTile>> Q;

        public override string Name => nameof(NaiveMultiDijkstra);

        public override void Initialize()
        {
            dist = new List<Dictionary<AbstractTile, double>>();
            prev = new List<Dictionary<AbstractTile, AbstractTile?>>();
            Q = new List<List<AbstractTile>>();
            for (var i = 0; i < Robots.Count; i++)
            {
                dist.Add(new Dictionary<AbstractTile, double>());
                prev.Add(new Dictionary<AbstractTile, AbstractTile?>());
                Q.Add(new List<AbstractTile>());
            }
        }

        public override void RunSearch()
        {
            if (Graph is null) throw new InvalidOperationException();
            Parallel.For(0, Robots.Count, i =>
            {
                var robot = Robots.ElementAt(i);
                foreach (var v in Graph.Vertices)
                {
                    dist[i][v] = double.PositiveInfinity;
                    prev[i][v] = null;
                    Q[i].Add(v);
                }

                dist[i][robot.Start] = 0;

                AbstractTile? u = null;
                while (Q.Count > 0)
                {
                    u = Q[i].OrderBy(t => dist[i][t]).First();
                    Q[i].Remove(u);

                    if (u == robot.Target)
                    {
                        break;
                    }

                    foreach (var v in Graph.AdjacentVertices(u))
                    {
                        if (!Q[i].Contains(v)) continue;

                        var alt = dist[i][u] + Helpers.Metrics.Euclidean(u, v);

                        if (alt < dist[i][v])
                        {
                            dist[i][v] = alt;
                            prev[i][v] = u;
                        }
                    }
                }

                if (u is null) throw new InvalidOperationException();

                if (prev[i][u] != null || u.IsStart)
                {
                    while (u != null)
                    {
                        robot.Path.Add(u);
                        u = prev[i][u];
                    }
                }

                robot.Path.Reverse();
            });
        }
    }
}
