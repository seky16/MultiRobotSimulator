#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Algos
{
    public class HCAStar : AbstractMultiRobotAlgo
    {
        private HashSet<SpaceTimeNode> _reservationTable;
        public override string Name => "Hierarchical Cooperative A*";

        public override void Initialize()
        {
            _reservationTable = new HashSet<SpaceTimeNode>();
            var spaceTimeGraph = new SpaceTimeGraph(Graph);
            foreach (HCAStarRobot robot in Robots)
            {
                robot.Initialize(spaceTimeGraph);
                _reservationTable.Add(new SpaceTimeNode(robot.Start.X, robot.Start.Y, 0));
            }
        }

        public override Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new HCAStarRobot(start, target);
        }

        public override void RunSearch()
        {
            foreach (HCAStarRobot robot in Robots)
            {
                robot.Search(ref _reservationTable);
            }
        }
    }
}
