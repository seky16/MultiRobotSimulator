using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions.Helpers;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractMultiRobotAlgo : AbstractMultiRobotAlgo<Robot>
    {
    }

    public abstract class AbstractMultiRobotAlgo<TRobot> : IAlgo<TRobot> where TRobot : Robot
    {
        private IGraph? _graph;

        public IGraph Graph
        {
            get
            {
                if (_graph is null || !Initialized)
                {
                    throw ExceptionHelper.GetInitializationException($"{nameof(Graph)} ({Name})");
                }
                return _graph;
            }
            private set { _graph = value; }
        }

        public bool Initialized { get; private set; }
        public abstract string Name { get; }

        public IReadOnlyCollection<TRobot> Robots { get; private set; } = new List<TRobot>();

        public abstract void Initialize();

        public void InitializeInternal(IGraph graph, IEnumerable<AbstractTile> starts, IEnumerable<AbstractTile> targets)
        {
            if (starts.Count() != targets.Count())
            {
                throw new InvalidOperationException($"Count mismatch! Starts={starts.Count()}, Targets={targets.Count()}");
            }

            Graph = graph;

            var robots = new List<TRobot>();
            for (var i = 0; i < starts.Count(); i++)
            {
                robots.Add((TRobot)Activator.CreateInstance(typeof(TRobot), starts.ElementAt(i), targets.ElementAt(i)));
            }

            if (robots.Count == 0)
            {
                throw ExceptionHelper.GetInitializationException(nameof(Robots));
            }

            Robots = robots;

            Initialize();
            Initialized = true;
        }

        public abstract void RunSearch();
    }
}
