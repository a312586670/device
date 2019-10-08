using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using WP.Device.Framework.Helper;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// SerialWindows.xaml 的交互逻辑
    /// </summary>
    public partial class SerialWindows : Window
    {
        private delegate void MyDelegate(string indata);

        private MyDelegate showDelegate = null;

        private SerialPort portSend;
        private SerialPort portReceive;

        public SerialWindows()
        {
            InitializeComponent();
            portSend = new SerialPort(this.txbCOMPort1.Text, 8);
            portReceive = new SerialPort(this.txbCOMPort2.Text, 8);

            portReceive.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            showDelegate = new MyDelegate(show);
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.Dispatcher.Invoke(showDelegate, new object[] { sp.ReadExisting() });
        }

        void show(string indata)
        {
            this.txbReceive.AppendText(indata + "\r\n");
        }


        #region 发送数据
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!string.IsNullOrEmpty(this.txbCOMPort1.Text))
                {
                    if (!portSend.IsOpen)
                    {
                        portSend.PortName = this.txbCOMPort1.Text;
                        portSend.Open();
                    }
                    portSend.WriteLine(txbSend.Text);
                }

            }
            catch (Exception ex)
            {
                TextHelper.Write($" 发送异常:{ex.Message}");
            }

            try
            {
                if (!string.IsNullOrEmpty(this.txbCOMPort2.Text))
                {
                    if (!portReceive.IsOpen)
                    {
                        TextHelper.Write($"{this.txbCOMPort2.Text} 端口打开中...");
                        portReceive.PortName = this.txbCOMPort2.Text;
                        portReceive.Open();
                        TextHelper.Write($"{this.txbCOMPort2.Text} 端口打开成功");
                    }
                }
            }
            catch (Exception ex)
            {
                TextHelper.Write($" 打开端口异常:{ex.Message}");
            }
        }
        #endregion

        #region 串口截取数据
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (this.portSend.IsOpen)
                {
                    this.portSend.Close();
                }
            }
            catch (Exception ex)
            {
                TextHelper.Write($"Send 关闭异常:{ex.Message}");
            }

            try
            {
                if (this.portReceive.IsOpen)
                {
                    this.portReceive.Close();
                }
            }
            catch (Exception ex)
            {
                TextHelper.Write($"Receive 关闭异常:{ex.Message}");

            }
        }
    }
}
