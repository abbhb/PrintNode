using NewLife;
using System.Globalization;
using System.Management;

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

        private static DateTime GetDateTime(string time)
        {
            if (time == null){
                return DateTime.Now;
            }
            string input = time;
            // 提取并转换时区偏移
            string offsetPart = input.Substring(input.Length - 4);
            int offsetMinutes = int.Parse(offsetPart.Substring(1));
            TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);
            string formattedOffset = $"{(offsetPart[0] == '+' ? "+" : "-")}{offset.Hours:D2}:{offset.Minutes:D2}";

            // 构建修正后的字符串
            string correctedInput = input.Substring(0, input.Length - 4) + formattedOffset;

            // 解析为 DateTimeOffset
            DateTimeOffset dto = DateTimeOffset.ParseExact(
                correctedInput,
                "yyyyMMddHHmmss.ffffffzzz",
                CultureInfo.InvariantCulture
            );
            return dto.DateTime;
        }

        public static PrintNode.PrintStatus getStatus()
        {

            // 使用 WMI 查询来获取打印队列信息
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob");

            // 获取所有打印任务
            ManagementObjectCollection printJobs = searcher.Get();
            int listNums = 0;
            int statusType = 0;
            string statusMessage = "异常";
            List<PrintRW> printJobsas = new List<PrintRW>();
            foreach (ManagementObject printJob in printJobs)
            {
                string documentName = printJob["Document"] as string;
                int pagesPrinted = Convert.ToInt32(printJob["PagesPrinted"]);
                int allPaper = Convert.ToInt32(printJob["TotalPages"]);

                string jobId = Convert.ToString(printJob["JobId"]);
                listNums++;
                PrintRW printRW = new()
                {
                    documentName = documentName,
                    pagesPrinted = pagesPrinted,
                    pageCount = allPaper,
                    id = jobId,
                    jobStatus = printJob["JobStatus"] as string,
                    startTime = Convert.ToDateTime(printJob["StartTime"]),
                    jobSubmitTime = GetDateTime(Convert.ToString(printJob["TimeSubmitted"]))

                };
                Console.WriteLine("打印任务: " + documentName);
                Console.WriteLine("打印页数: " + pagesPrinted);
                Console.WriteLine();
                printJobsas.Add(printRW);
            }
            if (listNums > 0 && listNums < 3)
            {
                statusType = 2;
                statusMessage = "繁忙";
            }
            else if (listNums > 3)
            {
                statusType = 3;
                statusMessage = "忙碌";
            }
            else if (listNums > 6)
            {
                statusType = 4;
                statusMessage = "爆满";
            }
            else
            {
                statusMessage = "空闲";
                statusType = 1;
            }
            PrintDeviceStatus printDeviceStatus = GetPrintDeviceStatus.getPrintDeviceStatu();
            if (printDeviceStatus == null)
            {
                statusType = 0;
                statusMessage = "打印机异常";
            }
            else
            {
                if (!printDeviceStatus.type.Equals(1))
                {
                    statusType = 0;
                    statusMessage = printDeviceStatus.message;
                }
            }

            PrintStatus printStatusa = new PrintStatus { printName = Config.tag, printDescription = Config.name, listNums = listNums, statusType = statusType, statusTypeMessage = statusMessage, printJobs = printJobsas };
            return printStatusa;
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
