using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WP.Device.Framework;
namespace Wp.Device.Console
{
    class Program
    {
        static void Main(string[] args)
        {
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

        public static List<int> Data
        {
            get
            {
                return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            }
        }

        //public static IEnumerable<int> FilterWithoutYield
        //{
        //    get
        //    {
        //        var result = new List<int>();
        //        foreach (var i in Data)
        //        {
        //            if (i > 4)
        //                result.Add(i);
        //        }
        //        return result;
        //    }
        //}

        ////申明属性，过滤器(使用yield)
        //public static IEnumerable<int> FilterWithoutYield2
        //{
        //    get
        //    {
        //        foreach (var i in Data)
        //        {
        //            if (i > 4)
        //                yield return i;
        //        }
        //    }
        //}
    }
}
