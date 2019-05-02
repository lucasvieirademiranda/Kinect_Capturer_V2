using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWPF
{
    public class TimeCounter
    {
        private TimeSpan _timeSpan = TimeSpan.Zero;

        private DateTime? _lastTime = null;

        private DateTime? _currentTime = null;
        
        public void Reset()
        {
            _timeSpan = TimeSpan.Zero;
            _lastTime = null;
            _currentTime = null;
        }

        public TimeSpan GetElapsedTime()
        {
            if (!_lastTime.HasValue)
                _lastTime = DateTime.Now;

            _currentTime = DateTime.Now;

            var currentTimeSpan = _currentTime.Value.Subtract(_lastTime.Value);

            _lastTime = _currentTime;

            _timeSpan = _timeSpan.Add(currentTimeSpan);

            return _timeSpan;
        }
    }
}
