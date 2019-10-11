using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP.Device.Framework
{
    /// <summary>
    /// 常量定义
    /// </summary>
    public class ConstDefintion
    {
        #region 键盘钩子使用的常量定义
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

        #endregion

        #region 鼠标钩子常量定义
        /// <summary>
        /// 全局钩子鼠标为14
        /// </summary>
        public const int WH_MOUSE_LL = 14;

        /// <summary>
        /// 移动鼠标时发生，同WM_MOUSEFIRST
        /// </summary>
        public const int WM_MOUSEMOVE = 0x200;

        /// <summary>
        /// 按下鼠标左键
        /// </summary>
        public const int WM_LBUTTONDOWN = 0x201;

        /// <summary>
        /// 释放鼠标左键
        /// </summary>
        public const int WM_LBUTTONUP = 0x202;

        /// <summary>
        /// 双击鼠标左键
        /// </summary>
        public const int WM_LBUTTONDBLCLK = 0x203;

        /// <summary>
        /// 按下鼠标右键
        /// </summary>
        public const int WM_RBUTTONDOWN = 0x204;

        /// <summary>
        /// 释放鼠标右键
        /// </summary>
        public const int WM_RBUTTONUP = 0x205;

        /// <summary>
        /// 双击鼠标右键
        /// </summary>
        public const int WM_RBUTTONDBLCLK = 0x206;

        /// <summary>
        /// 按下鼠标中键
        /// </summary>
        public const int WM_MBUTTONDOWN = 0x207;

        /// <summary>
        /// 释放鼠标中键
        /// </summary>
        public const int WM_MBUTTONUP = 0x208;

        /// <summary>
        /// 双击鼠标中键
        /// </summary>
        public const int WM_MBUTTONDBLCLK = 0x209;
        #endregion

        #region OCR解析相关常量定义

        /// <summary>
        /// OCR 解析金钱替换处理特殊字符
        /// </summary>
        public static readonly Dictionary<string, string> MONEY_REPLACE = new Dictionary<string, string>() {
            { "o","0" },
            { "。","0"},
            { " ",""},
            { "′",""},
            { "-","."},
            { "s","6"},
            { "'",","}
            //{ "j","."}
        };

        /// <summary>
        /// OCR 解析金钱移除处理的特殊字符
        /// </summary>
        public static readonly List<char> MONEY_REMOVE = new List<char> { '′', ' '};
        #endregion
    }
}
