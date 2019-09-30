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
    }
}
