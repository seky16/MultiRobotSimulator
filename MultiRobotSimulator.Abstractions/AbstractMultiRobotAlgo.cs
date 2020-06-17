using System;
using System.Collections.Generic;
using System.Linq;
using MultiRobotSimulator.Abstractions.Helpers;

namespace MultiRobotSimulator.Abstractions
{
    public abstract class AbstractMultiRobotAlgo : IAlgo
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

        public IReadOnlyCollection<Robot> Robots { get; private set; } = new List<Robot>();

        public abstract void Initialize();

        public void InitializeInternal(IGraph graph, IReadOnlyCollection<Robot> robots)
        {
            Initialized = true;
            Graph = graph;

            if (robots.Count == 0)
            {
                throw ExceptionHelper.GetInitializationException(nameof(Robots));
            }

            Robots = robots;

            var graphStarts = graph.Vertices.Where(t => t.IsStart);
            var graphTargets = graph.Vertices.Where(t => t.IsTarget);
            var startsCount = graphStarts.Count();
            var targetsCount = graphTargets.Count();
            if (startsCount != targetsCount && startsCount != Robots.Count)
            {
                throw new InvalidOperationException($"Count mismatch! Robots={Robots.Count} Starts={startsCount}, Targets={targetsCount}");
            }

            var robotStarts = Robots.Select(r => r.Start);
            var robotTargets = Robots.Select(r => r.Target);
            for (var i = 0; i < Robots.Count; i++)
            {
                if (!robotStarts.Contains(graphStarts.ElementAt(i)))
                {
                    throw ExceptionHelper.GetInitializationException("Start");
                }
                if (!robotTargets.Contains(graphTargets.ElementAt(i)))
                {
                    throw ExceptionHelper.GetInitializationException("Target");
                }
            }

            Initialize();
        }

        public virtual Robot RobotFactory(AbstractTile start, AbstractTile target)
        {
            return new Robot(start, target);
        }

        public abstract void RunSearch();
    }
}
