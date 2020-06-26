#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Algos
{
    public class CooperativeAStar : AbstractMultiRobotAlgo
    {
        private HashSet<SpaceTimeNode> _reservationTable;
        public override string Name => "3 Cooperative A*";

        public override void Initialize()
        {
            _reservationTable = new HashSet<SpaceTimeNode>();
            var spaceTimeGraph = new SpaceTimeGraph(Graph);
            foreach (CoopAStarRobot robot in Robots)
            {
                robot.Initialize(spaceTimeGraph);
                _reservationTable.Add(new SpaceTimeNode(robot.Start.X, robot.Start.Y, 0));
            }
        }

        public override Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new CoopAStarRobot(start, target);
        }

        public override void RunSearch()
        {
            foreach (CoopAStarRobot robot in Robots)
            {
                robot.Search(ref _reservationTable);
            }
        }
    }
}
