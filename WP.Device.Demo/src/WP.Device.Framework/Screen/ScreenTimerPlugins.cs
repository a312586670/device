using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Tesseract;
using WP.Device.Framework.Helper;

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
        public delegate void OcrTimerHandler(decimal data, Bitmap bitmap);
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
                OnTimerHandler(content, bitMap);
                #endregion
            }
            catch (Exception ex)
            {
                TextHelper.Write(ex.Message,ex);
                throw new Exception(ex.Message);
            }
        }

        public decimal GetOcrMoney(ScreenConfig config, out Bitmap bitMap)
        {
            bitMap = ScreenPlugins.GetBitmapFromPoint(config);
            if (bitMap == null)
                return 0M;

            #region 图片处理 方便OCR解析更加准确
            var bitMapOce = bitMap;

            bitMapOce = GetZoom(bitMap, 3.5);
            //灰度化
            ToGrey(bitMapOce);
            //二值化
            Thresholding(bitMapOce);
            #endregion

            #region OCR文字识别
            var ocr = new TesseractEngine(_config.OCRResourcePath, "chi_sim", EngineMode.Default);
            var page = ocr.Process(bitMapOce, PageSegMode.RawLine);
            #endregion

            #region OCR解析处理
            var moneyStr = page?.GetText();
            foreach (var item in ConstDefintion.MONEY_REPLACE)
            {
                moneyStr = moneyStr.Replace(item.Key, item.Value);
            }
            #endregion

            TextHelper.Write("data->" + page?.GetText() + " 转换后->" + moneyStr);
            return ResolveMoney(moneyStr);
        }

        /// <summary>
        /// 解析出金额
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private decimal ResolveMoney(string data)
        {
            Match m = Regex.Match(data, "\\d+(\\.\\d+){0,1}");
            var result = 0M;
            decimal.TryParse(m.Groups[0].ToString(), out result);
            return result;
        }
        #endregion

        #region 图片处理相关

        /// <summary>
        /// 获取缩小后的图片
        /// </summary>
        /// <param name="bm">要缩小的图片</param>
        /// <param name="times">要缩小的倍数</param>
        /// <returns></returns>
        private Bitmap GetZoom(Bitmap bm, double times)
        {
            int nowWidth = (int)(bm.Width * times);
            int nowHeight = (int)(bm.Height * times);
            Bitmap newbm = new Bitmap(nowWidth, nowHeight);//新建一个放大后大小的图片

            if (times >4)
            {
                return bm;

            }
            using (Graphics g = Graphics.FromImage(newbm))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(bm, new Rectangle(0, 0, nowWidth, nowHeight), new Rectangle(0, 0, bm.Width, bm.Height), GraphicsUnit.Pixel);
            }
            return newbm;
        }

        private void ToGrey(Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    //计算灰度值
                    int grey = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                    Color newColor = Color.FromArgb(grey, grey, grey);
                    bitmap.SetPixel(i, j, newColor);
                }
            }
        }

        private void Thresholding(Bitmap bitmap)
        {
            try
            {
                int[] histogram = new int[256];
                int minGrayValue = 255, maxGrayValue = 0;
                //求取直方图
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Color pixelColor = bitmap.GetPixel(i, j);
                        histogram[pixelColor.R]++;
                        if (pixelColor.R > maxGrayValue) maxGrayValue = pixelColor.R;
                        if (pixelColor.R < minGrayValue) minGrayValue = pixelColor.R;
                    }
                }
                //迭代计算阀值
                int threshold = -1;
                int newThreshold = (minGrayValue + maxGrayValue) / 2;
                for (int iterationTimes = 0; threshold != newThreshold && iterationTimes < 100; iterationTimes++)
                {
                    threshold = newThreshold;
                    int lP1 = 0;
                    int lP2 = 0;
                    int lS1 = 0;
                    int lS2 = 0;
                    //求两个区域的灰度的平均值
                    for (int i = minGrayValue; i < threshold; i++)
                    {
                        lP1 += histogram[i] * i;
                        lS1 += histogram[i];
                    }
                    int mean1GrayValue = (lP1 / lS1);
                    for (int i = threshold + 1; i < maxGrayValue; i++)
                    {
                        lP2 += histogram[i] * i;
                        lS2 += histogram[i];
                    }
                    int mean2GrayValue = (lP2 / lS2);
                    newThreshold = (mean1GrayValue + mean2GrayValue) / 2;
                }
                //计算二值化
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Color pixelColor = bitmap.GetPixel(i, j);
                        if (pixelColor.R > threshold) bitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                        else bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
