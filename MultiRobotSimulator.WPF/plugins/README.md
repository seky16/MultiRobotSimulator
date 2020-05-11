# Readme: Plugins

Supported plugins:

- [x] [dotnet](#dotnet-plugins)


## dotnet plugins

1. Create new project/solution (Class library, targeting `netcore 3.0`)
2. Reference `MultiRobotSimulator.Abstractions.dll` using
```xml
<Reference Include="MultiRobotSimulator.Abstractions">
    <HintPath>path\to\MultiRobotSimulator.Abstractions.dll</HintPath>
    <ExcludeAssets>runtime</ExcludeAssets>
    <Private>false</Private>
</Reference>
```
in your `.csproj` file
3. Create new class inheriting from `AbstractSingleRobotAlgo`, `AbstractMultiRobotAlgo` or `IAlgo` (advanced)
4. Implement required overrides:

TODO

5. (Optional) Implement optional overrides:

TODO

6. Build your project
7. Copy built `.dll` to `<appFolder>\plugins`
8. Run the app. Your algorithm should appear in dropdown

> **Note:** Constructor is only run once (during application startup). Use `Initialize()` for set-up code you need to run before each search.