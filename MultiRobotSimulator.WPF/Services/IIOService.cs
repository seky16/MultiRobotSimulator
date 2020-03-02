using System.IO;

namespace MultiRobotSimulator.WPF.Services
{
    /// <summary>
    /// Service for IO operations.
    /// </summary>
    /// <remarks>https://stackoverflow.com/a/1622980</remarks>
    public interface IIOService
    {
        string OpenFileDialog(string? defaultPath = null);

        TextReader OpenTextFile(string path);

        string SaveFileDialog(string? defaultPath = null);
    }
}
