using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Abstractions;
using MultiRobotSimulator.Core.Models;
using MultiRobotSimulator.WPF.Events;
using Stylet;
using StyletIoC;

namespace MultiRobotSimulator.WPF.Services
{
    public class AlgoService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<AlgoService> _logger;

        public AlgoService(IContainer container, ILogger<AlgoService> logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;

            Algos = container.GetAll<IAlgo>().ToDictionary(_ => Guid.NewGuid());
            _logger.LogInformation("Found {count} algorithms of type {type}", Algos.Count, "dotnet");

            foreach (var algo in Algos)
            {
                DisplayNames.Add(algo.Key, algo.Value.Name);
            }
        }

        public Dictionary<Guid, IAlgo> Algos { get; }
        public Dictionary<Guid, string> DisplayNames { get; } = new Dictionary<Guid, string>();

        internal void RunSearch(Guid guid, Map graph)
        {
            _logger.LogInformation("Begin search with algorithm {name} (graph V={nodes}, E={edges})", DisplayNames[guid], graph.Vertices.Count(), graph.Edges.Count());

            if (Algos.TryGetValue(guid, out var algo))
            {
                var sw = Stopwatch.StartNew();

                var robots = new List<Robot>();
                for (var i = 0; i < graph.Starts.Count; i++)
                {
                    robots.Add(new Robot(graph.Starts[i], graph.Targets[i]));
                }

                algo.InitializeInternal(graph.Clone(), robots);
                var initTime = sw.ElapsedMilliseconds;
                _logger.LogInformation("{action} took {ms} ms", "init", initTime);

                sw.Restart();
                algo.RunSearch();
                sw.Stop();
                _logger.LogInformation("{action} took {ms} ms", "search", sw.ElapsedMilliseconds);

                var result = new AlgoResult(algo, initTime, sw.ElapsedMilliseconds);

                _eventAggregator.Publish(new SearchDoneEvent(result));
            }
            else
            {
                throw new KeyNotFoundException($"Could not find algorithm with guid '{guid}'");
            }
        }
    }
}
