using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WP.Device.Framework.Base
{
    /// <summary>
    /// 系统GDI API
    /// </summary>
    public class GDIWin32Api
    {
        #region API
        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);

        #endregion

        #region 公共方法

        //private static System.Drawing.Pen _pen;
        //private static Graphics graphics;
        private static IntPtr LastPtr;

        /// <summary>
        /// 根据窗口句柄矩形
        /// </summary>
        /// <param name="tarent">目标窗口句柄</param>
        /// <param name="width">矩形宽度</param>
        /// <param name="height">矩形高度</param>
        /// <param name="lineWidth">矩形线条宽度(粗细度)</param>
        /// <param name="color">颜色</param>
        public static void DrawRectangle(IntPtr tarent, int width, int height, int lineWidth, Color color)
        {
            if (tarent == null || width <= 0 || height <= 0 || lineWidth <= 0)
                return;
            Task.Factory.StartNew(() =>
            {
                //获得句柄的DC
                var hdc = BaseWin32Api.GetWindowDC(tarent);
                //if (LastPtr != null)
                //    BaseWin32Api.RedrawWindow(hdc, null, IntPtr.Zero, 0x85);

                //TODO 重新绘图会导致卡顿问题[性能受到很大的影响]
                BaseWin32Api.RedrawWindow(IntPtr.Zero, null, IntPtr.Zero, 0x85);

                Thread.Sleep(400);
                //if (graphics == null)
                //    graphics = Graphics.FromHdc(hdc);
                //else
                //    graphics = Graphics.FromHdc(hdc);

                ////实例化Pen类
                //if (_pen == null)
                //    _pen = new System.Drawing.Pen(color, lineWidth);

                ////调用Graphics对象的DrawRectangle方法
                //graphics.DrawRectangle(_pen, 0, 0, width - lineWidth, height - lineWidth);

                using (Graphics gr = Graphics.FromHdc(hdc))
                {
                    //实例化Pen类
                    var pen = new System.Drawing.Pen(color, lineWidth);

                    //调用Graphics对象的DrawRectangle方法
                    gr.DrawRectangle(pen, 0, 0, width - lineWidth, height - lineWidth);
                }
                LastPtr = tarent;
            });


        }

        public static void DrawRectangle(int x, int y, int width, int height, int lineWidth, Color color)
        {
            IntPtr desk = BaseWin32Api.GetDesktopWindow();
            IntPtr deskDC = BaseWin32Api.GetDCEx(desk, IntPtr.Zero, 0x403);

            BaseWin32Api.RedrawWindow(IntPtr.Zero, null, IntPtr.Zero, 0x85);
            using (Graphics g = Graphics.FromHdc(deskDC))
            {

                //实例化Pen类
                System.Drawing.Pen myPen = new System.Drawing.Pen(color, lineWidth);
                g.DrawRectangle(myPen, 0, 0, width - lineWidth, height - lineWidth);
            }
            //g.DrawString("Lightning", new Font("宋体", 50, FontStyle.Bold), Brushes.Red, new PointF(100, 100));
        }
        #endregion
    }
}
