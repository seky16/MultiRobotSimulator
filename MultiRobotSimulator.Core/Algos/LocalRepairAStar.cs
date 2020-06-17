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
            System.Threading.Tasks.Parallel.ForEach(Robots, robot =>
            {
                ((LRAStarRobot)robot).Search(Graph);
            });

            var enRoute = Robots.Cast<LRAStarRobot>().Where(r => r.Position != null && r.Position != r.Target);
            while (enRoute.Any())
            {
                foreach (var robot in enRoute)
                {
                    var next = robot.Step(); // get the next position
                    if (next != null && enRoute.Any(r => r != robot && r.Position == next))
                    {
                        // occupied - recalculate remainder of the route
                        var graph = Graph.Clone();
                        graph.RemoveVertex(next);
                        robot.Search(graph);
                    }
                    else
                    {
                        robot.Position = next; // move to the next position
                    }
                }

                enRoute = Robots.Cast<LRAStarRobot>().Where(r => r.Position != null && r.Position != r.Target);
            }
        }
    }
}
