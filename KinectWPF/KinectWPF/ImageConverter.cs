using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectWPF
{
    public static class ImageConverter
    {
        private static readonly PixelFormat FORMAT = PixelFormats.Bgr32;

        private static readonly int BYTES_PER_PIXEL = (FORMAT.BitsPerPixel + 7) / 8;

        private static readonly double DPI = 96.0;

        public static object Bitmap { get; private set; }

        public static WriteableBitmap ColorToDepth(
            CoordinateMapper coordinateMapper,
            ColorFrame colorFrame,
            DepthFrame depthFrame,
            BodyIndexFrame bodyIndexFrame)
        {
            int colorWidth = colorFrame.FrameDescription.Width;
            int colorHeight = colorFrame.FrameDescription.Height;

            int depthWidth = depthFrame.FrameDescription.Width;
            int depthHeight = depthFrame.FrameDescription.Height;

            int bodyIndexWidth = bodyIndexFrame.FrameDescription.Width;
            int bodyIndexHeight = bodyIndexFrame.FrameDescription.Height;

            var depthData = new ushort[depthWidth * depthHeight];
            var bodyData = new byte[depthWidth * depthHeight];
            var colorData = new byte[colorWidth * colorHeight * 4];
            var displayPixels = new byte[depthWidth * depthHeight * 4];

            var colorPoints = new ColorSpacePoint[depthWidth * depthHeight];

            var bitmap = new WriteableBitmap(
                depthWidth,
                depthHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            
            depthFrame.CopyFrameDataToArray(depthData);

            colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Bgra);

            bodyIndexFrame.CopyFrameDataToArray(bodyData);

            coordinateMapper.MapDepthFrameToColorSpace(depthData, colorPoints);

            Array.Clear(displayPixels, 0, displayPixels.Length);

            for (int y = 0; y < depthHeight; ++y)
            {
                for (int x = 0; x < depthWidth; ++x)
                {
                    int depthIndex = (y * depthWidth) + x;

                    byte player = bodyData[depthIndex];

                    if (player != 0xff)
                    {
                        var colorPoint = colorPoints[depthIndex];

                        int colorX = (int)Math.Floor(colorPoint.X + 0.5);
                        int colorY = (int)Math.Floor(colorPoint.Y + 0.5);

                        if ((colorX >= 0) && (colorX < colorWidth) && (colorY >= 0) && (colorY < colorHeight))
                        {
                            int colorIndex = ((colorY * colorWidth) + colorX) * 4;
                            int displayIndex = depthIndex * 4;

                            displayPixels[displayIndex + 0] = colorData[colorIndex];
                            displayPixels[displayIndex + 1] = colorData[colorIndex + 1];
                            displayPixels[displayIndex + 2] = colorData[colorIndex + 2];
                        }
                    }
                }
            }

            bitmap.Lock();

            Marshal.Copy(displayPixels, 0, bitmap.BackBuffer, displayPixels.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, depthWidth, depthHeight));

            bitmap.Unlock();

            return bitmap;
        }
    }
}
