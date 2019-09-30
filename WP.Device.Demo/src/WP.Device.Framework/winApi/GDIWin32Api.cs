using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WP.Device.Framework.Base
{
    /// <summary>
    /// 系统GDI API
    /// </summary>
    public class GDIWin32Api
    {
        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);
    }
}
