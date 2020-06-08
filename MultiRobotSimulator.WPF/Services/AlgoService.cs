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

            // dotnet
            DotnetAlgos = container.GetAll<IAlgo>().ToDictionary(_ => Guid.NewGuid());
            _logger.LogInformation("Found {count} algorithms of type {type}", DotnetAlgos.Count, "dotnet");

            foreach (var dotnetAlgo in DotnetAlgos)
            {
                var type = string.Empty;
                if (dotnetAlgo.Value is AbstractSingleRobotAlgo)
                {
                    type = "SR";
                }
                else
                {
                    type = "MR";
                }

                var name = $"{dotnetAlgo.Value.Name} ({type}) dotnet";

                DisplayNames.Add(dotnetAlgo.Key, name);
            }
        }

        public Dictionary<Guid, string> DisplayNames { get; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, IAlgo> DotnetAlgos { get; }

        internal void RunSearch(Guid guid, Map graph)
        {
            _logger.LogInformation("Begin search with algorithm {name} (graph V={nodes}, E={edges})", DisplayNames[guid], graph.Vertices.Count(), graph.Edges.Count());

            if (DotnetAlgos.TryGetValue(guid, out var dotnetAlgo))
            {
                var sw = Stopwatch.StartNew();

                var robots = new List<Robot>();
                for (var i = 0; i < graph.Starts.Count; i++)
                {
                    robots.Add(new Robot(graph.Starts[i], graph.Targets[i]));
                }

                dotnetAlgo.InitializeInternal(graph, robots);
                _logger.LogInformation("{action} took {ms} ms", "dotnet init", sw.ElapsedMilliseconds);

                sw.Restart();
                dotnetAlgo.RunSearch();
                sw.Stop();
                _logger.LogInformation("{action} took {ms} ms", "dotnet search", sw.ElapsedMilliseconds);

                var result = new AlgoResult(dotnetAlgo, sw.ElapsedMilliseconds);

                _eventAggregator.Publish(new SearchDoneEvent(result));
            }
            else
            {
                throw new KeyNotFoundException($"Could not find algorithm with guid '{guid}'");
            }
        }
    }
}
