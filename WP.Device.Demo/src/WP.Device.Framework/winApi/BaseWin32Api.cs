using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WP.Device.Framework
{
    /// <summary>
    /// 底层操作系统API
    /// </summary>
    public partial class BaseWin32Api
    {
        #region 键盘钩子
        /// <summary>
        /// 声明键盘钩子的封送结构类型
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            /// <summary>
            /// 表示一个在1到254间的虚似键盘码
            /// </summary>
            public int vkCode; // 

            /// <summary>
            /// 表示硬件扫描码
            /// </summary>
            public int scanCode;

            /// <summary>
            /// 键标志
            /// </summary>
            public int flags;

            /// <summary>
            /// 指定的时间戳记的这个讯息
            /// </summary>
            public int time;

            /// <summary>
            ///  指定额外信息相关的信息
            /// </summary>
            public int dwExtraInfo;

        }

        #region 系统钩子相关win32 api

        public delegate int HookProcess(int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// 安装钩子的函数
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProcess lpfn, IntPtr hInstance, int threadId);

        /// <summary>
        /// 卸下钩子的函数
        /// </summary>
        /// <param name="idHook"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        /// <summary>
        /// 下一个钩挂的函数
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        #endregion

        /// <summary>
        /// 获得模块指针地址
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// 获取按键的状态
        /// </summary>
        /// <param name="pbKeyState"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="VirtualKey"></param>
        /// <param name="ScanCode"></param>
        /// <param name="lpKeySate"></param>
        /// <param name="lpChar"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "ToAscii")]
        public static extern bool ToAscii(int virtualKey, int scanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);

        /// <summary>
        /// 获取键值名称
        /// </summary>
        /// <param name="IParam"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        public static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);
        #endregion

        #region 窗口相关
        /// <summary>
        /// 获取窗体句柄
        /// </summary>，两个参数至少要知道一个
        /// <param name="lpClassName">窗体类名，可以通过Spy++获取，为null表示忽略</param>
        /// <param name="lpWindowName">窗体标题，Text属性，为null时表示忽略</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        public extern static IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        /// <summary>
        /// 获得桌面句柄
        /// </summary>
        /// <returns></returns>

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// 获得DC
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hrgnClip"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetDCEx", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

        /// <summary>
        /// 重绘桌面
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="rcUpdate"></param>
        /// <param name="hrgnUpdate"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool RedrawWindow(IntPtr hwnd, WindowRect rcUpdate, IntPtr hrgnUpdate, int flags);

        ///// <summary>
        ///// 根据窗体句柄获得窗体标题
        ///// </summary>
        ///// <param name="hWnd"></param>
        ///// <param name="lpText"></param>
        ///// <param name="nCount"></param>
        ///// <returns></returns>
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpText, int nCount);

        /// <summary>
        /// 屏幕上所有的父窗口
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int EnumWindows(CallBack x, int y);
        //public delegate bool CallBack(IntPtr hwnd, int lParam);

        ///// <summary>
        ///// 根据窗体句柄获得其进程ID
        ///// </summary>
        ///// <param name="hwnd"></param>
        ///// <param name="ID"></param>
        ///// <returns></returns>
        //[DllImport("User32.dll", CharSet = CharSet.Auto)]
        //private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        ///// <summary>
        ///// 修改位置、大小
        ///// </summary>
        ///// <param name="hWnd"></param>
        ///// <param name="hWndInsertAfter"></param>
        ///// <param name="X"></param>
        ///// <param name="Y"></param>
        ///// <param name="cx"></param>
        ///// <param name="cy"></param>
        ///// <param name="uFlags"></param>
        ///// <returns></returns>
        //[DllImport("user32.dll")]
        //private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        ///// <summary>
        /////     Retains the current size (ignores the cx and cy parameters).
        ///// </summary>
        //static uint SWP_NOSIZE = 0x0001;
        //static int HWND_TOP = 0;
        //public struct Rect
        //{
        //    public int Left;
        //    public int Top;
        //    public int Right;
        //    public int Bottom;
        //}

        //[DllImport("user32.dll")]
        //private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        ///// <summary>
        ///// 显示窗体,同  ShowWindowAsync 差不多
        ///// </summary>
        ///// <param name="hwnd"></param>
        ///// <param name="nCmdShow"></param>
        ///// <returns></returns>
        //[DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        //private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        //private const int SW_RESTORE = 9;


        ///// <summary> 
        ///// 该函数设置由不同线程产生的窗口的显示状态。 (没用)
        ///// </summary> 
        ///// <param name="hWnd">窗口句柄</param> 
        ///// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分。</param> 
        ///// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零。</returns> 
        //[DllImport("User32.dll")]
        //private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        ///// <summary> 
        ///// 该函数将创建指定窗口的线程设置到前台，并且激活该窗口。
        ///// 键盘输入转向该窗口，并为用户改各种可视的记号。系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        ///// (没用)
        ///// </summary> 
        ///// <param name="hWnd">将被激活并被调入前台的窗口句柄。</param> 
        ///// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零。</returns> 
        //[DllImport("User32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);
        //private const int WS_SHOWNORMAL = 1;

        ///// <summary>
        ///// 窗体焦点
        ///// </summary>
        ///// <param name="hWnd"></param>
        ///// <param name="fAltTab"></param>
        //[DllImport("user32.dll ", SetLastError = true)]
        //private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        public struct WindowPoint
        {
            int x;
            int y;
        }

        public class WindowRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// 获得DC
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        /// <summary>
        /// 根据当前定位获得句柄窗口
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(WindowPoint Point);

        /// <summary>
        /// 获得当前定位
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out WindowPoint lpPoint);

        #region 获取子窗口
        /// <summary>
        /// 获取子窗口
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="lpfn"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);
        #endregion

        /// <summary>
        /// 获得子级窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="uCmd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        public static WindowPoint GetCursorPos()
        {
            WindowPoint point;
            if (GetCursorPos(out point))
            {
                return point;
            }
            throw new Exception();
        }

        public static IntPtr WindowFromPoint()
        {
            var point = GetCursorPos();
            return WindowFromPoint(point);
        }

        public enum GetWindowCmd : uint
        {
            /// <summary>
            /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// 返回的句柄标识了在z序最低端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
            /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
            /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
            /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
            /// 如果无使能窗口，则获得的句柄与指定窗口相同。
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }
        #endregion


    }
}
