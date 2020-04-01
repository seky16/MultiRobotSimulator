using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MultiRobotSimulator.Abstractions;
using StyletIoC;

namespace MultiRobotSimulator.WPF.Services
{
    public class AlgoService
    {
        private readonly ILogger<AlgoService> _logger;

        public AlgoService(IContainer container, ILogger<AlgoService> logger)
        {
            _logger = logger;

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

        internal void RunSearch(Guid guid, IGraph graph)
        {
            _logger.LogInformation("Begin search with algorithm {name} (graph V={nodes}, E={edges})", DisplayNames[guid], graph.Vertices.Count(), graph.Edges.Count());

            if (DotnetAlgos.TryGetValue(guid, out var dotnetAlgo))
            {
                var sw = Stopwatch.StartNew();
                dotnetAlgo.Initialize(graph);
                _logger.LogInformation("dotnet init took {ms} ms", sw.ElapsedMilliseconds);

                sw.Restart();
                dotnetAlgo.RunSearch();
                _logger.LogInformation("dotnet search took {ms} ms", sw.ElapsedMilliseconds);
            }
            else
            {
                throw new KeyNotFoundException($"Could not find algorithm with guid '{guid}'");
            }
        }
    }
}
