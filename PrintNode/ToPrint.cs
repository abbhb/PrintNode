using Acrobat;
using NLog.Fluent;
using Org.BouncyCastle.Asn1.Utilities;
using PdfiumViewer;
using PInvoke;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using static PInvoke.DEVMODE;

namespace PrintNode
{
    public class ToPrint
    {

        [DllImport("ole32.dll")]
        public static extern void CoInitialize(IntPtr pvReserved);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetPrinter(IntPtr hPrinter, int Level, IntPtr pPrinter, int Command);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetPrinter(IntPtr hPrinter, int Level, IntPtr pPrinter, int cbBuf, out int pcbNeeded);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("PrintConfigSetting.dll", EntryPoint = "PrintSet", CharSet = CharSet.Ansi)]
        extern static int PrintSet(string deviceaName, int duplex, int printingDirection, int color);

        [DllImport("PrintConfigSetting.dll", EntryPoint = "Testasdd", CharSet = CharSet.Ansi)]
        extern static int Testasdd(int a, int b);


        public ToPrintResp printAsync(string filePath, PrintDataPDFToPrintReq printReq,string filename)
        {
            try
            {
                //调用默认打印机，或者配置文件配置一下
                CoInitialize(nint.Zero);
                AcroApp app = new Acrobat.AcroApp();
                app.Hide();
                AcroAVDoc avDoc = new AcroAVDoc();
                CAcroPDDoc _pdDoc = null;
                // 打开要转成PDF的文件
                Console.WriteLine($"filePath:{filePath},fileName:{filename}");
                avDoc.Open(filePath, filename);
                _pdDoc = avDoc.GetPDDoc();
                int startPage = printReq.startNum;
                int endPage = printReq.endNum;
                //最大份数不在node上约束
                int copies = printReq.copies;
                if(endPage> _pdDoc.GetNumPages())
                {
                    //用户越界，给他重置到最后一页
                    endPage = _pdDoc.GetNumPages();
                }
                if(startPage > endPage)
                {
                    return new ToPrintResp { isSuccess = false, message = "起始页越界" };
                }
                int fangxiang = 0;//默认竖着
                if (!printReq.landscape.Equals(0))
                {
                    fangxiang = 1;
                }
                int afdas = Testasdd(1, 2);
                Console.WriteLine($"输出：{afdas}");

                 PrinterSettings settings = new PrinterSettings();
                if (settings.CanDuplex)
                {
                    if (printReq.isDuplex.Equals(1))
                    {
                        PrintSet(settings.PrinterName, 1, fangxiang, 0);

                    }
                    if (printReq.isDuplex.Equals(2))
                    {
                        PrintSet(settings.PrinterName, 2, fangxiang, 0);

                    }
                    if (printReq.isDuplex.Equals(3))
                    {
                        PrintSet(settings.PrinterName, 3, fangxiang, 0);
                    }
                }
                else
                {
                    PrintSet(settings.PrinterName, 1, fangxiang, 0);

                }

                Console.WriteLine($"当前的配置：duplex:{settings.Duplex},ori:{settings.DefaultPageSettings.Landscape}");
                avDoc.PrintPagesSilent(startPage-1, endPage-1, 2,1,1);
                //得关闭
                avDoc.Close(0);
                app.Exit();
                ToPrintResp toPrintResp = new ToPrintResp()
                {
                    isSuccess = true,
                };
                
                return toPrintResp;

            }catch (Exception e)
            {
                Log.Error();
                Console.WriteLine(e.ToString());
                //异常就是打印失败
                ToPrintResp toPrintResp = new ToPrintResp()
                {
                    isSuccess = false,
                    message = e.Message
                };
                return toPrintResp;
            }finally { 

                CoUninitialize(); 
            }

           
        }
    }
}
