using Acrobat;
using PdfiumViewer;
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


        public static ToPrintResp print(string filePath, PrintDataPDFToPrintReq printReq)
        {
            try
            {
                //调用默认打印机，或者配置文件配置一下
                CoInitialize(nint.Zero);
                AcroApp app = new AcroApp();
                AcroAVDoc avDoc = new AcroAVDoc();
                // 打开要转成PDF的文件
                Console.WriteLine($"filePath:{filePath},fileName:{printReq.name}");
                avDoc.Open(filePath, printReq.name);
                AcroPDDoc pdDoc = avDoc.GetPDDoc();
                int startPage = printReq.startNum;
                int endPage = printReq.endNum;
                //最大份数不在node上约束
                int copies = printReq.copies;
                if(endPage>pdDoc.GetNumPages())
                {
                    //用户越界，给他重置到最后一页
                    endPage = pdDoc.GetNumPages();
                }
                if(startPage > endPage)
                {
                    return new ToPrintResp { isSuccess = false, message = "起始页越界" };
                }

                PrinterSettings settings = new PrinterSettings();
                string defaultPrinter = settings.PrinterName;

                // 打开打印机
                // 修改打印设置
                PrinterSettings printerSettings = new PrinterSettings();

                printerSettings.PrinterName = defaultPrinter; // 替换为你的打印机名称
                printerSettings.Copies = 1; // 设置打印份数
                printerSettings.DefaultPageSettings.Landscape = printReq.landscape.Equals(0)?false:true; // 设置打印方向为横向
                if(printReq.isDuplex.Equals(1))
                {
                    printerSettings.Duplex = Duplex.Simplex;

                }
                if (printReq.isDuplex.Equals(2))
                {
                    printerSettings.Duplex = Duplex.Vertical;

                }
                if (printReq.isDuplex.Equals(3))
                {
                    printerSettings.Duplex = Duplex.Horizontal;
                }
                // 应用新的默认打印设置
                printerSettings.SetHdevmode(printerSettings.GetHdevmode()); // 保存修改后的设置

                avDoc.PrintPagesSilent(startPage, endPage, 2,1,1);
                //得关闭

                ToPrintResp toPrintResp = new ToPrintResp()
                {
                    isSuccess = true,
                };
                avDoc.Close(0);
                app.Exit();
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
