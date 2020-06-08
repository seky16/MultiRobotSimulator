using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions.Helpers;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractSingleRobotAlgo : AbstractSingleRobotAlgo<Robot>
    { }

    public abstract class AbstractSingleRobotAlgo<TRobot> : IAlgo<TRobot> where TRobot : Robot
    {
        private IGraph? _graph;

        protected AbstractSingleRobotAlgo()
        {
        }

        public IGraph Graph
        {
            get
            {
                if (_graph is null || !Initialized)
                {
                    throw ExceptionHelper.GetInitializationException(nameof(Graph));
                }
                return _graph;
            }
            private set { _graph = value; }
        }

        public bool Initialized { get; private set; }
        public abstract string Name { get; }

        public TRobot Robot => Robots.First();

        public IReadOnlyCollection<TRobot> Robots { get; private set; } = new List<TRobot>();

        public abstract void Initialize();

        public void InitializeInternal(IGraph graph, IEnumerable<AbstractTile> starts, IEnumerable<AbstractTile> targets)
        {
            Graph = graph;

            if (!starts.Any())
            {
                throw ExceptionHelper.GetNotFoundException(nameof(Robot.Start));
            }
            else if (starts.Count() > 1)
            {
                throw ExceptionHelper.GetNonSingleException(nameof(Robot.Start));
            }

            if (!targets.Any())
            {
                throw ExceptionHelper.GetNotFoundException(nameof(Robot.Target));
            }
            else if (targets.Count() > 1)
            {
                throw ExceptionHelper.GetNonSingleException(nameof(Robot.Target));
            }

            Robots = new List<TRobot>() { (TRobot)Activator.CreateInstance(typeof(TRobot), starts.Single(), targets.Single()) };

            Initialize();
            Initialized = true;
        }

        public abstract void RunSearch();
    }
}
