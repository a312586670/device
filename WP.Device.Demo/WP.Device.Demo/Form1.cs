using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WP.Device.Demo
{
    public partial class Form1 : Form
    {
        KeyboardHookHelper hookHelper;


        public Form1()
        {
            InitializeComponent();

            hookHelper = new KeyboardHookHelper();
            hookHelper.KeyPressEvent += HookKeyPressEvent;
            hookHelper.Start();//安装键盘钩子
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {

            //StringBuilder sbDevHst = new StringBuilder();
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
            //foreach (ManagementObject mgt in searcher.Get())
            //{
            //    sbDevHst.AppendLine(Convert.ToString(mgt["Name"]));
            //    sbDevHst.AppendLine("");
            //}
            //return sbDevHst.ToString();//获取的字符串 
            //SerialPort mySerialPort = new SerialPort("COM1");

            //mySerialPort.BaudRate = 9600;
            //mySerialPort.Parity = Parity.None;
            //mySerialPort.StopBits = StopBits.One;
            //mySerialPort.DataBits = 8;
            //mySerialPort.Handshake = Handshake.None;

            //mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            //mySerialPort.Open();

            //Console.WriteLine("Press any key to continue...");
            //Console.WriteLine();
            ////mySerialPort.Close();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //SerialPort sp = (SerialPort)sender;
            //string indata = sp.ReadExisting();
            //this.txb_Code.Text = indata;
        }

        #region 钩子
        private void HookKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            this.txb_Code.Text += e.KeyChar;

            int i = (int)e.KeyChar;
            //System.Windows.Forms.MessageBox.Show(i.ToString());
        }

        private void HookKeyDown(object sender, KeyEventArgs e)
        {
            this.txb_Code.Text += (char)e.KeyData;


            //判断按下的键（Alt + A） 
            //if (e.KeyValue == (int)Keys.A && (int)System.Windows.Forms.Control.ModifierKeys == (int)Keys.Alt)
            //{
            //    System.Windows.Forms.MessageBox.Show("ddd");
            //}
        }

        //private void Window_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    hookHelper.Stop();
        //}
        #endregion
    }
}
