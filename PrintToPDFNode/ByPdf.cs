using NewLife.Serialization;
using NewLife.Serialization.Json;
using Newtonsoft.Json;
using NLog.Fluent;

namespace PrintToPDFNode
{
    public class ByPdf : IPdfConversionStrategy
    {
        public ToPdfResp ConvertToPdf(string filetemppat)
        {
            // 根据文件类型执行不同的转换操作
            // 如果是 PDF 文件，则直接返回
            return ToPDF.ToPdfByPdf(filetemppat);
        }
    }

}
