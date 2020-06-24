#nullable disable

using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Algos
{
    public class LocalRepairAStar : AbstractMultiRobotAlgo
    {
        public override string Name => "Local Repair A*";

        public override void Initialize()
        {
        }

        public override Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new LRAStarRobot(start, target);
        }

        public override void RunSearch()
        {
            var lraStarRobots = Robots.Cast<LRAStarRobot>();
            foreach (var robot in lraStarRobots)
            {
                var graph = Graph.Clone();
                foreach (var neighbor in graph.AdjacentVertices(robot.Start))
                {
                    if (lraStarRobots.Any(r => r.Start == neighbor))
                    {
                        graph.RemoveAdjacentEdges(neighbor);
                    }
                }
                robot.Search(graph);
            }

            var enRoute = lraStarRobots.Where(r => r.Position != null && r.Position != r.Target);
            while (enRoute.Any())
            {
                foreach (var robot in enRoute)
                {
                    var next = robot.Step(); // get the next position
                    if (next != null && lraStarRobots.Any(r => r.Position == next))
                    {
                        // occupied - recalculate remainder of the route
                        var graph = Graph.Clone();
                        graph.RemoveAdjacentEdges(next);
                        robot.Search(graph);
                    }
                    else
                    {
                        robot.Position = next; // move to the next position
                    }
                }

                enRoute = lraStarRobots.Where(r => r.Position != null && r.Position != r.Target);
            }
        }
    }
}
