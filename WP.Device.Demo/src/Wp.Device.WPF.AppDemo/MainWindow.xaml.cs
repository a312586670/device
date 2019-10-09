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
            //DeviceGlobalManage.Register((data) =>
            //{
            //    if (data.IsValid)
            //    {
            //        this.txbCode.Text = data.Code;
            //    }
            //});
            #endregion

            #region 注册OCR 图片文字识别插件
            //DeviceGlobalManage.OrcRegister((data, bit) =>
            //{
            //    UpdateValueMethod myDelegate = new UpdateValueMethod(UpdateValue);
            //    this.Dispatcher.BeginInvoke(myDelegate, data, bit);
            //});
            #endregion

            //#region 注册全局鼠标钩子
            //DeviceGlobalManage.MouseRegister((e, data) =>
            //{
            //    TextHelper.Write($"x:{data.Location.X},y:{data.Location.X}");
            //    this.txbCode.Text = $"x:{data.Location.X},y:{data.Location.X}";

            //    //根据当前鼠标位置获得窗口句柄
            //    var maindHwnd = BaseWin32Api.WindowFromPoint();

            //    //获得当前窗口句柄的UI元素
            //    var rootHwnd = Utility.GetAutomationElementFromHandle(maindHwnd);

            //    var text = "";

            //    #region 查找具体的金额
            //    //var tool = Utility.GetAutoElementByPath(maindHwnd, new string[] { "9.05" });
            //    //if (tool == null)
            //    //{
            //    //    text = "没有找到子窗口[test]";
            //    //}
            //    //else
            //    //{
            //    //    text = tool.Current.Name+$"坐标:x->{tool.Current.BoundingRectangle.X},y->{tool.Current.BoundingRectangle.Y},width:{tool.Current.BoundingRectangle.Width},height:{tool.Current.BoundingRectangle.Height}";
            //    //}
            //    #endregion

            //    #region 查找一系列
            //    var tools = Utility.FindAutoElementListByPath(maindHwnd,null);
            //    foreach (var item in tools)
            //    {
            //        if (item == null)
            //        {
            //            text = "没有找到子窗口[test]";
            //        }
            //        else
            //        {
            //            text += item.Current.Name + $"坐标:x->{item.Current.BoundingRectangle.X},y->{item.Current.BoundingRectangle.Y},width:{item.Current.BoundingRectangle.Width},height:{item.Current.BoundingRectangle.Height}";
            //        }
            //    }
            //    #endregion

            //    GDIWin32Api.DrawRectangle(maindHwnd, Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Width), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Height), 2, System.Drawing.Color.Thistle);
            //    ////获得句柄的Dc
            //    //var hdc = BaseWin32Api.GetWindowDC(maindHwnd);
            //    //using (Graphics gr = Graphics.FromHdc(hdc))
            //    //{
            //    //    System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Tomato, 2);//实例化Pen类

            //    //    //调用Graphics对象的DrawRectangle方法
            //    //    gr.DrawRectangle(myPen, 1, 1, Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Width-2), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Height-2));
            //    //}
            //    this.txbCode.Text = text;
            //});
            //#endregion

            #endregion
        }

        /// <summary>
        /// 委托事件获取当前时间
        /// </summary>
        /// <param name="para"></param>
        private void UpdateValue(decimal para, Bitmap bit)
        {
            this.txbCode.Text = para.ToString();

            if (bit == null)
            {
                return;
            }
            BitmapSource source = ScreenPlugins.GetBitMapSourceFromBitmap(bit);
            System.Windows.Clipboard.SetImage(source);

            ImageSource img = System.Windows.Clipboard.GetImage();
            image1.Width = img.Width;
            image1.Height = img.Height;
            image1.Source = img;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.txbCode.Text = GetText();
        }

        public string GetText()
        {
            var img = new Bitmap(this.txtFile.Text);    //需要识别的图片
            var ocr = new TesseractEngine(@"D:\demo\WP.Device.Demo\src\Wp.Device.WPF.AppDemo\tessdata", "chi_sim", EngineMode.Default);    //使用chi_sim中文语言包做测试

            var page = ocr.Process(img);
            Console.Write(page.GetText());
            return page.GetText();
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
            image1.Width = img.Width;
            image1.Height = img.Height;
            image1.Source = img;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //#region test
            //var maindHwnd = BaseWin32Api.WindowFromPoint();

            //var text = "";
            //var tool = Utility.GetAutoElementByPath(maindHwnd, new string[] { "9.05" });
            //if (tool == null)
            //{
            //    text = "没有找到子窗口[test]";
            //}
            //else
            //{
            //    text = tool.Current.Name;
            //}
            //#endregion

            //this.txbCode.Text = text;
            //TextHelper.Write(text);
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
            DeviceGlobalManage.MouseRegister((e, data) =>
            {

                this.txbCode.Text = $"x:{data.X},y:{data.Y}";

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
                    //this.txbCode.Text += $"\n 没有找到句柄";
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

                //#region 查找句柄的子元素
                //stopwatch.Start();
                //var tool = Utility.FindAutoElementByPath(maindHwnd, new string[] { this.txbMoney.Text });
                //stopwatch.Stop();
                //this.txbCode.Text += $"\n 查找跟子元素UI 花费时间:{stopwatch.ElapsedMilliseconds}毫秒";

                //if (tool != null)
                //{
                //    this.txbDescription.Text= $"总共找到{1}处金额,{tool.Current.Name}";
                //}
                //#endregion
                //if(data.Clicks==1)
                //{
                //stopwatch.Start();
                //GDIWin32Api.DrawRectangle(currentHwnd, Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Width), Convert.ToInt32(rootHwnd.Current.BoundingRectangle.Height), 2, System.Drawing.Color.Red);
                //stopwatch.Stop();
                //this.txbCode.Text += $"绘图 花费时间:{stopwatch.ElapsedMilliseconds}毫秒";
                //}
                TextHelper.WriteAsync(this.txbCode.Text);
            });
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
    }
}
