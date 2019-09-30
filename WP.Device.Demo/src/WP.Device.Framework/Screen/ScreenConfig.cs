using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP.Device.Framework.Screen
{
    /// <summary>
    /// 截图坐标配置
    /// </summary>
    public class ScreenConfig
    {
        /// <summary>
        /// 左上角坐标
        /// </summary>
        public Size LeftTopCoordinate { set; get; }

        /// <summary>
        /// 左下角坐标
        /// </summary>
        public Size LeftBottomCoordinate { set; get; }

        /// <summary>
        /// 右上角坐标
        /// </summary>
        public Size RightTopCoordinate { set; get; }

        /// <summary>
        /// 右下角坐标
        /// </summary>
        public Size RightBottomCoordinate { set; get; }

        /// <summary>
        /// OCR 识别语言库地址
        /// </summary>
        public string OCRResourcePath { set; get; }
    }
}
