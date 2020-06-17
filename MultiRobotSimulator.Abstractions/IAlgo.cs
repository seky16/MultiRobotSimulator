using System.Collections.Generic;

namespace MultiRobotSimulator.Abstractions
{
    public interface IAlgo
    {
        IGraph Graph { get; }
        string Name { get; }

        IReadOnlyCollection<Robot> Robots { get; }

        void InitializeInternal(IGraph graph, IReadOnlyCollection<Robot> robots);

        Robot RobotFactory(AbstractTile start, AbstractTile target);

        void RunSearch();
    }
}
