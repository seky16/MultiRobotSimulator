using System.IO;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.Core.Factories
{
    public interface IMapFactory
    {
        Map CreateMap(int width, int height);

        Map FromText(TextReader textReader);
    }
}
