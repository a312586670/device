using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLibrary;
using WP.Device.Framework;
namespace Wp.Device.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            const int BM_CLICK = 0xF5;
            IntPtr maindHwnd = BaseWin32Api.FindWindow(null, "QQ"); //获得QQ登陆框的句柄  
            if (maindHwnd != IntPtr.Zero)
            {
                //var main = Utility.GetAutomationElementFromHandle(maindHwnd);
                //if (main == null)
                //{
                //    System.Console.WriteLine("没有找到子窗口[maindHwnd]");
                //}

                //var textElement = Utility.GetTextElement(main);

                //if (textElement == null)
                //{
                //    System.Console.WriteLine($"没有找到text元素->{textElement}");
                //}
                //else
                //{
                //    var text = Utility.GetFontNameAttribute(textElement);
                //    System.Console.WriteLine($"text元素->{text}");
                //}

                //var tool = Utility.GetAutoElementByPath(main, new string[] { "312586670", "登录" });
                //if (tool == null)
                //{
                //    System.Console.WriteLine("没有找到子窗口[test]");
                //    //StatusMsg = "打开好友管理器失败，找不到工具菜单项";
                //    //return ErrType.OperationFailed;
                //}

                //IntPtr childHwnd = BaseWin32Api.FindWindowEx(maindHwnd, IntPtr.Zero, null, "登录");   //获得按钮的句柄  
                //if (childHwnd != IntPtr.Zero)
                //{
                //    System.Console.WriteLine("找到子窗口" + childHwnd);
                //    BaseWin32Api.SendMessage(childHwnd, BM_CLICK, IntPtr.Zero, null);     //发送点击按钮的消息  
                //}
                //else
                //{
                //    System.Console.WriteLine("没有找到子窗口");
                //}
            }
            else
            {
                System.Console.WriteLine("没有找到窗口");
            }
            System.Console.ReadKey();
            //const string language = "eng";
            //string imageFile = args[0];

            //TesseractProcessor processor = new TesseractProcessor();

            //using (var bmp = Bitmap.FromFile(imageFile) as Bitmap)
            //{
            //    var success = processor.Init(TessractData, language, (int)eOcrEngineMode.OEM_DEFAULT);
            //    if (!success)
            //    {
            //        Console.WriteLine("Failed to initialize tesseract.");
            //    }
            //    else
            //    {
            //        string text = processor.Recognize(bmp);
            //        Console.WriteLine("Text:");
            //        Console.WriteLine("*****************************");
            //        Console.WriteLine(text);
            //        Console.WriteLine("*****************************");
            //    }
            //}

            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
        }
    }
}
