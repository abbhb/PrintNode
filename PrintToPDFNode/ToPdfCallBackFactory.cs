using NewLife;
using Newtonsoft.Json;
using NLog.Fluent;
using PrintToPDFNode;

namespace PrintToPDFNode
{
    public class ToPdfCallBackFactory{
        // 定义 PDF 转换策略
        private static Dictionary<string, IPdfConversionStrategy> conversionStrategies = new Dictionary<string, IPdfConversionStrategy>{
        { "pdf", new ByPdf() },
        { "word", new ByWord() },
        { "any", new ByAny() },
        // 添加其他文件类型的转换策略
        
    };

        public static IPdfConversionStrategy getPdfConversion(string lastFileName)
        {
            if (lastFileName.EndsWith("pdf"))
            {
                return conversionStrategies["pdf"];

            }
            else if (lastFileName.EndsWith("doc") || lastFileName.EndsWith("docx"))
            {
                return conversionStrategies["word"];
            }
            else
            {
                //通用方法
                return conversionStrategies["any"];

            }
        }



    }

}
