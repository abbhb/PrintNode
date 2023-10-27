using Acrobat;
using Org.BouncyCastle.Asn1.Utilities;
using PdfiumViewer;
using PInvoke;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

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
                Console.WriteLine($"filePath:{filePath},fileName:{printReq.name}");
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

                PrinterSettings settings = new PrinterSettings();

                // 打开打印机
                // 获取默认打印机的 DEVMODE
                IntPtr hDevMode = settings.GetHdevmode();
                if (hDevMode != IntPtr.Zero)
                {
                    // 锁定 DEVMODE 内存
                    IntPtr pDevMode = GlobalLock(hDevMode);

                    if (pDevMode != IntPtr.Zero)
                    {
                        // 修改 DEVMODE 的属性
                        DEVMODE devMode = (DEVMODE)Marshal.PtrToStructure(pDevMode, typeof(DEVMODE));
                        if (settings.CanDuplex)
                        {
                            if (printReq.isDuplex.Equals(1))
                            {
                                devMode.dmDuplex = DEVMODE.PrintDuplexOptions.DMDUP_SIMPLEX; // 设置双面打印

                            }
                            if (printReq.isDuplex.Equals(2))
                            {
                                devMode.dmDuplex = DEVMODE.PrintDuplexOptions.DMDUP_VERTICAL; // 设置双面打印

                            }
                            if (printReq.isDuplex.Equals(3))
                            {
                                devMode.dmDuplex = DEVMODE.PrintDuplexOptions.DMDUP_HORIZONTAL; // 设置双面打印
                            }
                        }
                        else
                        {
                            devMode.dmDuplex = DEVMODE.PrintDuplexOptions.DMDUP_SIMPLEX; // 设置双面打印

                        }
                        devMode.dmCopies = 1;
                        devMode.dmOrientation = printReq.landscape.Equals(0) ? DEVMODE.PrinterOrientationOptions.DMORIENT_LANDSCAPE: DEVMODE.PrinterOrientationOptions.DMORIENT_PORTRAIT;
                        // 解锁 DEVMODE 内存
                        GlobalUnlock(hDevMode);
                    }
                    // 应用 DEVMODE 设置
                    settings.SetHdevmode(hDevMode);
                    settings.DefaultPageSettings.SetHdevmode(hDevMode);
                }
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
