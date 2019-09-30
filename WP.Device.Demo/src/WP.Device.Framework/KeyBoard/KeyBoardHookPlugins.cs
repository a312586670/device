using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WP.Device.Framework.Event;
using WP.Device.Framework.Helper;
using static WP.Device.Framework.KeyBoardHookPlugins;

namespace WP.Device.Framework
{
    #region 钩子相关实现方法
    /// <summary>
    /// 全局钩子插件方法
    /// </summary>
    public class KeyBoardHookPlugins
    {
        #region 定义委托
        public delegate void BoardDataEventHandler(KeyboardHookModel data);
        #endregion

        #region 定义事件
        public event BoardDataEventHandler OnDataEvent;
        #endregion

        #region 变量声明
        /// <summary>
        /// 声明KeyboardHookProcedure作为HookProc类型
        /// </summary>
        BaseWin32Api.HookProcess KeyboardHookProcedure;

        public KeyboardHookModel data;

        string keyCodeStr;
        static int hKeyboardHook = 0; //声明键盘钩子处理的初始值
        #endregion

        #region 公共方法
        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        public void Start()
        {
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new BaseWin32Api.HookProcess(KeyboardHookProcess);
                ProcessModule cModule = Process.GetCurrentProcess().MainModule;
                var mhIntPrt = BaseWin32Api.GetModuleHandle(cModule.ModuleName);

                hKeyboardHook = BaseWin32Api.SetWindowsHookEx(ConstDefintion.WH_KEYBOARD_LL, KeyboardHookProcedure, mhIntPrt, 0);
                data = new KeyboardHookModel();
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
            }

        }

        /// <summary>
        /// 停止钩子
        /// </summary>
        public void Stop()
        {
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = BaseWin32Api.UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }

            if (!(retKeyboard))
                throw new Exception("卸载钩子失败！");
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 获取键盘消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyboardHookProcess(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var message = (BaseWin32Api.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(BaseWin32Api.KeyboardHookStruct));
                if (wParam == ConstDefintion.WM_KEYUP)
                {
                    #region 数据处理
                    data.VirtKey = message.vkCode & 0xff;//虚拟吗
                    data.ScanCode = message.scanCode & 0xff;//扫描码
                    StringBuilder keyName = new StringBuilder(225);
                    if (BaseWin32Api.GetKeyNameText(data.ScanCode * 65536, keyName, 255) > 0)
                    {
                        data.KeyName = keyName.ToString().Trim(new char[] { ' ', '\0' });
                    }
                    else
                    {
                        data.KeyName = "";
                    }
                    byte[] kbArray = new byte[256];
                    uint uKey = 0;
                    BaseWin32Api.GetKeyboardState(kbArray);

                    if (BaseWin32Api.ToAscii(data.VirtKey, data.ScanCode, kbArray, ref uKey, 0))
                    {
                        data.Ascll = uKey;
                        data.Chat = Convert.ToChar(uKey);
                    }

                    TimeSpan ts = DateTime.Now.Subtract(data.Time);
                    #endregion

                    #region 手动输入
                    //时间戳，大于50 毫秒表示手动输入
                    if (ts.TotalMilliseconds > 50)
                    {
                        keyCodeStr = data.Chat.ToString();
                        data.Code = keyCodeStr;
                        data.IsValid = false;
                        TextHelper.Write($"json -01 ->{JsonConvert.SerializeObject(data)}");
                    }
                    #endregion

                    #region 扫码枪或者刷脸设备键盘
                    else
                    {
                        if (keyCodeStr.Length > 5)
                        {
                            data.IsValid = true;
                        }
                        keyCodeStr += data.Chat.ToString();
                        data.Code = keyCodeStr;
                        TextHelper.Write($"json -02 ->{JsonConvert.SerializeObject(data)}");

                    }
                    data.Time = DateTime.Now;
                    #endregion

                    #region 数据传输事件
                    if (OnDataEvent != null && wParam == ConstDefintion.WM_KEYUP)
                    {
                        OnDataEvent(data);
                    }
                    #endregion
                }
            }
            return BaseWin32Api.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        #endregion

        ~KeyBoardHookPlugins()
        {
            Stop();
        }
    }

    /// <summary>
    /// 全局键盘钩子承载信息数据实体
    /// </summary>
    public struct KeyboardHookModel
    {
        /// <summary>
        /// 虚拟码
        /// </summary>
        public int VirtKey;

        /// <summary>
        /// 扫描码
        /// </summary>
        public int ScanCode;

        /// <summary>
        /// 键名
        /// </summary>
        public string KeyName;

        /// <summary>
        /// Ascll
        /// </summary>
        public uint Ascll;

        /// <summary>
        /// 字符
        /// </summary>
        public char Chat;

        /// <summary>
        /// 条码信息保存最终的条码
        /// </summary>
        public string Code;

        /// <summary>
        /// 条码是否有效
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// 扫描时间,
        /// </summary>
        public DateTime Time;
    }
    #endregion

}
