# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## TODO
- Algorithms as plugins
    - [dotnet](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)
    - [Python?](https://stackoverflow.com/a/53612533)
    - [Lua?](https://www.moonsharp.org/)
    - [JavaScript?](https://github.com/Microsoft/ClearScript)
- Tests
- Documentation
- Benchmarks

## [Unreleased]
### Added

### Changed

### Removed

## [0.2.0] (2020-04-02)
### Added
- Logging - [NLog](https://nlog-project.org/)
- Abstractions project (API)
- Single-robot implementation of Dijkstra's algorithm (WIP)
- Wrapper around graph
- `AlgoService` responsible for handling algorithms

### Changed
- Switched grid representation to graph using [QuickGraph](https://yaccconstructor.github.io/QuickGraph/)
- Moved `Enums` to Core project
- moved `MapService` to Core project and renamed to `MapFactory`
- Renamed `Tile#IsFinish` to `Tile#IsTarget`

## [0.1.0] (2020-03-03)
### Added
- Saving/loading `.map` files ([MovingAI](https://www.movingai.com/benchmarks/formats.html))
- Multiple `.map` tabs
- Drawing obstacles/starts/finishes


[Unreleased]: https://github.com/seky16/MultiRobotSimulator/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0...0.2.0
[0.1.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0