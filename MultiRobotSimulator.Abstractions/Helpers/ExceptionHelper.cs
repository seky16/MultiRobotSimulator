using System;

namespace MultiRobotSimulator.Abstractions.Helpers
{
    public static class ExceptionHelper
    {
        public static Exception GetInitializationException(string name) => new InvalidOperationException($"Failed to initialize '{name}'");

        public static Exception GetNonSingleException(string element) => new InvalidOperationException($"Only one {element} element can be defined in grid for single robot algorithm");

        public static Exception GetNotFoundException(string element) => new InvalidOperationException($"No {element} element was found in grid");
    }
}
