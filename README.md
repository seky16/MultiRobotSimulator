# MultiRobotSimulator

Code for diploma thesis *Path planning for multiple robots*.

## Features

- Path-planning for multiple robots
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
  - [x] [Dijkstra](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm) - just a naive implementation (for testing purposes) - commented out
  - [A*](https://en.wikipedia.org/wiki/A*_search_algorithm)
    - [x] Local Repair A* [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
    - [x] Cooperative A* [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
    - [x] Hierarchical Cooperative A* [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]