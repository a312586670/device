using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
using Wp.Device.WPF.AppDemo.demo;
using WP.Device.Framework;
using WP.Device.Framework.Event;
using WP.Device.Framework.Helper;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void UpdateValueMethod(decimal data,Bitmap bit);

        public MainWindow()
        {
            InitializeComponent();

            #region 注册插件Demo

            #region 注册全局键盘钩子
            DeviceGlobalManage.Register((data) =>
            {
                if (data.IsValid)
                {
                    this.txbCode.Text = data.Code;
                }
            });
            #endregion

            #region 注册OCR 图片文字识别插件
            DeviceGlobalManage.OrcRegister((data,bit) =>
            {
                UpdateValueMethod myDelegate = new UpdateValueMethod(UpdateValue);
                this.Dispatcher.BeginInvoke(myDelegate, data,bit);
            });
            #endregion

            #endregion
        }

        /// <summary>
        /// 委托事件获取当前时间
        /// </summary>
        /// <param name="para"></param>
        private void UpdateValue(decimal para,Bitmap bit)
        {
            this.txbCode.Text = para.ToString();

            if(bit==null)
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
    }
}
