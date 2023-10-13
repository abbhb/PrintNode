using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Acrobat;
using iTextSharp.text.pdf;
using NewLife.Reflection;
using Word = Microsoft.Office.Interop.Word;

namespace PrintToPDFNode
{
    public class ToPDF
    {

        [DllImport("ole32.dll")]
        public static extern void CoInitialize(IntPtr pvReserved);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        public static string getLastFileName(string filePath)
        {
            return System.IO.Path.GetExtension(filePath);
        }
        /**
         pdf转pdf
         */
        public static ToPdfResp ToPdfByPdf(string filePath)
        {

            ToPdfResp resp = new()
            {
                pdfPage = getPdfNums(filePath),
                pdfPath = filePath
            };
            return resp;
        }

        /**
         * word To pdf
        */
        public static ToPdfResp ToPdfByWord(string filePath)
        {
            int pageCount = 0;
            string newFileName = TempFileUtil.tempPath + Path.GetRandomFileName()+".pdf";
            Console.WriteLine($"filename:{newFileName}");


            // 初始化COM库
            CoInitialize(IntPtr.Zero);
            // 创建Word应用程序对象
            var wordApp = new Word.Application();

            // 打开Word文档
            var doc = wordApp.Documents.Open(filePath);

            // 获取总页数
            doc.Repaginate();
            pageCount = doc.ComputeStatistics(Word.WdStatistic.wdStatisticPages);

            // 将Word文档另存为PDF
            doc.SaveAs2(newFileName, Word.WdSaveFormat.wdFormatPDF);

            // 关闭Word文档
            doc.Close();

            // 退出Word应用程序
            wordApp.Quit();
            // 反初始化COM库
            CoUninitialize();
            ToPdfResp resp = new()
            {
                pdfPage = pageCount,
                pdfPath = newFileName
            };
            return resp;

            
            

            return null;
        }


        /**
         * 任意文件转pdf，需要adobe
         */
        public static ToPdfResp ToPdfByAny(string filePath)
        {
            int pageCount = 0;
            string newFileName = TempFileUtil.tempPath + Path.GetRandomFileName() + ".pdf";
            // 初始化COM库
            CoInitialize(IntPtr.Zero); // 初始化COM库
            try
            {
                // Create the document (Can only create the AcroExch.PDDoc object using late-binding)
                // Note using VisualBasic helper functions, have to add reference to DLL
                // 创建Acrobat应用程序对
                AcroApp app = new AcroApp();
                AcroAVDoc avDoc = new AcroAVDoc();
                // 打开要转成PDF的文件

                avDoc.Open(filePath, "title");
                AcroPDDoc pdDoc = avDoc.GetPDDoc();
                // 执行转换操作
                pdDoc.Save(1, newFileName);


                // 关闭PDF文件
                pdDoc.Close();
                avDoc.Close(1);
                app.CloseAllDocs();

                pageCount = getPdfNums(newFileName);
                // 反初始化COM库
                ToPdfResp resp = new()
                {
                    pdfPage = pageCount,
                    pdfPath = newFileName
                };
                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { CoUninitialize(); }
        }

        private static int getPdfNums(string filePath)
        {
            PdfReader reader = new PdfReader(filePath);
            int iPageNum = reader.NumberOfPages;
            reader.Close(); //不关闭会一直占用pdf资源，对接下来的操作会有影响
            return iPageNum;
        }

    }
}
