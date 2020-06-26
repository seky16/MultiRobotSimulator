using MultiRobotSimulator.Core.Models;

namespace MultiRobotSimulator.WPF.Events
{
    public class SearchDoneEvent
    {
        private readonly static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public SearchDoneEvent(AlgoResult result)
        {
            _logger.Trace("{event} called", nameof(SearchDoneEvent));
            Result = result;
        }

        public AlgoResult Result { get; }
    }
}
