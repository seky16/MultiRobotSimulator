using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public interface IAlgo : IAlgo<Robot>
    {
    }

    public interface IAlgo<TRobot> where TRobot:Robot
    {
        IGraph Graph { get; }
        string Name { get; }

        IReadOnlyCollection<TRobot> Robots { get; }

        void InitializeInternal(IGraph graph, IEnumerable<AbstractTile> starts, IEnumerable<AbstractTile> targets);

        void RunSearch();
    }
}
