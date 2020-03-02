using System;
using System.IO;
using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.WPF.Services
{
    public class MapService : IMapService
    {
        private readonly Func<Map> _mapFactory;

        public MapService(Func<Map> importedMapFactory)
        {
            _mapFactory = importedMapFactory;
        }

        public Map GetNewMap(int width, int height)
        {
            var map = _mapFactory();
            map.Width = width;
            map.Height = height;
            map.EmptyTiles();
            return map;
        }

        public Map ImportMap(TextReader textReader)
        {
            var map = _mapFactory();

            if (!map.ReadHeader(textReader))
                throw new FileFormatException("Invalid header");

            if (!map.ReadMap(textReader))
                throw new FileFormatException("Invalid map");

            return map;
        }

        public void SaveMap(Map map, string path)
        {
            File.WriteAllText(path, map.ToString());
        }
    }
}
