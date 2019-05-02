using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWPF
{
    public class FrameCounter
    {
        private long _frameCounter = 0;

        public void Reset()
        {
            _frameCounter = 0;
        }

        public long GetTotalFrames()
        {
            _frameCounter += 1;
            return _frameCounter;
        }
    }
}
