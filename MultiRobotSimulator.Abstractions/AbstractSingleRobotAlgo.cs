using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractSingleRobotAlgo : IAlgo
    {
        private IGraph? graph;

        private AbstractTile? start;

        private AbstractTile? target;

        protected AbstractSingleRobotAlgo()
        {
        }

        public IGraph Graph
        {
            get
            {
                if (graph is null || !Initialized)
                {
                    throw GetInitializationException();
                }
                return graph;
            }
            private set { graph = value; }
        }

        public bool Initialized { get; private set; }
        public abstract string Name { get; }
        public List<AbstractTile> Path { get; private set; }

        public AbstractTile Start
        {
            get
            {
                if (start is null || !Initialized)
                {
                    throw GetInitializationException();
                }
                return start;
            }
            private set { start = value; }
        }

        public AbstractTile Target
        {
            get
            {
                if (target is null || !Initialized)
                {
                    throw GetInitializationException();
                }
                return target;
            }
            private set { target = value; }
        }

        public virtual void Initialize()
        {
        }

        public void InitializeInternal(IGraph graph)
        {
            Initialized = true;
            Graph = graph;

            var starts = graph.Vertices.Where(t => t.IsStart);
            if (!starts.Any())
            {
                throw GetNotFoundException(nameof(Start));
            }
            else if (starts.Count() > 1)
            {
                throw GetSingleException(nameof(Start));
            }
            else
            {
                Start = starts.Single();
            }

            var targets = graph.Vertices.Where(t => t.IsTarget);
            if (!targets.Any())
            {
                throw GetNotFoundException(nameof(Target));
            }
            else if (targets.Count() > 1)
            {
                throw GetSingleException(nameof(Target));
            }
            else
            {
                Target = targets.Single();
            }

            Path = new List<AbstractTile>();

            Initialize();
        }

        public abstract void RunSearch();

        private Exception GetInitializationException() => new InvalidOperationException("Failed to initialize");

        private Exception GetNotFoundException(string element) => new InvalidOperationException($"No {element} element was found in grid");

        private Exception GetSingleException(string element) => new InvalidOperationException($"Only one {element} element can be defined in grid for single robot algorithm");
    }
}
