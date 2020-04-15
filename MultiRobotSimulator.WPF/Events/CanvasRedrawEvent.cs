using System.Collections.Generic;
using MultiRobotSimulator.Abstractions;

namespace MultiRobotSimulator.WPF.Events
{
    public class CanvasRedrawEvent
    {
        private readonly static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public CanvasRedrawEvent(IEnumerable<AbstractTile>? path = null)
        {
            _logger.Debug("Canvas redraw event called");
            Path = path;
        }

        public IEnumerable<AbstractTile>? Path { get; }
    }
}
