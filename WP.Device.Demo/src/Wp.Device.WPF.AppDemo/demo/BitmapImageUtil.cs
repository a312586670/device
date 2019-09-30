using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Wp.Device.WPF.AppDemo.demo
{
    public class BitmapImageUtil
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        private static MemoryStream globalMemoryStream = new System.IO.MemoryStream();

        public static BitmapSource getBitMapSourceFromSnapScreen()
        {
            Bitmap bitmap = GetScreenSnapshot();
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(intPtrl);
            return bitmapSource;
        }

        public static BitmapSource getBitMapSourceFromBitmap(Bitmap bitmap)
        {
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(intPtrl);
            return bitmapSource;
        }

        public static BitmapImage getBitmapImageFromSnapScreen()
        {
            Bitmap bitmap = GetScreenSnapshot();
            bitmap.Save(globalMemoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bitmapBytes = globalMemoryStream.GetBuffer();  //byte[]   bytes=   ms.ToArray();  
            //ms.Close(); 
            bitmap.Dispose();
            // Init bitmap
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(bitmapBytes);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }


        public static Bitmap GetScreenSnapshot()
        {
            System.Drawing.Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }
    }
}
