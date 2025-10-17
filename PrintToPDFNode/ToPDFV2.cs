using ConvertPDFByMicrosoft;
using iTextSharp.text.pdf;

namespace PrintToPDFNode
{
    public class ToPDFV2
    {
        // 任意文件转pdf
        public static ToPdfResp ToPdf(string filePath)
        {
            int pageCount = 0;
            string newFileName = TempFileUtil.tempPath + Path.GetRandomFileName() + ".pdf";
            Console.WriteLine($"ToPdfV2:ToPdf:filename:{newFileName}");
            filePath.ConvertToPdf(newFileName);
            // 获取总页数
            pageCount = getPdfNums(newFileName);
            ToPdfResp resp = new()
            {
                pdfPage = pageCount,
                pdfPath = newFileName
            };
            return resp;

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
