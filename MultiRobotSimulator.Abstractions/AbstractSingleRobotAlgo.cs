using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions.Helpers;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractSingleRobotAlgo : IAlgo
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

        public Robot Robot => Robots.First();

        public IReadOnlyCollection<Robot> Robots { get; private set; } = new List<Robot>();

        public abstract void Initialize();

        public void InitializeInternal(IGraph graph, IReadOnlyCollection<Robot> robots)
        {
            Initialized = true;
            Graph = graph;

            if (robots.Count != 1 || !(robots.ElementAt(0) is Robot robot))
            {
                throw ExceptionHelper.GetInitializationException(nameof(Robot));
            }

            var starts = graph.Vertices.Where(t => t.IsStart);
            if (!starts.Any())
            {
                throw ExceptionHelper.GetNotFoundException(nameof(robot.Start));
            }
            else if (starts.Count() > 1)
            {
                throw ExceptionHelper.GetNonSingleException(nameof(robot.Start));
            }
            else if (starts.Single() != robot.Start)
            {
                throw ExceptionHelper.GetInitializationException(nameof(robot.Start));
            }

            var targets = graph.Vertices.Where(t => t.IsTarget);
            if (!targets.Any())
            {
                throw ExceptionHelper.GetNotFoundException(nameof(robot.Target));
            }
            else if (targets.Count() > 1)
            {
                throw ExceptionHelper.GetNonSingleException(nameof(robot.Target));
            }
            else if (targets.Single() != robot.Target)
            {
                throw ExceptionHelper.GetInitializationException(nameof(robot.Target));
            }

            Robots = robots;

            Initialize();
        }

        public abstract void RunSearch();
    }
}
