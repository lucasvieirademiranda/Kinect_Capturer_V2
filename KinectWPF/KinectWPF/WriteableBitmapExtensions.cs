using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KinectWPF
{
    public enum ImageFormats
    {
        Bmp,
        Gif,
        Jpg,
        Png,
        Tif,
        Wmp
    }

    public static class WriteableBitmapExtensions
    {
        public static void Save(this WriteableBitmap bitmap, string path, ImageFormats format)
        {
            BitmapEncoder encoder = null;
            
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write,  4096, true))
            {
                switch (format)
                {
                    case ImageFormats.Bmp:
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ImageFormats.Gif:
                        encoder = new GifBitmapEncoder();
                        break;
                    case ImageFormats.Jpg:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ImageFormats.Png:
                        encoder = new PngBitmapEncoder();
                        break;
                    case ImageFormats.Tif:
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ImageFormats.Wmp:
                        encoder = new WmpBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }
        }
    }
}
