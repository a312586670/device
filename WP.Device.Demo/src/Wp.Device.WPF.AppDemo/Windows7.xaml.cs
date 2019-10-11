using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wp.Device.WPF.AppDemo.demo;
using WP.Device.Framework;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// Window7.xaml 的交互逻辑
    /// </summary>
    public partial class Window7 : Window
    {
        private BitmapImage GlobalBitmapImage;
        private System.Drawing.Bitmap GlobalBitmap;
        private bool DownFlag = false;
        private Point PreviousPoint;
        private Border globalBorder;
        private Border capture;

        MainWindow mainWindow;
        public Window7(BitmapImage bitmapImage, System.Drawing.Bitmap bitMap, MainWindow mainWindow)
        {
            InitializeComponent();
            GlobalBitmap = bitMap;
            GlobalBitmapImage = bitmapImage;
            mainGrid.Background = new ImageBrush() { ImageSource = GlobalBitmapImage };

            globalBorder = new Border() { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };

            capture = new Border() { BorderBrush = Brushes.Red, Opacity = 0.2, Background = Brushes.Red, BorderThickness = new Thickness(1), Width = 200, Height = 100 };
            mainGrid.Children.Add(globalBorder);
            mainGrid.Children.Add(capture);

            this.mainWindow = mainWindow;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownFlag = true;
            PreviousPoint = e.GetPosition(mainGrid);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (DownFlag == true)
            {
                Point currentPoint = e.GetPosition(mainGrid);
                double left = Math.Min(PreviousPoint.X, currentPoint.X);
                double top = Math.Min(PreviousPoint.Y, currentPoint.Y);
                double width = Math.Abs(PreviousPoint.X - currentPoint.X);
                double height = Math.Abs(PreviousPoint.Y - currentPoint.Y);

                capture.Margin = new Thickness(left, top, 0, 0);
                capture.Width = width;
                capture.Height = height;
                capture.HorizontalAlignment = HorizontalAlignment.Left;
                capture.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            DownFlag = false;
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DownFlag = false;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Drawing.Bitmap bitmap;
            if (DownFlag == true && capture != null)
            {
                int left = Convert.ToInt32(capture.Margin.Left);
                int top = Convert.ToInt32(capture.Margin.Top);
                int width = Convert.ToInt32(capture.Width);
                int height = Convert.ToInt32(capture.Height);
                double r_1 = left + width;
                double h_1 = top + height;

                var leftTopSize = new System.Drawing.Size(left, top);
                var leftBottomSize = new System.Drawing.Size(left, top + height);

                var rightTopSize = new System.Drawing.Size(left + width, top);
                var rightBottomSize = new System.Drawing.Size(left + width, top + height);

                //坐标相关配置
                var config = new WP.Device.Framework.Screen.ScreenConfig()
                {
                    LeftBottomCoordinate = leftBottomSize,
                    LeftTopCoordinate = leftTopSize,
                    RightBottomCoordinate = rightBottomSize,
                    RightTopCoordinate = rightTopSize,
                };

                //Ocr 获取金额
                var money = DeviceGlobalManage.GetOcrMoney(config, out bitmap);
                this.mainWindow.txbMoneyScreen.Text = money.ToString();


                using (System.Drawing.Bitmap map = (System.Drawing.Bitmap)bitmap)
                {

                    BitmapSource source = ScreenPlugins.GetBitMapSourceFromBitmap(map);
                    Clipboard.SetImage(source);

                    bitmap.Dispose();
                    GlobalBitmap.Dispose();
                    this.Close();
                }
            }
        }
    }
}
