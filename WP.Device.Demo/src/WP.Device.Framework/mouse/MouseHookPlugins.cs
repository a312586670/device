using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WP.Device.Framework;
using WP.Device.Framework.Helper;

namespace WP.Device.Framework
{
    public class MouseHookPlugins
    {
        #region 定义

        /// <summary>
        /// 坐标点
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 钩子结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public POINT pt;
            public int hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        /// <summary>
        /// 钩子回调函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        // 声明鼠标钩子事件类型
        private BaseWin32Api.HookProcess _mouseHookProcedure;
        private static int _hMouseHook = 0; //鼠标钩子句柄

        // 全局的鼠标事件
        public event MouseEventHandler OnMouseMoveHandler;

        public event MouseEventHandler OnMouseClickHanler;
        #endregion

        #region 构造和析构函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public MouseHookPlugins()
        {
        }
        
        /// <summary>
        /// 析构函数
        /// </summary>
        ~MouseHookPlugins()
        {
            Stop();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 启动全局钩子
        /// </summary>
        public void Start()
        {
            // 安装鼠标钩子
            if (_hMouseHook == 0)
            {
                // 生成一个HookProc的实例.
                _mouseHookProcedure = new BaseWin32Api.HookProcess(MouseHookProc);

                ProcessModule cModule = Process.GetCurrentProcess().MainModule;
                var mhIntPrt = BaseWin32Api.GetModuleHandle(cModule.ModuleName);

                _hMouseHook = BaseWin32Api.SetWindowsHookEx(ConstDefintion.WH_MOUSE_LL, _mouseHookProcedure, mhIntPrt, 0);

                //假设装置失败停止钩子
                if (_hMouseHook == 0)
                {
                    Stop();
                    throw new Exception("安装鼠标钩子失败...");
                }
            }
        }

        /// <summary>
        /// 停止全局钩子
        /// </summary>
        public void Stop()
        {
            bool retMouse = true;

            if (_hMouseHook != 0)
            {
                retMouse = BaseWin32Api.UnhookWindowsHookEx(_hMouseHook);
                _hMouseHook = 0;
            }

            // 假设卸下钩子失败
            if (!(retMouse))
                throw new Exception("UnhookWindowsHookEx failed.");
        }
        #endregion

        #region 私有方法

        private MouseEventArgs GetMouseEventArgs(Int32 wParam, IntPtr lParam)
        {
            int clickCount = 0;
            MouseButtons button = MouseButtons.None;
            switch (wParam)
            {
                case ConstDefintion.WM_LBUTTONDOWN:
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case ConstDefintion.WM_LBUTTONUP:
                    button = MouseButtons.Left;
                    clickCount = 1;
                    break;
                case ConstDefintion.WM_LBUTTONDBLCLK:
                    button = MouseButtons.Left;
                    clickCount = 2;
                    break;
                case ConstDefintion.WM_RBUTTONDOWN:
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case ConstDefintion.WM_RBUTTONUP:
                    button = MouseButtons.Right;
                    clickCount = 1;
                    break;
                case ConstDefintion.WM_RBUTTONDBLCLK:
                    button = MouseButtons.Right;
                    clickCount = 2;
                    break;
            }

            TextHelper.Write($"button:{button.ToString()},clientCount:{clickCount}"); ;
            // 从回调函数中得到鼠标的信息
            MouseHookStruct mouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            return new MouseEventArgs(button, clickCount, mouseHookStruct.pt.x, mouseHookStruct.pt.y, 0);
        }

        /// <summary>
        /// 鼠标钩子回调函数
        /// </summary>
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 假设正常执行而且用户要监听鼠标的消息
            if ((nCode >= 0))
            {
                var eventArgs = GetMouseEventArgs(wParam, lParam);
               
                switch (wParam)
                {
                    case ConstDefintion.WM_LBUTTONDOWN:
                        OnMouseClickHanler(this, eventArgs);
                        break;
                    case ConstDefintion.WM_LBUTTONUP:
                        break;
                    case ConstDefintion.WM_LBUTTONDBLCLK:
                        break;
                    case ConstDefintion.WM_MOUSEMOVE:
                        OnMouseMoveHandler(this, eventArgs);
                        break;
                    case ConstDefintion.WM_RBUTTONDOWN:
                        break;
                    case ConstDefintion.WM_RBUTTONUP:
                        break;
                    case ConstDefintion.WM_RBUTTONDBLCLK:
                        break;
                }
            }

            //启动下一次钩子
            return BaseWin32Api.CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
        }
        #endregion
    }
}
