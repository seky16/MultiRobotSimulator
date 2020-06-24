#nullable disable

using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Algos;

namespace MultiRobotSimulator.Core.Models
{
    public class HCAStarRobot : CoopAStarRobot
    {
        private RRAStar _rra;

        public HCAStarRobot(AbstractTile start, AbstractTile target) : base(start, target)
        {
        }

        public override void Initialize(SpaceTimeGraph spaceTimeGraph)
        {
            base.Initialize(spaceTimeGraph);
            _rra = new RRAStar(spaceTimeGraph.Graph, Start, Target);
        }

        protected override double Heuristic(AbstractTile p)
        {
            return _rra.AbstractDist(p);
        }
    }
}
