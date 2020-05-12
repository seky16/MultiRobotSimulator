# MultiRobotSimulator

Code for diploma thesis *Path planning for multiple robots*.

## Features

- Path-planning
  - [x] Single robot
  - [ ] Multiple robots to single target
  - [x] Multiple robots to multiple targets
- [x] Support for saving/loading `.map` files ([MovingAI](https://www.movingai.com/benchmarks/formats.html))
- [x] Logging
- [ ] Tests
- [ ] Documentation
- [ ] Benchmarking
- Support for algorithms as external plugins
  - [x] [dotnet](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)
  - [ ] [Python](https://stackoverflow.com/a/53612533)
  - [ ] [Lua](https://www.moonsharp.org/)
  - [ ] [JavaScript](https://github.com/Microsoft/ClearScript)
- Path-planning algorithms
  - [x] [Dijkstra](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)
  - [ ] [A*](https://en.wikipedia.org/wiki/A*_search_algorithm) + COOP A* or other multi-agent variant
  - [ ] [PSO](https://en.wikipedia.org/wiki/Particle_swarm_optimization) variants