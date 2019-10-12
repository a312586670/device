using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using UtilityLibrary;
using WP.Device.Framework;
using WP.Device.Framework.Helper;

namespace Wp.Device.Console
{
    class Program
    {


        /// <summary>
        /// 查找窗体上控件句柄
        /// </summary>
        /// <param name="hwnd">父窗体句柄</param>
        /// <param name="lpszWindow">控件标题(Text)</param>
        /// <param name="bChild">设定是否在子窗体中查找</param>
        /// <returns>控件句柄，没找到返回IntPtr.Zero</returns>
        private static IntPtr FindWindowEx(IntPtr hwnd, string lpszWindow, bool bChild)
        {
            IntPtr iResult = IntPtr.Zero;
            // 首先在父窗体上查找控件
            iResult = BaseWin32Api.FindWindowEx(hwnd, 0, null, lpszWindow);
            // 如果找到直接返回控件句柄
            if (iResult != IntPtr.Zero) return iResult;

            // 如果设定了不在子窗体中查找

            if (!bChild) return iResult;

            var list = EnumChildWindows(hwnd);

            //var list2 = new List<IntPtr>();
            //foreach (var item in list)
            //{
            //    list2 = EnumChildWindows(item);
            //}
            //list.AddRange(list2);

            // 枚举子窗体，查找控件句柄
            //int i = BaseWin32Api.EnumChildWindows(
            //hwnd,
            //(h, l) =>
            //{
            //    var tool = Utility.GetAutomationElementFromHandle(h);
            //    var uiElement = Utility.FindAutoElementByPath(h, new string[] { lpszWindow });
            //    IntPtr f1 = BaseWin32Api.FindWindowEx(h, 0, null, lpszWindow);
            //    if (f1 == IntPtr.Zero)
            //        return true;
            //    else
            //    {
            //        iResult = f1;
            //        return false;
            //    }
            //},
            //0);
            // 返回查找结果
            return iResult;
        }

        /// <summary>
        /// 便利查找子窗口
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static List<IntPtr> EnumChildWindows(IntPtr hwnd, string names = "", int index = 0)
        {
            var uiElement2 = Utility.FindAutoElementByPath(hwnd, new string[] { names });

            ////枚举子窗体，查找控件句柄
            //int i = BaseWin32Api.EnumChildWindows(
            //hwnd,
            //(h, l) =>
            //{
            //    var tool = Utility.GetAutomationElementFromHandle(h);
            //    var uiElement = Utility.FindAutoElementByPath(h, new string[] { names });
            //    IntPtr f1 = BaseWin32Api.FindWindowEx(h, 0, null, names);

            //    EnumChildWindows(h, names,(index+1));

            //    return true;
            //    //if (f1 == IntPtr.Zero)
            //    //    return true;
            //    //else
            //    //{
            //    //    //iResult = f1;
            //    //    return false;
            //    //}

            //},
            //index);

            return null;
            //var list = new List<IntPtr>();
            //int i = BaseWin32Api.EnumChildWindows(hwnd, (h, l) =>
            //{
            //    IntPtr f1 = BaseWin32Api.FindWindowEx(h, 0, null, names)

            //    var tool = Utility.GetAutomationElementFromHandle(h);
            //    var uiElement = Utility.FindAutoElementByPath(h, new string[] { names });
            //    if (h != IntPtr.Zero)
            //     {
            //         list.Add(h);
            //         return false;
            //     }
            //     else
            //     {
            //         return true;
            //     }
            // }, 0);
            //return list;
        }


