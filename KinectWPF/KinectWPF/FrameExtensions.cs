using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectWPF
{
    public static class FrameExtensions
    {
        public static WriteableBitmap ToColorBitmap(this ColorFrame frame)
        {
            var width = frame.FrameDescription.Width;
            var height = frame.FrameDescription.Height;

            var rectangle = new Int32Rect(0, 0, width, height);
            var stride = width * 4;

            var bitmap = new WriteableBitmap(width,
                                             height,
                                             96,
                                             96,
                                             PixelFormats.Bgr32,
                                             null);

            var pixels = new byte[4 * frame.FrameDescription.LengthInPixels];

            frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);

            bitmap.WritePixels(rectangle, pixels, stride, 0);

            return bitmap;
        }

        public static WriteableBitmap ToDepthBitmap(this DepthFrame frame)
        {
            var width = frame.FrameDescription.Width;
            var height = frame.FrameDescription.Height;

            var rectangle = new Int32Rect(0, 0, width, height);
            var stride = width * 2;

            var bitmap = new WriteableBitmap(width,
                                             height,
                                             96,
                                             96,
                                             PixelFormats.Gray16,
                                             null);

            var pixels = new ushort[frame.FrameDescription.LengthInPixels];

            frame.CopyFrameDataToArray(pixels);

            bitmap.WritePixels(rectangle, pixels, stride, 0);

            return bitmap;
        }
    }
}
