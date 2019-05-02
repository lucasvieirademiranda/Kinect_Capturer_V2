using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWPF
{
    public class FrameRateCounter
    {
        private double _fps = 0;

        private DateTime? _lastFrame = null;

        private DateTime? _currentFrame = null;

        public void Reset()
        {
            _fps = 0;
            _lastFrame = null;
            _currentFrame = null;
        }

        public double GetFPS()
        {
            if (!_lastFrame.HasValue)
                _lastFrame = DateTime.Now;
            
            _currentFrame = DateTime.Now;
            
            var timeSpan = _currentFrame.Value.Subtract(_lastFrame.Value);

            _lastFrame = _currentFrame;

            _fps = 1000 / timeSpan.TotalMilliseconds;

            return _fps;
        }
    }
}
