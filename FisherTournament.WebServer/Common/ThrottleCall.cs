using System.Diagnostics;

namespace FisherTournament.WebServer.Common
{
    public class ThrottleCall
    {
        private readonly Stopwatch _lastCallWatch;
        private readonly int _minTime_s;
        private bool _isFirstCall = true;

        public Action? CallBack { get; set; }

        public ThrottleCall(int minTime_s)
        {
            _lastCallWatch = new Stopwatch();
            _minTime_s = minTime_s;
        }

        public void Call()
        {
            var elapsed = _lastCallWatch.Elapsed.TotalSeconds;
            if (_isFirstCall || elapsed >= _minTime_s)
            {
                _lastCallWatch.Restart();
                CallBack?.Invoke();
                _isFirstCall = false;
            }
        }
    }
}