        public static void GetList()
        {
            var list = new List<IntPtr>();
            IntPtr winPtr = BaseWin32Api.GetDesktopWindow();

            var list2 = EnumChildWindows(winPtr, "51.50");
            ////2、获得一个子窗口（这通常是一个顶层窗口，当前活动的窗口）
            //IntPtr winPtr = BaseWin32Api.GetWindow(desktopPtr, BaseWin32Api.GetWindowCmd.GW_CHILD);

            //3、循环取得桌面下的所有子窗口
            while (winPtr != IntPtr.Zero)
            {
                //4、继续获取下一个子窗口
                winPtr = BaseWin32Api.GetWindow(winPtr, BaseWin32Api.GetWindowCmd.GW_HWNDNEXT);

                var tool = Utility.GetAutomationElementFromHandle(winPtr);
                if (tool == null)
                    continue;

                if (tool != null)
                {
                    TextHelper.Write("==========" + tool.Current.Name + "-" + tool.Current.LocalizedControlType);
                    System.Console.WriteLine(tool.Current.Name + "-" + tool.Current.LocalizedControlType);
                }

                if (!tool.Current.Name.Contains("思迅天店"))
                {
                    continue;
                }


                var toolElement = FindWindowEx(winPtr, "909.90", true);

                //var uiElement = Utility.FindAutoElementByPath(winPtr, new string[] { "2019108.00" });
                //if (uiElement != null)
                //{
                //    System.Console.WriteLine("找到金额" + uiElement.Current.Name);
                //    TextHelper.Write("=====找到金额=====" + uiElement.Current.Name);
                //}
            }

            System.Console.WriteLine("便利结束");
        }

        /// <summary>
        /// 全局搜索桌面UI元素
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static List<AutomationElement> FindGlobalElementByName(string names)
        {
            var list = new List<AutomationElement>();
            BaseWin32Api.EnumWindows((hwnd, l) =>
            {
                //if ((list?.Count ?? 0) > 0)
                //    return false;
                //var rootUI = Utility.GetAutomationElementFromHandle(hwnd);
                //if ((rootUI?.Current.Name ?? "").Contains("思迅"))
                //{

                //}
                int i = BaseWin32Api.EnumChildWindows(hwnd, (h, l2) =>
                {
                    var uiElement = Utility.FindAutoElementByPath(h, new string[] { names });
                    if (uiElement != null)
                    {
                        System.Console.WriteLine(hwnd.ToString()+"id->"+uiElement.Current.AutomationId);
                        //TextHelper.Write($"json->{JsonConvert.SerializeObject(uiElement.Current)}");
                        list.Add(uiElement);
                        return false;
                    }
                    System.Console.WriteLine("true");
                    return true;
                }, 0);

                return true;
            }, 0);
            return list;
        }
        static void Main(string[] args)
        {
            //根据当前鼠标位置获得窗口句柄
            //var maindHwnd = BaseWin32Api.WindowFromPoint();
            System.Console.WriteLine("请输入信息");

            System.Console.ReadKey();


            var names = "50.00";
            var list2 = FindGlobalElementByName(names);
            System.Console.WriteLine($"找到了{list2?.Count ?? 0}个窗体");
            ////GetList();
            //var list = BaseWin32Api.EnumWindows((hwnd, l) =>
            //{
            //    var rootUI = Utility.GetAutomationElementFromHandle(hwnd);
            //    //System.Console.WriteLine($"name->{rootUI?.Current.Name??""},intpr->{h.ToString()}");
            //    list2.Add(hwnd);

            //    if ((rootUI?.Current.Name ?? "").Contains("思迅"))
            //    {
            //        var uiElement2 = Utility.FindAutoElementByPath(hwnd, new string[] { "909.90" });

            //        int i = BaseWin32Api.EnumChildWindows(
            //        hwnd,
            //        (h, l2) =>
            //        {
            //            var tool = Utility.GetAutomationElementFromHandle(h);

            //            System.Console.WriteLine($"name child->{tool?.Current.Name ?? ""},intpr->{h.ToString()}");

            //            var uiElement = Utility.FindAutoElementByPath(h, new string[] { names });
            //            IntPtr f1 = BaseWin32Api.FindWindowEx(h, 0, null, names);

            //            //EnumChildWindows(h, names, (index + 1));

            //            return true;
            //            //if (f1 == IntPtr.Zero)
            //            //    return true;
            //            //else
            //            //{
            //            //    //iResult = f1;
            //            //    return false;
            //            //}

            //        },
            //        0);
            //    }
            //    //var uiElement2 = Utility.FindAutoElementByPath(h, new string[] { "909.90" });
            //    return true;
            //}, 0);

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
