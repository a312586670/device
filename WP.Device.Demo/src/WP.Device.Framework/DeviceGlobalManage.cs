using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WP.Device.Framework.Screen;
using static WP.Device.Framework.KeyBoardHookPlugins;
using static WP.Device.Framework.Screen.ScreenTimerPlugins;

namespace WP.Device.Framework
{
    /// <summary>
    /// 设备全局管理器
    /// </summary>
    public class DeviceGlobalManage
    {
        #region 定义属性
        /// <summary>
        /// 全局键盘钩子插件
        /// </summary>
        static KeyBoardHookPlugins _keyboardHookPlugins;
        static ScreenTimerPlugins _screenTimerPlugins;
        static MouseHookPlugins _mouseHookPlugins;
        #endregion

        #region 注册设备相关方法
        /// <summary>
        /// 注册全局键盘钩子,注册全局钩子，退出时需要卸载全局键盘钩子
        /// </summary>
        /// <param name="dataEvent">回调事件</param>
        public static void Register(BoardDataEventHandler dataEvent)
        {
            _keyboardHookPlugins = new KeyBoardHookPlugins();
            _keyboardHookPlugins.OnDataEvent += dataEvent;

            _keyboardHookPlugins.Start();
        }

        /// <summary>
        /// 卸载全局键盘钩子
        /// </summary>
        public static void UnRegister()
        {
            _keyboardHookPlugins.Stop();
        }
        #endregion

        #region 注册鼠标全局钩子

        /// <summary>
        /// 注册鼠标全局钩子
        /// </summary>
        /// <param name="mouseMoveEvent"></param>
        /// <param name="mouseClickEvent"></param>
        public static void MouseRegister(MouseEventHandler mouseMoveEvent,MouseEventHandler mouseClickEvent)
        {
            _mouseHookPlugins = new MouseHookPlugins();
            _mouseHookPlugins.OnMouseMoveHandler += mouseMoveEvent;
            _mouseHookPlugins.OnMouseClickHanler += mouseClickEvent;
            _mouseHookPlugins.Start();
        }

        public static void UnMouseRegister()
        {
            _mouseHookPlugins.Stop();
        }
        #endregion

        #region 注册Ocr插件
        /// <summary>
        /// 注册截屏插件
        /// </summary>
        /// <param name="timeHandler"></param>
        public static void OrcRegister(OcrTimerHandler timeHandler, ScreenConfig config=null)
        {
            _screenTimerPlugins = new ScreenTimerPlugins(config);
            _screenTimerPlugins.OnTimerHandler += timeHandler;

            _screenTimerPlugins.Start();
        }

        /// <summary>
        /// 获取Ocr 内容
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static decimal GetOcrMoney(ScreenConfig config,out Bitmap bitmap)
        {
            if (_screenTimerPlugins == null)
                _screenTimerPlugins = new ScreenTimerPlugins(config);

            _screenTimerPlugins.InitConifg(config);
            return _screenTimerPlugins.GetOcrMoney(config,out bitmap);
        }
        #endregion
    }
}
