using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tesseract;
using UtilityLibrary;
using Wp.Device.WPF.AppDemo.demo;
using WP.Device.Framework;
using WP.Device.Framework.Base;
using WP.Device.Framework.Event;
using WP.Device.Framework.Helper;
using WP.Device.Framework.Screen;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void UpdateValueMethod(decimal data, Bitmap bit);
        private delegate void UpdateDescription(int data);


        public MainWindow()
        {
            InitializeComponent();

            #region 注册插件Demo

            #region 注册全局键盘钩子
            DeviceGlobalManage.Register((data) =>
            {
                if (data.IsValid)
                {
                    this.txbPayCode.Text = data.Code;
                }
            });
            #endregion

            #region 注册OCR 图片文字识别插件
            //注册图片查找钩子
            DeviceGlobalManage.OrcRegister((data, bit) =>
            {
                UpdateValueMethod myDelegate = new UpdateValueMethod(UpdateValue);
                this.Dispatcher.BeginInvoke(myDelegate, data, bit);
            });
            #endregion

            #region 注册全局鼠标钩子
            //DeviceGlobalManage.MouseRegister(MouseMoveEventHandler, MouseDoubleEvent);
            #endregion

            #endregion
        }

        /// <summary>
        /// 委托事件获取当前时间
        /// </summary>
        /// <param name="para"></param>
        private void UpdateValue(decimal para, Bitmap bit)
        {
            this.txbMoneyScreen.Text = para.ToString();

            if (bit == null)
            {
                return;
            }
            BitmapSource source = ScreenPlugins.GetBitMapSourceFromBitmap(bit);
            //System.Windows.Clipboard.SetImage(source);

            //ImageSource img = System.Windows.Clipboard.GetImage();
            if (source != null)
            {
                this.ImagePannel.Width = source.Width;
                this.ImagePannel.Height = source.Height;
                this.ImagePannel.Source = source;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.txbCode.Text = GetText();
        }

        public string GetText()
        {
            try
            {
                var img = new Bitmap(this.txtFile.Text);    //需要识别的图片
                var ocr = new TesseractEngine(@"D:\demo\device\WP.Device.Demo\src\Wp.Device.WPF.AppDemo\resource\tessdata", "chi_sim", EngineMode.Default);    //使用chi_sim中文语言包做测试

                var page = ocr.Process(img);
                Console.Write(page.GetText());
                return page.GetText();
            }
            catch (Exception ex)
            { }
            return "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            Bitmap bitMap = ScreenPlugins.GetScreenSnapshot();
            BitmapImage bitmapImage = ScreenPlugins.BitmapToBitmapImage(bitMap);

            Window7 win7 = new Window7(bitmapImage, bitMap, this);
            win7.ShowDialog();

            //image1.Source 
            ImageSource img = System.Windows.Clipboard.GetImage();
            this.ImagePannel.Width = img.Width;
            this.ImagePannel.Height = img.Height;
            this.ImagePannel.Source = img;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SerialWindows win7 = new SerialWindows();
            win7.ShowDialog();
        }

        private IntPtr maindHwnd;
        private void Button_Click_4(object sender, RoutedEventArgs ea)
        {
            #region 注册全局鼠标钩子
            DeviceGlobalManage.MouseRegister(MouseMoveEventHandler, MouseDoubleEvent);
            #endregion
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var test = this.txbMoney.Text;
            Task.Factory.StartNew(() =>
            {
                var list = Utility.FindGlobalElementByName(test);

                var count = list?.Count ?? 0;
                UpdateDescription updateDescritionAction = new UpdateDescription(UpdateDescriptionMethod);
                this.Dispatcher.BeginInvoke(updateDescritionAction, count);
            });
        }

        public void UpdateDescriptionMethod(int data)
        {
            this.txbDescription.Text = $"定位到{data}个窗口";
        }

        #region 鼠标相关
        public void MouseMoveEventHandler(object sender, System.Windows.Forms.MouseEventArgs data)
        {
            this.txbCode.Text = $"x:{data.X},y:{data.Y},clickCount:{data.Clicks},button:{data.Button.ToString()}";

            #region 查找坐标句柄并且跟上次对比
            //根据当前鼠标位置获得窗口句柄
            var currentHwnd = BaseWin32Api.WindowFromPoint();
            if (maindHwnd == currentHwnd)
            {
                return;
            }

            maindHwnd = currentHwnd;
            if (currentHwnd == null)
            {
                return;
            }
            #endregion

            #region 查找句柄的当前元素
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //获得当前窗口句柄的UI元素
            var rootHwnd = Utility.GetAutomationElementFromHandle(currentHwnd);
            stopwatch.Stop();
            this.txbCode.Text += $"查找跟UI 花费时间:{stopwatch.ElapsedMilliseconds}毫秒";
            if (rootHwnd != null)
            {
                if (rootHwnd.Current.Name.Equals(this.txbMoney.Text))
                {
                    stopwatch.Start();
                    GDIWin32Api.DrawRectangle(currentHwnd, Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Width), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Height), 2, System.Drawing.Color.Red);
                    stopwatch.Stop();
                    this.txbCode.Text += $"绘图 花费时间:{stopwatch.ElapsedMilliseconds}毫秒";
                    this.txbDescription.Text = $"总共找到{1}处金额,{rootHwnd.Current.Name}";
                }
            }
            #endregion

            TextHelper.WriteAsync(this.txbCode.Text);
        }

        public void MouseDoubleEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //DeviceGlobalManage.UnMouseRegister();

            //#region 查找当前鼠标的区域
            //var currentHwnd = BaseWin32Api.WindowFromPoint();
           
            //#endregion

            //#region 查找当前鼠标区域句柄的UI元素
            //var rootHwnd = Utility.GetAutomationElementFromHandle(currentHwnd);
            //#endregion

            //var leftTop = new System.Drawing.Size(Convert.ToInt32(rootHwnd.Current.BoundingRectangle.TopLeft.X), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.TopLeft.Y));
            //var leftBottom = new System.Drawing.Size(Convert.ToInt32(rootHwnd.Current.BoundingRectangle.BottomLeft.X), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.BottomLeft.Y));

            //var rightTop = new System.Drawing.Size(Convert.ToInt32(rootHwnd.Current.BoundingRectangle.TopRight.X), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.TopRight.Y));
            //var rightBottom = new System.Drawing.Size(Convert.ToInt32(rootHwnd.Current.BoundingRectangle.BottomRight.X), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.BottomRight.Y));

            //var screenConfig = new ScreenConfig()
            //{
            //    LeftTopCoordinate = leftTop,
            //    LeftBottomCoordinate = leftBottom,
            //    RightBottomCoordinate = rightBottom,
            //    RightTopCoordinate = rightTop,
            //};

            //#region 注册OCR 图片文字识别插件
            ////注册图片查找钩子
            //DeviceGlobalManage.OrcRegister((data, bit) =>
            //{
            //    UpdateValueMethod myDelegate = new UpdateValueMethod(UpdateFindFormValue);
            //    this.Dispatcher.BeginInvoke(myDelegate, data, bit);
            //}, screenConfig);
            //#endregion
            System.Windows.MessageBox.Show("点击");
        }

        /// <summary>
        /// 委托事件获取
        /// </summary>
        /// <param name="para"></param>
        private void UpdateFindFormValue(decimal para, Bitmap bit)
        {
            this.txbMoneyForm.Text = para.ToString();

            if (bit == null)
            {
                return;
            }
            BitmapSource source = ScreenPlugins.GetBitMapSourceFromBitmap(bit);
            System.Windows.Clipboard.SetImage(source);

            ImageSource img = System.Windows.Clipboard.GetImage();
            if (img != null)
            {
                this.ImagePannel2.Width = img.Width;
                this.ImagePannel2.Height = img.Height;
                this.ImagePannel2.Source = img;
            }
        }
        #endregion
    }
}
