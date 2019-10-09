using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Automation.Text;
using WP.Device.Framework;

namespace UtilityLibrary
{
    public static class Utility
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        private static object syncLock = new object();
        private static Random random = new Random();

        public enum MouseEvent
        {
            LeftClick,
            LeftDoubleClick,
            LeftDown,
            LeftUp,
            RightClick,
            RightDoubleClick,
            RightDown,
            RightUp
        }

        public class Win32WindowInfo
        {
            public int ProcessId;
            public IntPtr Handle;
            public string Title;
            public string ClassName;

            public Win32WindowInfo(int processId, IntPtr handle, string title, string className)
            {
                this.ProcessId = processId;
                this.Handle = handle;
                this.Title = title;
                this.ClassName = className;
            }
        }

        #region Win32 APIs

        [DllImport("user32.dll")]
        public extern static IntPtr FindWindow(string classname, string captionName);

        [DllImport("user32.dll")]
        public extern static IntPtr FindWindowEx(IntPtr parent, IntPtr child, string classname, string captionName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsProc ewp, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// 置窗口状态
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow">0 隐藏取消激活 1 还原激活 2 最小化激活 3 最大化激活 4 还原 6 最小化取消激活 7 最小化 9 还原激活</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("user32.dll")]
        public static extern void ShowWindowAsync(IntPtr hwnd, uint nCmdShow);

        [DllImport("User32.dll")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        public static extern int keybd_event(int bVk,               //欲模拟的虚拟键码
                                                int bScan,          //键的OEM扫描码
                                                int dwFlags,        //标志常数
                                                int dwExtraInfo);   //通常不用的一个值

        [DllImport("user32.dll")]
        public static extern int ScreenToClient(IntPtr hwnd, ref Rectangle lpPoint);

        [DllImport("User32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Rectangle lpPoint);

        [DllImport("user32.dll")]
        public static extern int GetClientRect(IntPtr hwnd, ref Rectangle lpPoint);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, ref Rectangle lpPoint);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hwnd
                                                , int hWndInsertAfter
                                                , int x //窗口新的x坐标。如hwnd是一个子窗口，则x用父窗口的客户区坐标表示
                                                , int y //窗口新的y坐标。如hwnd是一个子窗口，则y用父窗口的客户区坐标表示
                                                , int cx //指定新的窗口宽度
                                                , int cy //指定新的窗口高度
                                                , int wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point p);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int x, int y);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
         string lpszDriver,         // driver name驱动名
         string lpszDevice,         // device name设备名
         string lpszOutput,         // not used; should be NULL
         IntPtr lpInitData   // optional printer data
         );

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(
         IntPtr hdcDest, // handle to destination DC目标设备的句柄
         int nXDest,   // x-coord of destination upper-left corner目标对象的左上角的X坐标
         int nYDest,   // y-coord of destination upper-left corner目标对象的左上角的Y坐标
         int nWidth,   // width of destination rectangle目标对象的矩形宽度
         int nHeight, // height of destination rectangle目标对象的矩形长度
         IntPtr hdcSrc,   // handle to source DC源设备的句柄
         int nXSrc,    // x-coordinate of source upper-left corner源对象的左上角的X坐标
         int nYSrc,    // y-coordinate of source upper-left corner源对象的左上角的Y坐标
         UInt32 dwRop   // raster operation code光栅的操作值
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(
         IntPtr hdc // handle to DC
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
         IntPtr hdc,         // handle to DC
         int nWidth,      // width of bitmap, in pixels
         int nHeight      // height of bitmap, in pixels
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
         IntPtr hdc,           // handle to DC
         IntPtr hgdiobj    // handle to object
         );

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(
         IntPtr hdc           // handle to DC
         );

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(
         IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
         IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
         UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
         );

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(
         IntPtr hwnd
         );

        #endregion

        public static void CloseWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return;
            PostMessage(handle, 16, 0, 0);
        }

