using NewLife;

namespace PrintNode
{
    public class GetPrintDeviceStatus
    {
        //写一个通用方法，后续数据结构不会变动
        public static PrintDeviceStatus getPrintDeviceStatu()
        {
            PrintDeviceStatus prds = new PrintDeviceStatus();
            try
            {
                string clsa = Class1.Get3();
                if (StringHelper.IsNullOrEmpty(clsa)){
                    throw new Exception();
                }
                if (clsa.Equals("未找到窗口"))
                {
                    prds.type = 0;
                    prds.message = "打印机可能异常";
                }else if(!clsa.Equals("休眠")&& !clsa.Equals("正常")&&!clsa.EndsWith("就绪"))
                {
                    prds.type = 0;
                    prds.message = clsa;
                }
                else
                {
                    prds.type = 1;
                }
                return prds;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                prds.type = 0;
                prds.message = "当前打印机不支持状态监测";
                return prds;
            }
            
        }
    }
    [Serializable]
    public class PrintDeviceStatus
    {
        //1正常，0为异常返回
        public int type;

        public string message;
    }
    }
