#nullable disable

using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Algos
{
    public class WHCAStar : AbstractMultiRobotAlgo
    {
        private HashSet<SpaceTimeNode> _reservationTable;
        public override string Name => "WHCA*";

        public override void Initialize()
        {
            _reservationTable = new HashSet<SpaceTimeNode>();
            var spaceTimeGraph = new SpaceTimeGraph(Graph);
            foreach (WHCAStarRobot robot in Robots)
            {
                robot.Initialize(spaceTimeGraph, 16);
                _reservationTable.Add(new SpaceTimeNode(robot.Start.X, robot.Start.Y, 0));
            }
        }

        public override Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new WHCAStarRobot(start, target);
        }

        public override void RunSearch()
        {
            var whcaStarRobots = Robots.Cast<WHCAStarRobot>();

            var enRoute = whcaStarRobots.Where(r => r.Position != null && r.Position != r.Target).ToList();
            while (enRoute.Count > 0)
            {
                foreach (var robot in whcaStarRobots)
                {
                    robot.Step(ref _reservationTable);
                }

                enRoute = whcaStarRobots.Where(r => r.Position != null && r.Position != r.Target).ToList();
            }
        }
    }
}
