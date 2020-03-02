using System.IO;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.WPF.Services
{
    public interface IMapService
    {
        Map GetNewMap(int width, int height);

        Map ImportMap(TextReader textReader);

        void SaveMap(Map map, string path);
    }
}