        public static IntPtr GetSysTrayWindow()
        {
            IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
            hwnd = FindWindowEx(hwnd, IntPtr.Zero, "TrayNotifyWnd", null);
            hwnd = FindWindowEx(hwnd, IntPtr.Zero, "SysPager", null);
            return FindWindowEx(hwnd, IntPtr.Zero, "ToolbarWindow32", null);
        }

        [DllImport("cIcon.dll")]
        public static extern void CleanIcons();

        public static List<Win32WindowInfo> EnumWindows()
        {
            List<Win32WindowInfo> windows = new List<Win32WindowInfo>();
            StringBuilder stringBuilder = new StringBuilder();
            Func<IntPtr, int, bool> proc = (hWnd, lParam) =>
            {
                try
                {
                    string title = null;
                    string className = null;

                    if (stringBuilder == null)
                        stringBuilder = new StringBuilder();
                    stringBuilder.Clear();
                    stringBuilder.Capacity = 256;
                    GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
                    title = stringBuilder.ToString(); //title

                    stringBuilder.Clear();
                    stringBuilder.Capacity = 256;
                    GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
                    className = stringBuilder.ToString(); //calssname

                    int processId;
                    GetWindowThreadProcessId(hWnd, out processId);

                    windows.Add(new Win32WindowInfo(processId, hWnd, title, className));
                    return true;
                }
                catch
                {
                    return true;
                }

            };

            EnumWindows(new EnumWindowsProc(proc), 0);

            return windows;
        }

        public static List<Win32WindowInfo> FindWindows(string titleExpression, string className = null, List<Win32WindowInfo> list = null)
        {
            var all = list;
            if (all == null)
                all = EnumWindows();
            return all.FindAll(o => Regex.IsMatch(o.Title, titleExpression) && (className == null || o.ClassName == className));
        }

        public static List<Win32WindowInfo> FindWindows(int processId, string className = null, List<Win32WindowInfo> list = null)
        {
            var all = list;
            if (all == null)
                all = EnumWindows();
            return all.FindAll(o => o.ProcessId == processId && (className == null || o.ClassName == className));
        }

        public static List<Win32WindowInfo> FindWindows(string titleExpression, int processId, string className = null, List<Win32WindowInfo> list = null)
        {
            var all = list;
            if (all == null)
                all = EnumWindows();
            return all.FindAll(o => Regex.IsMatch(o.Title, titleExpression) && o.ProcessId == processId && (className == null || o.ClassName == className));
        }

        #region 获得UI元素相关
        public static AutomationElement GetAutomationElementFromHandle(IntPtr handle)
        {
            try
            {
                return AutomationElement.FromHandle(handle);
            }
            catch { return null; }
        }

        //public static AutomationElement GetAutoElementByPath(AutomationElement target, int[] path)
        //{
        //    try
        //    {
        //        AutomationElementCollection collection;
        //        foreach (int index in path)
        //        {
        //            collection = target.FindAll(TreeScope.Children, Condition.TrueCondition);
        //            if (collection.Count <= index) return null;
        //            target = collection[index];
        //        }

        //        return target;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static AutomationElement GetAutoElementByPath(IntPtr handle, int[] path)
        //{
        //    var target = GetAutomationElementFromHandle(handle);
        //    if (null == target) return null;

        //    return GetAutoElementByPath(target, path);
        //}

        public static AutomationElement FindAutoElementByPath(IntPtr handle, string[] names)
        {
            var target = GetAutomationElementFromHandle(handle);
            if (target == null) return null;

            foreach (string name in names)
            {
                if (target.Current.Name.Equals(name))
                {
                    return target;
                }
            }

            return FindAutoElementByPath(target, names);
        }

        /// <summary>
        /// 根据name查找窗口句柄中的子元素UI列表
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<AutomationElement> FindAutoElementListByPath(IntPtr handle, string[] names=null)
        {
            var target = GetAutomationElementFromHandle(handle);
            if (target == null) return null;

            return FindAutoElementListByPath(target, names);
        }

