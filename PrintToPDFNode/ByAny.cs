using PrintToPDFNode;

namespace PrintToPDFNode
{
    public class ByAny : IPdfConversionStrategy
    {
        public ToPdfResp ConvertToPdf(string filePath)
        {
            // 根据文件类型执行不同的转换操作
            // 如果是 PDF 文件，则直接返回
            return ToPDF.ToPdfByAny(filePath);
        }
    }
}
