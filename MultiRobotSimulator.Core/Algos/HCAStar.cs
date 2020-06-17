#nullable disable

using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Algos
{
    public class HCAStar : AbstractMultiRobotAlgo
    {
        private HashSet<(int, int, int)> _reservationTable;
        public override string Name => "Hierarchical Cooperative A*";

        public override void Initialize()
        {
            _reservationTable = new HashSet<(int, int, int)>();
            foreach (var robot in Robots)
            {
                _reservationTable.Add((robot.Start.X, robot.Start.Y, 0));
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
                var rra = new RRAStar(Graph, robot.Start, robot.Target);
                robot.Search(Graph, ref _reservationTable, (s, _) => rra.AbstractDist(s));
            }
        }
    }
}
