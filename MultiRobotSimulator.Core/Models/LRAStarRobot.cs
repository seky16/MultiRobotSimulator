#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using Priority_Queue;

namespace MultiRobotSimulator.Core.Models
{
    public class LRAStarRobot : Robot
    {
        private int _posIndex = 0;

        public LRAStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
            Path.Add(start);
            Position = start;
        }

        public AbstractTile Position { get; set; }

        public void Search(IGraph graph)
        {
            Path.RemoveRange(_posIndex, Path.Count - _posIndex);

            var cameFrom = new Dictionary<AbstractTile, AbstractTile>();
            var gScore = new Dictionary<AbstractTile, double>();
            var closed = new HashSet<AbstractTile>();
            var open = new SimplePriorityQueue<AbstractTile, double>();

            gScore[Position] = 0;
            open.Enqueue(Position, Helpers.Metrics.Octile(Position, Target)); // TODO add agitation noise (see Silver)

            AbstractTile current;
            while (open.Count > 0)
            {
                current = open.Dequeue();
                closed.Add(current);

                if (current == Target)
                {
                    Path.AddRange(ReconstructPath(cameFrom, current));
                    return;
                }

                foreach (var neighbor in graph.AdjacentVertices(current))
                {
                    if (closed.Contains(neighbor))
                    {
                        continue;
                    }

                    var tentative_gScore = gScore.GetValueOrDefault(current, double.PositiveInfinity) + Helpers.Metrics.Octile(current, neighbor); // TODO add agitation noise (see Silver)
                    if (tentative_gScore < gScore.GetValueOrDefault(neighbor, double.PositiveInfinity))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;

                        var fScore = tentative_gScore + Helpers.Metrics.Octile(neighbor, Target);
                        if (!open.TryUpdatePriority(neighbor, fScore))
                        {
                            open.Enqueue(neighbor, fScore);
                        }
                    }
                }
            }
        }

        public AbstractTile Step()
        {
            if (_posIndex < Path.Count)
            {
                return Path[++_posIndex];
            }

            return null;
        }

        private static List<AbstractTile> ReconstructPath(Dictionary<AbstractTile, AbstractTile> cameFrom, AbstractTile current)
        {
            var path = new List<AbstractTile>() { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
