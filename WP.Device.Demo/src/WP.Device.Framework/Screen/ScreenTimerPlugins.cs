using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Tesseract;

namespace WP.Device.Framework.Screen
{
    /// <summary>
    /// 截屏 Ocr识别定时器插件
    /// </summary>
    public class ScreenTimerPlugins
    {
        #region 属性定义
        private Timer aTimer;

        private int interval = 500;

        /// <summary>
        /// 坐标信息配置
        /// </summary>
        private ScreenConfig _config;

        private readonly object loker = new object();

        public OcrTimerHandler OnTimerHandler;
        #endregion

        #region 委托定义
        public delegate void OcrTimerHandler(decimal data);
        #endregion

        #region 公共方法
        public ScreenTimerPlugins(ScreenConfig config)
        {
            if (config == null)
                config = new ScreenConfig();

            config.OCRResourcePath = config?.OCRResourcePath ?? AppDomain.CurrentDomain.BaseDirectory + "resource\\tessdata";
            if (string.IsNullOrEmpty(config?.OCRResourcePath))
                throw new ArgumentNullException("OCR 语言库资源地址未配置");

            _config = config;
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        /// <param name="config"></param>
        public void InitConifg(ScreenConfig config)
        {
            if (string.IsNullOrEmpty(config?.OCRResourcePath))
                config.OCRResourcePath = _config.OCRResourcePath;

            _config = config;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            // 已经初始化过，就直接返回
            if (!SetTimer())
                return;

            aTimer.Start();
        }
        #endregion

        #region 定时方法

        /// <summary>
        /// 设置定时器
        /// </summary>
        private bool SetTimer()
        {
            if (aTimer != null)
                return false;
            lock (loker)
            {
                if (aTimer != null)
                    return false;
                aTimer = new Timer
                {
                    Interval = interval
                };
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.AutoReset = true;//每到指定时间Elapsed事件是到时间就触发
                aTimer.Enabled = true; //指示 Timer 是否应引发 Elapsed 事件。
                return true;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                var content = GetOcrMoney(_config, out Bitmap bitMap);

                #region 回调
                OnTimerHandler(content);
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public decimal GetOcrMoney(ScreenConfig config, out Bitmap bitMap)
        {
            bitMap = ScreenPlugins.GetBitmapFromPoint(config);
            if (bitMap == null)
                return 0M;

            #region OCR文字识别
            var ocr = new TesseractEngine(_config.OCRResourcePath, "chi_sim", EngineMode.Default);
            var page = ocr.Process(bitMap);
            #endregion

            return ResolveMoney(page?.GetText() ?? "");
        }

        /// <summary>
        /// 解析出金额
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private decimal ResolveMoney(string data)
        {
            data = Regex.Replace(data, @"[^\d.\d]", "");
            // 如果是数字，则转换为decimal类型
            if (Regex.IsMatch(data, @"^[+-]?\d*[.]?\d*$"))
            {
                var result = 0M;
                decimal.TryParse(data,out result);
                return result;
            }
            return 0.0M;
        }
        #endregion
    }
}