        public static List<AutomationElement> FindAutoElementListByPath(AutomationElement target, string[] names)
        {
            var element = new List<AutomationElement>();
            try
            {
                AutomationElementCollection collection= target.FindAll(TreeScope.Children, Condition.TrueCondition); ;

                #region 查询全部子UI元素
                if (names == null)
                {
                    foreach (AutomationElement item in collection)
                    {
                        if (!string.IsNullOrEmpty(item.Current.Name))
                        {
                            element.Add(item);
                        }
                    }
                    return element;
                }
                #endregion

                foreach (string name in names)
                {
                    foreach (AutomationElement item in collection)
                    {
                        if (item.Current.Name.Equals(name))
                        {
                            element.Add(item);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return element;
        }

        /// <summary>
        /// 获得指定其中一个即返回
        /// </summary>
        /// <param name="target"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static AutomationElement FindAutoElementByPath(AutomationElement target, string[] names)
        {
            AutomationElement result = target;
            try
            {
                AutomationElementCollection collection;
                foreach (string name in names)
                {
                    collection = result.FindAll(TreeScope.Children, Condition.TrueCondition);
                    result = null;
                    foreach (AutomationElement item in collection)
                    {
                        if (item.Current.Name.Equals(name))
                        {
                            result = item;
                            break;
                        }
                    }
                    if (result == null)
                        break;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 全局搜索桌面UI元素
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<AutomationElement> FindGlobalElementByName(string names)
        {
            var list = new List<AutomationElement>();
            IntPtr desktopPtr = BaseWin32Api.GetDesktopWindow();

            //2、获得一个子窗口（这通常是一个顶层窗口，当前活动的窗口）
            IntPtr winPtr = BaseWin32Api.GetWindow(desktopPtr, BaseWin32Api.GetWindowCmd.GW_CHILD);

            //3、循环取得桌面下的所有子窗口
            while (winPtr != IntPtr.Zero)
            {
                //4、继续获取下一个子窗口
                winPtr = BaseWin32Api.GetWindow(winPtr, BaseWin32Api.GetWindowCmd.GW_HWNDNEXT);

                var uiElement = Utility.FindAutoElementByPath(winPtr, new string[] {  names });
                if (uiElement != null)
                {
                    if (uiElement.Current.Name.Contains(names))
                    {
                        list.Add(uiElement);
                    }
                }
            }
            return list;
        }
        #endregion

        #region 鼠标相关
        public static void PostMouseEvent(IntPtr hwnd, System.Windows.Point point, MouseEvent type = MouseEvent.LeftClick)
        {
            int position = (int)point.X + 65536 * (int)point.Y;
            PostMessage(hwnd, 512, 2, position);

            switch (type)
            {
                case MouseEvent.LeftClick:
                    PostMessage(hwnd, 513, 1, position);
                    PostMessage(hwnd, 514, 0, position);
                    break;
                case MouseEvent.LeftDoubleClick:
                    PostMessage(hwnd, 513, 1, position);
                    PostMessage(hwnd, 514, 0, position);
                    PostMessage(hwnd, 515, 0, position);
                    break;
                case MouseEvent.LeftDown:
                    PostMessage(hwnd, 513, 1, position);
                    break;
                case MouseEvent.LeftUp:
                    PostMessage(hwnd, 514, 0, position);
                    break;
                case MouseEvent.RightClick:
                    PostMessage(hwnd, 516, 2, position);
                    PostMessage(hwnd, 517, 2, position);
                    break;
                case MouseEvent.RightDoubleClick:
                    PostMessage(hwnd, 516, 2, position);
                    PostMessage(hwnd, 517, 2, position);
                    PostMessage(hwnd, 518, 0, position);
                    break;
                case MouseEvent.RightDown:
                    PostMessage(hwnd, 516, 2, position);
                    break;
                case MouseEvent.RightUp:
                    PostMessage(hwnd, 517, 2, position);
                    break;
            }
        }

        public static void PostKey(IntPtr hwnd, int keycode, bool syskey)
        {
            if (syskey)
            {
                PostMessage(hwnd, 260, keycode, 0);
                PostMessage(hwnd, 261, keycode, 0);
            }
            else
            {
                PostMessage(hwnd, 256, keycode, 0);
                PostMessage(hwnd, 257, keycode, 0);
            }
        }

        public static void PostText(IntPtr hwnd, string text)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(text);
            if (bytes == null) return;

            foreach (var bt in bytes)
            {
                PostMessage(hwnd, 258, (int)bt, 0);
            }
        }

        public static void SendKeysForeGround(string text)
        {
            foreach (char c in text)
            {
                SendKeysForeGround(c);
            }
        }

        public static void SendKeysForeGround(char key)
        {
            keybd_event((int)key, 0, 0, 0);
            System.Windows.Forms.Application.DoEvents();
            keybd_event((int)key, 0, 2, 0);
            System.Windows.Forms.Application.DoEvents();
        }
        #endregion

        #region 其他相关

        [DllImport("WinSpy.dll")]
        public static extern void Keybd_Sendkeys(string text);

        [DllImport("WinSpy.dll")]
        public static extern void Keybd_Sendkey(int keychar);

        [DllImport("WinSpy.dll")]
        public static extern bool ChangeIPADSL(string name, string user, string pwd);

        [DllImport("WinSpy.dll")]
        public static extern int CaptureWindowArea(int x1, int y1, int x2, int y2, string file);

        [DllImport("WinSpy.dll")]
        public static extern bool GetADSLInfo(StringBuilder name, StringBuilder user, StringBuilder pwd);

        [DllImport("WinSpy.dll")]
        public static extern void MAC_Restart();

        [DllImport("WinSpy.dll")]
        public static extern void MouseClick(int x, int y);  //前台单击屏幕坐标

        [DllImport("WinSpy.dll")]
        public static extern void MouseClickMsg(IntPtr hwnd, int x, int y);

        [DllImport("WinSpy.dll")]
        public static extern void MouseMove(int x, int y);

        [DllImport("WinSpy.dll")]
        public static extern bool SetWindowPosition(IntPtr hwnd, int width, int height);

        [DllImport("WinSpy.dll")]
        public static extern void RefreshSysIcon();

        [DllImport("WinSpy.dll")]
        public static extern void GetMousePosition(ref int x, ref int y);

        [DllImport("WinSpy.dll")]
        public static extern void GetScreenSize(ref int x, ref int y);

        public static void ForceShowWindow(IntPtr hwnd, bool alwaysTop)
        {
            ShowWindow(hwnd, 9);
            SetForegroundWindow(hwnd);
            if (alwaysTop)
            {
                SetWindowPos(hwnd, -1, 0, 0, 0, 0, 3);
            }
            else
            {
                SetWindowPos(hwnd, -2, 0, 0, 0, 0, 3);
            }
        }

        public static Bitmap GetWindowCapture(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            Rectangle windowRect = new Rectangle();
            GetWindowRect(hWnd, ref windowRect);
            int width = windowRect.Width - windowRect.X;
            int height = windowRect.Height - windowRect.Y;
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//删除用过的对象
            DeleteDC(hmemdc);//删除用过的对象
            return bmp;
        }

        public static Bitmap GetBitmapPart(Bitmap oriPic, int pPartStartPointX, int pPartStartPointY, int pPartWidth, int pPartHeight, int pOrigStartPointX, int pOrigStartPointY)
        {
            Bitmap partImg = new Bitmap(pPartWidth, pPartHeight);
            Graphics graphics = Graphics.FromImage(partImg);
            Rectangle destRect = new Rectangle(new Point(pPartStartPointX, pPartStartPointY), new Size(pPartWidth, pPartHeight));//目标位置
            Rectangle origRect = new Rectangle(new Point(pOrigStartPointX, pOrigStartPointY), new Size(pPartWidth, pPartHeight));//原图位置（默认从原图中截取的图片大小等于目标图片的大小）

            graphics.DrawImage(oriPic, destRect, origRect, GraphicsUnit.Pixel);
            graphics.Dispose();
            return partImg;
        }

        public static Bitmap CaptureWindow(IntPtr hwnd, int x, int y, int width, int height)
        {
            Bitmap bp1 = null;
            Bitmap bp2 = null;
            try
            {
                bp1 = GetWindowCapture(hwnd);
                bp2 = GetBitmapPart(bp1, 0, 0, width, height, x, y);
                return bp2;
            }
            catch
            {
                if (bp2 != null)
                    bp2.Dispose();
                return null;
            }
            finally
            {
                if (bp1 != null)
                    bp1.Dispose();
            }
        }

        public static Bitmap CaptureScreenArea(int x, int y, int width, int height)
        {
            //Screen scr = Screen.PrimaryScreen;
            //Rectangle rc = scr.Bounds;
            //int iWidth = rc.Width;
            //int iHeight = rc.Height;
            Bitmap myImage = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(x, y), new Point(0, 0), new Size(width, height));
            return myImage;
        }

        public static Bitmap CaptureWindowScreenArea(IntPtr hwnd, int x, int y, int width, int height)
        {
            lock (syncLock)
            {
                var rect = new Rectangle(x, y, width, height);
                ClientToScreen(hwnd, ref rect);
                //return CaptureScreenArea(rect.X, rect.Y, rect.Width, rect.Height);
                string file = Path.Combine(Application.StartupPath, "tmocpt.bmp");
                //FileInfo fi = new FileInfo(Path.Combine(Application.StartupPath, "dm.dll"));
                //if (fi.Length != 823296)
                //{ 

                //}
                int ret = CaptureWindowArea(rect.X, rect.Y, rect.Right, rect.Bottom, file);
                Image bit = null;
                int timeout = 5;
                while (timeout-- > 0 && !File.Exists(file))
                    System.Threading.Thread.Sleep(1000);
                if (!File.Exists(file))
                    throw new FileNotFoundException("截图保存的验证码图片不存在！");
                if (File.Exists(file))
                {
                    using (var fs = File.OpenRead(file))
                    {
                        var tmp = new Bitmap(fs);
                        bit = new Bitmap(tmp);
                        tmp.Dispose();
                        bit.Save(Path.Combine(Application.StartupPath, "bmpcache.bmp"));
                    }
                }
                try
                {
                    File.Delete(file);
                }
                catch { }
                return bit as Bitmap;
            }
        }

        public static decimal GetStringSimilarity(string str1, string str2)
        {
            return 0.0M;
            //return LevenshteinDistance.Instance.LevenshteinDistancePercent(str1, str2);
        }

        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = System.Drawing.Image.FromStream(ms);
            ms.Dispose();
            return image;
        }

        public static int BitmapCompare(Bitmap bitmap1, Bitmap bitmap2)
        {
            int result = 0; //假设两幅图片相同
            if (bitmap1 == null || bitmap2 == null)
                return -1;
            if (bitmap1.Width == bitmap2.Width && bitmap1.Height == bitmap2.Height)
            {
                for (int i = 0; i < bitmap1.Width; i++)
                {
                    for (int j = 0; j < bitmap1.Height; j++)
                    {
                        Color color1 = bitmap1.GetPixel(i, j);
                        Color color2 = bitmap2.GetPixel(i, j);
                        if (color1 != color2)
                        {
                            result = color1.ToArgb() - color2.ToArgb();
                            break;
                        }
                    }
                    if (result != 0)
                        break;
                }
            }
            else if (bitmap1.Width != bitmap2.Width)
            {
                result = bitmap1.Width - bitmap2.Width;
            }
            else if (bitmap1.Height != bitmap2.Height)
            {
                result = bitmap1.Height - bitmap2.Height;
            }
            return result;
        }

        public static bool IsSameBitmap(Bitmap bitmap1, Bitmap bitmap2)
        {
            return BitmapCompare(bitmap1, bitmap2) == 0;
        }

        [DllImport("sensapi.dll")]
        private extern static bool IsNetworkAlive(out int connectionDescription);

        //public static bool IsNetworkAlive()
        //{
        //    string msg;
        //    return IsNetworkAlive(out msg);
        //}

        //public static bool IsNetworkAlive(out string outPut)
        //{
        //    outPut = "Unknown";

        //    int NETWORK_ALIVE_LAN = 0;
        //    int NETWORK_ALIVE_WAN = 2;
        //    int NETWORK_ALIVE_AOL = 4;

        //    int flags;//上网方式 
        //    bool m_bOnline = true;//是否在线 

        //    m_bOnline = IsNetworkAlive(out flags);
        //    if (m_bOnline)//在线   
        //    {
        //        if ((flags & NETWORK_ALIVE_LAN) == NETWORK_ALIVE_LAN)
        //        {
        //            outPut = "在线：NETWORK_ALIVE_LAN";
        //        }
        //        if ((flags & NETWORK_ALIVE_WAN) == NETWORK_ALIVE_WAN)
        //        {
        //            outPut = "在线：NETWORK_ALIVE_WAN";
        //        }
        //        if ((flags & NETWORK_ALIVE_AOL) == NETWORK_ALIVE_AOL)
        //        {
        //            outPut = "在线：NETWORK_ALIVE_AOL";
        //        }
        //    }
        //    else
        //    {
        //        outPut = "不在线";
        //    }

        //    return m_bOnline;
        //}


        public static bool IsOnline()
        {
            //var http = new HttpHelper();
            //var item = new HttpItem
            //{
            //    URL = "http://www.baidu.com"
            //};
            //var result = http.GetHtml(item);

            //return (int)result.StatusCode != 0;
            return false;
        }

        public static void ToGrey(this Bitmap img1)
        {
            if (img1 == null) return;

            for (int i = 0; i < img1.Width; i++)
            {
                for (int j = 0; j < img1.Height; j++)
                {
                    Color pixelColor = img1.GetPixel(i, j);
                    //计算灰度值
                    int grey = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                    Color newColor = Color.FromArgb(grey, grey, grey);
                    img1.SetPixel(i, j, newColor);
                }
            }
        }
        public static void Thresholding(this Bitmap img1)
        {
            if (img1 == null) return;

            try
            {
                int[] histogram = new int[256];
                int minGrayValue = 255, maxGrayValue = 0;
                //求取直方图
                for (int i = 0; i < img1.Width; i++)
                {
                    for (int j = 0; j < img1.Height; j++)
                    {
                        Color pixelColor = img1.GetPixel(i, j);
                        histogram[pixelColor.R]++;
                        if (pixelColor.R > maxGrayValue) maxGrayValue = pixelColor.R;
                        if (pixelColor.R < minGrayValue) minGrayValue = pixelColor.R;
                    }
                }
                //迭代计算阀值
                int threshold = -1;
                int newThreshold = (minGrayValue + maxGrayValue) / 2;
                for (int iterationTimes = 0; threshold != newThreshold && iterationTimes < 100; iterationTimes++)
                {
                    threshold = newThreshold;
                    int lP1 = 0;
                    int lP2 = 0;
                    int lS1 = 0;
                    int lS2 = 0;
                    //求两个区域的灰度的平均值
                    for (int i = minGrayValue; i < threshold; i++)
                    {
                        lP1 += histogram[i] * i;
                        lS1 += histogram[i];
                    }
                    int mean1GrayValue = (lP1 / lS1);
                    for (int i = threshold + 1; i < maxGrayValue; i++)
                    {
                        lP2 += histogram[i] * i;
                        lS2 += histogram[i];
                    }
                    int mean2GrayValue = (lP2 / lS2);
                    newThreshold = (mean1GrayValue + mean2GrayValue) / 2;
                }
                //计算二值化
                for (int i = 0; i < img1.Width; i++)
                {
                    for (int j = 0; j < img1.Height; j++)
                    {
                        Color pixelColor = img1.GetPixel(i, j);
                        if (pixelColor.R > threshold) img1.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                        else img1.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            catch { }
        }

        public static int TotalBlackPixels(this Bitmap img1)
        {
            if (img1 == null) return 0;

            int count = 0;
            for (int i = 0; i < img1.Width; i++)
            {
                for (int j = 0; j < img1.Height; j++)
                {
                    Color pixelColor = img1.GetPixel(i, j);
                    if (pixelColor.R == 0) //黑色
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                    reg.SetValue(name, fileName);
                else
                    reg.SetValue(name, false);
            }
            catch
            {
                //throw new Exception(ex.ToString());  
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }

        public static Point RandomMoveMouse(IntPtr handle, Rectangle destRect, bool relative = true, int speed = 50, int nodeCount = 3)
        {
            int x = 0, y = 0;

            if (handle == IntPtr.Zero)
                handle = GetDesktopWindow();
            var ctl = GetAutomationElementFromHandle(handle);
            if (ctl == null)
            {
                GetMousePosition(ref x, ref y);
                return new Point(x, y);
            }

            var rectm = ctl.Current.BoundingRectangle;
            var rectc = default(Rectangle);

            if (destRect != default(Rectangle))
            {
                if (relative)
                    ClientToScreen(handle, ref destRect);
                rectc = destRect;
            }

            GetMousePosition(ref x, ref y);

            //make the nodes
            List<Point> nodes = new List<Point>();
            nodes.Add(new Point(x, y));
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(new Point(random.Next((int)rectm.X, (int)rectm.Right), random.Next((int)rectm.Y, (int)rectm.Bottom)));
            }
            if (rectc != default(Rectangle))
            {
                nodes.Add(new Point(random.Next((int)rectc.X, (int)rectc.Right), random.Next((int)rectc.Y, (int)rectc.Bottom)));
            }

            Func<Point, Point, List<Point>> makeRoute = (a, b) =>
            {
                int xspan = 30;
                int yspan = 30;
                var list = new List<Point>();
                int step = Math.Abs((b.X - a.X)) / xspan;
                if (b.X < a.X) xspan = 0 - xspan;
                if (step != 0)
                {
                    yspan = (b.Y - a.Y) / step;
                }
                else
                {
                    step = Math.Abs((b.Y - a.Y)) / yspan;
                    if (step != 0)
                    {
                        xspan = (b.X - a.X) / step;
                    }
                }

                for (int i = 1; i <= step; i++)
                {
                    list.Add(new Point(a.X + i * xspan, a.Y + (i - 1) * yspan + (yspan > 0 ? random.Next(yspan / 2, yspan) : random.Next(yspan, yspan / 2))));
                }
                list.Add(b);

                return list;
            };

            //now let's move
            List<Point> route = new List<Point>();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                route.AddRange(makeRoute(nodes[i], nodes[i + 1]));
            }
            foreach (var p in route)
            {
                System.Threading.Thread.Sleep(speed);
                MouseMove(p.X, p.Y);
            }
            if (route.Count == 0)
            {
                GetMousePosition(ref x, ref y);
                return new Point(x, y);
            }
            else
                return route.LastOrDefault();
        }

        //public static Point MoveMouseToDesktopSpace()
        //{
        //    IntPtr desktop = GetDesktopWindow();
        //    int width = 0, height = 0;
        //    GetScreenSize(ref width, ref height);

        //    bool succ = false;
        //    Point p = default(Point);
        //    while (!succ)
        //    {
        //        p.X = random.Next(0, width);
        //        p.Y = random.Next(0, height);

        //        IntPtr hwnd = WindowFromPoint(p.X, p.Y);
        //        if (hwnd == IntPtr.Zero || hwnd == desktop)
        //            succ = true;
        //    }
        //    MouseMove(p.X, p.Y);
        //    return p;
        //}

        public static void ToggleDesktop()
        {
            Type shellType = Type.GetTypeFromProgID("Shell.Application");
            object shellObject = System.Activator.CreateInstance(shellType);
            shellType.InvokeMember("ToggleDesktop", System.Reflection.BindingFlags.InvokeMethod,
                null, shellObject, null);
        }
        #endregion
    }
}
