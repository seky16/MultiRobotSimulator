# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## TODO
- Algorithms as plugins
    - [Python?](https://stackoverflow.com/a/53612533)
    - [Lua?](https://www.moonsharp.org/)
    - [JavaScript?](https://github.com/Microsoft/ClearScript)
- Tests
- Documentation
- Benchmarks
- Collision checks, result validation 
- change QuickGraph to Algorithmia for `Graph`

## [Unreleased]
### Added
- Local Repair A* implementation [Silver]
### Changed
- Moved Algos to separate namespace
### Removed

## [0.4.0] (2020-05-12)
### Added
- Added `AbstractMultiRobotAlgo`
- Added naive Dijkstra implementation for multiple robots
- Added `Robot`
- Added `SearchDoneEvent`

### Changed
- Fixed rendering called for each tab

## [0.3.0] (2020-04-16)
### Added
- Caching of tiles in `Map`
- Added `Helpers.Metrics` to API
- dotnet plugin support (see [plugins readme](MultiRobotSimulator.WPF/plugins/README.md))
- Added rendering of found path (WIP)

### Changed
- Merged `Map` and `Graph`
- Fixed corner cutting
- Fixed dotnet algo initialization

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
- Moved `MapService` to Core project and renamed to `MapFactory`
- Renamed `Tile#IsFinish` to `Tile#IsTarget`

## [0.1.0] (2020-03-03)
### Added
- Saving/loading `.map` files ([MovingAI](https://www.movingai.com/benchmarks/formats.html))
- Multiple `.map` tabs
- Drawing obstacles/starts/finishes


[Unreleased]: https://github.com/seky16/MultiRobotSimulator/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0