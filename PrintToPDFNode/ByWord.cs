using NewLife.Serialization;
using NewLife.Serialization.Json;
using Newtonsoft.Json;
using NLog.Fluent;

namespace PrintToPDFNode
{
    public class ByWord : IPdfConversionStrategy
    {
        public ToPdfResp ConvertToPdf(string filetemppath)
        {
            try
            {
                return ToPDF.ToPdfByWord(filetemppath);
            }catch (Exception ex)
            {
                Log.Error("Word异常");
                Log.Error(ex.Message);
                return ToPDF.ToPdfByAny(filetemppath);
            }
            
        }
    }
}
