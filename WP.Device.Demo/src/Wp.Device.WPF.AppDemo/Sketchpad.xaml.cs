using System;
using System.Collections.Generic;
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
using WP.Device.Framework.Helper;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// 画板
    /// </summary>
    public partial class Sketchpad : Window
    {
        public Sketchpad()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var point= e.GetPosition(this);
            Top = point.Y;
            Left = point.X;

            //TextHelper.Write($"test----->x:{Top},left:{Left}");
            //if (e.LeftButton == MouseButtons.Left)
            //{
            //    Top = MousePosition.Y - _mousePoint.Y;
            //    Left = MousePosition.X - _mousePoint.X;
            //} 
            //Point currentPoint = e.GetPosition(this);
            //double left = Math.Min(PreviousPoint.X, currentPoint.X);
            //double top = Math.Min(PreviousPoint.Y, currentPoint.Y);
            //double width = Math.Abs(PreviousPoint.X - currentPoint.X);
            //double height = Math.Abs(PreviousPoint.Y - currentPoint.Y);

            //capture.Margin = new Thickness(left, top, 0, 0);
            //capture.Width = width;
            //capture.Height = height;
            //capture.HorizontalAlignment = HorizontalAlignment.Left;
            //capture.VerticalAlignment = VerticalAlignment.Top;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
