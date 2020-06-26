# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
### Changed
### Removed

## [1.0.0] (2020-06-27)
### Added
- Clear Robots button
- `RobotFactory`
- Multi Agent D* Lite implementation
- Windowed Hierarchical Cooperative A* implementation [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
- `SpaceTimeGraph` for CA*, HCA* and WHCA*
### Changed
- Faster rendering using `StreamGeometry`
- Add initial positions to reservation table in CA* and HCA*
- Fix tile cache not working for cloned graphs
- Update Stylet to 1.3.3
- Better `AlgoResult` path validation and info
- Change log verbosity for several calls

## [0.7.0] (2020-06-17)
### Added
- [Octile metric](https://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html#diagonal-distance)
- Rendering paths with distinct colors
### Changed
- CA* and HCA* will no longer solve unsolvable problems

## [0.6.0] (2020-06-15)
### Added
- Hierarchical Cooperative A* implementation [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
### Changed
- Replaced Algorithmia with [OptimizedPriorityQueue](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp)

## [0.5.0] (2020-06-14)
### Added
- Local Repair A* implementation [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
- Cooperative A* implementation [[Silver](https://www.davidsilver.uk/wp-content/uploads/2020/01/coop-path-AIIDE.pdf)]
- [Algorithmia](https://github.com/SolutionsDesign/Algorithmia) dependency (used in LRA*)
### Changed
- Moved Algos to separate namespace

## [0.4.0] (2020-05-12)
### Added
- `AbstractMultiRobotAlgo`
- Naive Dijkstra implementation for multiple robots
- `Robot`
- `SearchDoneEvent`

### Changed
- Fixed rendering called for each tab

## [0.3.0] (2020-04-16)
### Added
- Caching of tiles in `Map`
- Added `Helpers.Metrics` to API
- dotnet plugin support (see [plugins readme](MultiRobotSimulator.WPF/plugins/README.md))
- Rendering of found path (WIP)

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


[Unreleased]: https://github.com/seky16/MultiRobotSimulator/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.7.0...v1.0.0
[0.7.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/seky16/MultiRobotSimulator/compare/v0.1.0