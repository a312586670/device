using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityLibrary;
using WP.Device.Framework;
using WP.Device.Framework.Helper;

namespace Wp.Device.Console
{
    class Program
    {

        public static void GetList()
        {
            var list = new List<IntPtr>();
            IntPtr desktopPtr = BaseWin32Api.GetDesktopWindow();
            //2、获得一个子窗口（这通常是一个顶层窗口，当前活动的窗口）
            IntPtr winPtr = BaseWin32Api.GetWindow(desktopPtr, BaseWin32Api.GetWindowCmd.GW_CHILD);

            //3、循环取得桌面下的所有子窗口
            while (winPtr != IntPtr.Zero)
            {
                //4、继续获取下一个子窗口
                winPtr = BaseWin32Api.GetWindow(winPtr, BaseWin32Api.GetWindowCmd.GW_HWNDNEXT);

                var tool = Utility.GetAutomationElementFromHandle(winPtr);
                if (tool != null)
                {
                    TextHelper.Write("=========="+tool.Current.Name + "-" + tool.Current.LocalizedControlType);
                    System.Console.WriteLine(tool.Current.Name+"-"+tool.Current.LocalizedControlType);
                }

                var uiElement = Utility.FindAutoElementByPath(winPtr,new string[] { "2019108.00"});
                if (uiElement != null)
                {
                    System.Console.WriteLine("找到金额"+uiElement.Current.Name);
                    TextHelper.Write("=====找到金额=====" + uiElement.Current.Name);
                }
                //list.Add(winPtr);
            }

            System.Console.WriteLine("便利结束");
        }

        static void Main(string[] args)
        {
            //根据当前鼠标位置获得窗口句柄
            //var maindHwnd = BaseWin32Api.WindowFromPoint();
            System.Console.WriteLine("请输入信息");

            System.Console.ReadKey();

            GetList();

            //var dask = BaseWin32Api.GetDesktopWindow();
            //var maindHwnd = BaseWin32Api.GetWindow(dask, BaseWin32Api.GetWindowCmd.GW_OWNER);


            //if (maindHwnd == null)
            //{
            //    System.Console.WriteLine("没有找到句柄");
            //}

            ////获得当前窗口句柄的UI元素
            ////var rootHwnd = Utility.GetAutomationElementFromHandle(maindHwnd);



            //var text = "";

            //#region 查找具体的金额
            ////var tool = Utility.FindAutoElementByPath(maindHwnd);
            ////if (maindHwnd != null && tool == null)
            ////{
            ////    System.Console.WriteLine("没有找到句柄 金额元素 money:{this.txbMoney.Text}");
            ////}
            //#endregion

            //#region 查找金额
            //var tools = Utility.FindAutoElementListByPath(maindHwnd, null);

            //if (tools != null)
            //{
            //    foreach (var item in tools)
            //    {
            //        text += $"{item.Current.ClassName}    ";
            //    }
            //}

            //System.Console.WriteLine($"总共找到{(tools?.Count ?? 0)}处金额 {text}");
            //#endregion


            //IntPtr maindHwnd = BaseWin32Api.FindWindow(null, "QQ"); //获得QQ登陆框的句柄  
            //if (maindHwnd != IntPtr.Zero)
            //{
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
            //}
            //else
            //{
            //    System.Console.WriteLine("没有找到窗口");
            //}
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
        }
    }
}
