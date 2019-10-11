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
using WP.Device.Framework.Base;
using WP.Device.Framework.Screen;

namespace WP.Device.Framework
{
    /// <summary>
    /// 截屏插件
    /// </summary>
    public class ScreenPlugins
    {
        private static MemoryStream _globalMemoryStream = new System.IO.MemoryStream();

        /// <summary>
        /// 根据快照获得位图资源
        /// </summary>
        /// <returns></returns>
        public static BitmapSource GetBitMapSourceFromSnapScreen()
        {
            Bitmap bitmap = GetScreenSnapshot();
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            GDIWin32Api.DeleteObject(intPtrl);
            return bitmapSource;
        }

        public static BitmapSource GetBitMapSourceFromBitmap(Bitmap bitmap)
        {
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            GDIWin32Api.DeleteObject(intPtrl);
            return bitmapSource;
        }

        public static BitmapImage GetBitmapImageFromSnapScreen()
        {
            using (Bitmap bitmap = GetScreenSnapshot())
            {
                bitmap.Save(_globalMemoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bitmapBytes = _globalMemoryStream.GetBuffer();
                bitmap.Dispose();

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bitmapBytes);
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
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

        /// <summary>
        /// 获得屏幕快照
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetScreenSnapshot()
        {
            System.Drawing.Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;

            var bitmap = new Bitmap(rect.Width, rect.Height);
            using (Graphics grap = Graphics.FromImage(bitmap))
            {
                grap.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        /// <summary>
        /// 根据坐标来截图
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Bitmap GetBitmapFromPoint(ScreenConfig config)
        {
            var width = config.RightTopCoordinate.Width - config.LeftTopCoordinate.Width;
            var height = config.RightBottomCoordinate.Height - config.RightTopCoordinate.Height;
            if (width <= 0 || height <= 0)
                return null;

            Bitmap image = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                //设置截屏区域
                graphics.CopyFromScreen(config.LeftTopCoordinate.Width, config.LeftTopCoordinate.Height, 0, 0, new System.Drawing.Size(width, height));
                return image;
            }
        }
    }
}
