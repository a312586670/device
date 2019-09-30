using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Wp.Device.WPF.AppDemo.Plugins.Event;
using static Wp.Device.WPF.AppDemo.Plugins.InterceptWin32Api;
using static Wp.Device.WPF.AppDemo.Plugins.KeyboardHookPlugins;

namespace Wp.Device.WPF.AppDemo.Plugins
{
    /// <summary>
    /// 键盘全局钩子管理器
    /// </summary>
    public class KeyBordHookManager
    {
        static KeyboardHookPlugins keyboardHookPlugins;

        public static void AddInputHandler(BoardInputEventHandler inputEventHandler)
        {
            keyboardHookPlugins = new KeyboardHookPlugins();
            keyboardHookPlugins.OnInputEvent += inputEventHandler;

            //keyboardHookPlugins.OnKeyUpEvent += keyUpEventHandler;
            //keyboardHookPlugins.OnKeyPressEvent += keyPressEventHandler;

            keyboardHookPlugins.Start();
        }
    }

    #region 钩子相关实现方法
    /// <summary>
    /// 全局钩子插件方法
    /// </summary>
    public class KeyboardHookPlugins
    {
        public delegate void BoardInputEventHandler(object sender, BoardInputEventArgs eventArgs);

        /// <summary>
        /// 声明KeyboardHookProcedure作为HookProc类型
        /// </summary>
        InterceptWin32Api.HookProc KeyboardHookProcedure;

        //public event CustomizeKeyEventHandler OnKeyDownEvent;
        //public event CustomizeKeyEventHandler OnKeyUpEvent;
        public event BoardInputEventHandler OnInputEvent;
        static int hKeyboardHook = 0; //声明键盘钩子处理的初始值

        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        public void Start()
        {
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new InterceptWin32Api.HookProc(KeyboardHookProc);

                ProcessModule cModule = Process.GetCurrentProcess().MainModule;

                var mhIntPrt = InterceptWin32Api.GetModuleHandle(cModule.ModuleName);

                hKeyboardHook = InterceptWin32Api.SetWindowsHookEx(InterceptWin32Api.WH_KEYBOARD_LL, KeyboardHookProcedure, mhIntPrt, 0);

                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }

        public void Stop()
        {
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = InterceptWin32Api.UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }

            if (!(retKeyboard))
                throw new Exception("卸载钩子失败！");
        }

        /// <summary>
        /// 获取键盘消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {


                #region old

                KeyboardHookStruct KeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

                #region 暂时不用
                //int vkCode = Marshal.ReadInt32(lParam);
                ////WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件
                //if (OnKeyDownEvent != null && (wParam == InterceptWin32Api.WM_KEYDOWN || wParam == InterceptWin32Api.WM_SYSKEYDOWN))
                //{
                //    // 此处触发键盘按下事件
                //    // keyData为按下键盘的值,对应 虚拟码
                //    CustomizeKeyEventArgs e = new CustomizeKeyEventArgs(vkCode);
                //    OnKeyDownEvent(this, e);
                //}

                //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件 

                //if (OnKeyUpEvent != null && (wParam == InterceptWin32Api.WM_KEYUP || wParam == InterceptWin32Api.WM_SYSKEYUP))
                //{
                //    // 此处触发键盘抬a起事件
                //    CustomizeKeyEventArgs e = new CustomizeKeyEventArgs(vkCode);
                //    OnKeyUpEvent(this, e);
                //}
                #endregion

                #region 输入
                if (OnInputEvent != null && wParam == WM_KEYUP)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (ToAscii(KeyboardHookStruct.vkCode, KeyboardHookStruct.scanCode, keyState, inBuffer, KeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        OnInputEvent(this, new BoardInputEventArgs() { KeyChar = e.KeyChar });
                    }
                }
                #endregion

                #endregion
            }

            return InterceptWin32Api.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        ~KeyboardHookPlugins()
        {
            Stop();
        }

    }

    //public class CustomizeKeyEventArgs : EventArgs
    //{
    //    public int VirtualKeyCode;
    //    public Key Key;
    //    public Keys FormKeys;
    //    //public bool IsSysKey;

    //    public CustomizeKeyEventArgs(int vkCode)
    //    {
    //        this.VirtualKeyCode = vkCode;
    //        //this.IsSysKey = isSysKey;
    //        this.Key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(vkCode);
    //        this.FormKeys = (Keys)KeyInterop.VirtualKeyFromKey(this.Key);
    //    }
    //}
    #endregion

}
