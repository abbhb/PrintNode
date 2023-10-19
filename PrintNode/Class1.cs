using System.Runtime.InteropServices;
using System.Text;

namespace PrintNode
{
    public class Class1
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, StringBuilder lParam);



        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetDlgCtrlID(IntPtr hwndCtl);

        private const int WM_GETTEXT = 0x000D;
        public static string Get()
        {
            // 通过窗口标题获取窗口句柄
            IntPtr hwnd = FindWindow(null, "Brother 状态监控器");

            if (hwnd != IntPtr.Zero)
            {
                // 获取label标签的句柄


                int aa2s1, aa2s2, aa2s3;
                aa2s1 = 0;
                aa2s2 = 0;
                aa2s3 = 0;

                // 获取label标签的内容
                StringBuilder sb = new StringBuilder(1024);
                int v = SendMessage(FindControl(hwnd, 1016), WM_GETTEXT, sb.Capacity, sb);
                string labelText = sb.ToString();
                aa2s1 = 1;
                string as1w1 = labelText;

                IntPtr labelHwnd2 = FindControl(hwnd, 1023);

                // 获取label标签的内容
                StringBuilder sb2 = new StringBuilder(1024);
                SendMessage(labelHwnd2, WM_GETTEXT, sb2.Capacity, sb2);
                string labelText2 = sb2.ToString();
                aa2s2 = 1;
                string as1w2 = labelText2;

                IntPtr labelHwnd3 = FindControl(hwnd, 1009);

                // 获取label标签的内容
                StringBuilder sb3 = new StringBuilder(1024);
                SendMessage(labelHwnd3, WM_GETTEXT, sb3.Capacity, sb3);
                string labelText3 = sb3.ToString();
                aa2s3 = 1;
                string as1w3 = labelText3;

                if (aa2s1 == 1 && aa2s2 == 1 && aa2s3 == 1)
                {
                    if (as1w1.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w2.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w3.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    //string sas = as1w1+",";
                    //byte[] bytes1 = System.Text.Encoding.UTF8.GetBytes(as1w1);
                    //byte[] bytes2 = System.Text.Encoding.UTF8.GetBytes(as1w2);
                    //byte[] bytes3 = System.Text.Encoding.UTF8.GetBytes(as1w3);
                    //byte[] bytes4 = System.Text.Encoding.UTF8.GetBytes(",");
                    //string str11 = System.Text.Encoding.UTF8.GetString(bytes1);
                    //string str12 = System.Text.Encoding.UTF8.GetString(bytes2);
                    //string str13 = System.Text.Encoding.UTF8.GetString(bytes3);
                    //string str14 = System.Text.Encoding.UTF8.GetString(bytes4);
                    // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
                    byte[] utf16Bytes = Encoding.Unicode.GetBytes(as1w1 + "," + as1w2 + "," + as1w3);
                    byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
                    //return Encoding.Default.GetString(utf8Bytes); 
                    return as1w1;
                }
                else
                {
                    return "未找到窗口";
                }


            }
            else
            {
                return "未找到窗口";
            }
        }


        public static string Get2()
        {
            // 通过窗口标题获取窗口句柄
            IntPtr hwnd = FindWindow(null, "Brother 状态监控器");

            if (hwnd != IntPtr.Zero)
            {
                // 获取label标签的句柄


                int aa2s1, aa2s2, aa2s3;
                aa2s1 = 0;
                aa2s2 = 0;
                aa2s3 = 0;

                // 获取label标签的内容
                StringBuilder sb = new StringBuilder(1024);
                int v = SendMessage(FindControl(hwnd, 1016), WM_GETTEXT, sb.Capacity, sb);
                string labelText = sb.ToString();
                aa2s1 = 1;
                string as1w1 = labelText;

                IntPtr labelHwnd2 = FindControl(hwnd, 1023);

                // 获取label标签的内容
                StringBuilder sb2 = new StringBuilder(1024);
                SendMessage(labelHwnd2, WM_GETTEXT, sb2.Capacity, sb2);
                string labelText2 = sb2.ToString();
                aa2s2 = 1;
                string as1w2 = labelText2;

                IntPtr labelHwnd3 = FindControl(hwnd, 1009);

                // 获取label标签的内容
                StringBuilder sb3 = new StringBuilder(1024);
                SendMessage(labelHwnd3, WM_GETTEXT, sb3.Capacity, sb3);
                string labelText3 = sb3.ToString();
                aa2s3 = 1;
                string as1w3 = labelText3;

                if (aa2s1 == 1 && aa2s2 == 1 && aa2s3 == 1)
                {
                    if (as1w1.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w2.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w3.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    //string sas = as1w1+",";
                    //byte[] bytes1 = System.Text.Encoding.UTF8.GetBytes(as1w1);
                    //byte[] bytes2 = System.Text.Encoding.UTF8.GetBytes(as1w2);
                    //byte[] bytes3 = System.Text.Encoding.UTF8.GetBytes(as1w3);
                    //byte[] bytes4 = System.Text.Encoding.UTF8.GetBytes(",");
                    //string str11 = System.Text.Encoding.UTF8.GetString(bytes1);
                    //string str12 = System.Text.Encoding.UTF8.GetString(bytes2);
                    //string str13 = System.Text.Encoding.UTF8.GetString(bytes3);
                    //string str14 = System.Text.Encoding.UTF8.GetString(bytes4);
                    // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
                    byte[] utf16Bytes = Encoding.Unicode.GetBytes(as1w1 + "," + as1w2 + "," + as1w3);
                    byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
                    //return Encoding.Default.GetString(utf8Bytes); 
                    return as1w2;
                }
                else
                {
                    return "未找到窗口";
                }


            }
            else
            {
                return "未找到窗口";
            }
        }
        public static string Get3()
        {
            // 通过窗口标题获取窗口句柄
            IntPtr hwnd = FindWindow(null, "Brother 状态监控器");

            if (hwnd != IntPtr.Zero)
            {
                // 获取label标签的句柄


                int aa2s1, aa2s2, aa2s3;
                aa2s1 = 0;
                aa2s2 = 0;
                aa2s3 = 0;

                // 获取label标签的内容
                StringBuilder sb = new StringBuilder(1024);
                int v = SendMessage(FindControl(hwnd, 1016), WM_GETTEXT, sb.Capacity, sb);
                string labelText = sb.ToString();
                aa2s1 = 1;
                string as1w1 = labelText;

                IntPtr labelHwnd2 = FindControl(hwnd, 1023);

                // 获取label标签的内容
                StringBuilder sb2 = new StringBuilder(1024);
                SendMessage(labelHwnd2, WM_GETTEXT, sb2.Capacity, sb2);
                string labelText2 = sb2.ToString();
                aa2s2 = 1;
                string as1w2 = labelText2;

                IntPtr labelHwnd3 = FindControl(hwnd, 1009);

                // 获取label标签的内容
                StringBuilder sb3 = new StringBuilder(1024);
                SendMessage(labelHwnd3, WM_GETTEXT, sb3.Capacity, sb3);
                string labelText3 = sb3.ToString();
                aa2s3 = 1;
                string as1w3 = labelText3;

                if (aa2s1 == 1 && aa2s2 == 1 && aa2s3 == 1)
                {
                    if (as1w1.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w2.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    if (as1w3.Length < 1)
                    {
                        return "未找到窗口";
                    }
                    //string sas = as1w1+",";
                    //byte[] bytes1 = System.Text.Encoding.UTF8.GetBytes(as1w1);
                    //byte[] bytes2 = System.Text.Encoding.UTF8.GetBytes(as1w2);
                    //byte[] bytes3 = System.Text.Encoding.UTF8.GetBytes(as1w3);
                    //byte[] bytes4 = System.Text.Encoding.UTF8.GetBytes(",");
                    //string str11 = System.Text.Encoding.UTF8.GetString(bytes1);
                    //string str12 = System.Text.Encoding.UTF8.GetString(bytes2);
                    //string str13 = System.Text.Encoding.UTF8.GetString(bytes3);
                    //string str14 = System.Text.Encoding.UTF8.GetString(bytes4);
                    // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
                    byte[] utf16Bytes = Encoding.Unicode.GetBytes(as1w1 + "," + as1w2 + "," + as1w3);
                    byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
                    //return Encoding.Default.GetString(utf8Bytes); 
                    return as1w3;
                }
                else
                {
                    return "未找到窗口";
                }


            }
            else
            {
                return "未找到窗口";
            }
        }

        public static IntPtr FindControl(IntPtr parentHwnd, int controlId)
        {
            IntPtr childHwnd = IntPtr.Zero;
            IntPtr result = IntPtr.Zero;
            while ((childHwnd = FindWindowEx(parentHwnd, childHwnd, null, null)) != IntPtr.Zero)
            {
                int id = GetDlgCtrlID(childHwnd);
                if (id == controlId)
                {
                    result = childHwnd;
                    break;
                }
                else
                {
                    result = FindControl(childHwnd, controlId);
                    if (result != IntPtr.Zero)
                    {
                        break;
                    }
                }
            }
            return result;
        }
    }
}
