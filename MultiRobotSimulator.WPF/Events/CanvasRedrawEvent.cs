﻿namespace MultiRobotSimulator.WPF.Events
{
    public class CanvasRedrawEvent
    {
        private readonly static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public CanvasRedrawEvent()
        {
            _logger.Trace("{event} called", nameof(CanvasRedrawEvent));
        }
    }
}
