using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Wp.Device.WPF.AppDemo
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //KeyboardListener KListener = new KeyboardListener();

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
        //}

        //void KListener_KeyDown(object sender, RawKeyEventArgs args)
        //{
        //    Console.WriteLine(args.Key.ToString());
        //    // I tried writing the data in file here also, to make sure the problem is not in Console.WriteLine
        //}

        //private void Application_Exit(object sender, ExitEventArgs e)
        //{
        //    KListener.Dispose();
        //}
    }
}
