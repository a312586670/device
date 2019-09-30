using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wp.Device.WPF.AppDemo.Plugins
{
    public class InterceptWin32Api
    {
        /// <summary>
        /// 键盘按下
        /// </summary>
        public const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// 按下放开
        /// </summary>
        public const int WM_KEYUP = 0x101;

        public const int WM_SYSKEYDOWN = 0x104;

        public const int WM_SYSKEYUP = 0x105;

        public const int WH_KEYBOARD_LL = 13;



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

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// 安装钩子的函数
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

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
        /// ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符
        /// </summary>
        /// <param name="uVirtKey">指定虚拟关键代码进行翻译。</param>
        /// <param name="uScanCode">指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压）</param>
        /// <param name="lpbKeyState">指针，以256字节数组，包含当前键盘的状态。每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。在低比特，如果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。在切换状态的NUM个锁和滚动锁定键被忽略。</param>
        /// <param name="lpwTransKey">指针的缓冲区收到翻译字符或字符。</param>
        /// <param name="fuState">Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise.</param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
    }
}
