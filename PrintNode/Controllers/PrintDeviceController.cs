using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Management;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PrintNode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class PrintDeviceController : ControllerBase
    {
        // GET: api/<PrintDeviceController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PrintDeviceController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        // 获取该打印机状态
        // GET api/<PrintDeviceController>/status
        [HttpGet("status")]
        public R<PrintStatus>  GetStatus()
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
                string printerName = printJob["HostPrintQueue"] as string;
                int pagesPrinted = Convert.ToInt32(printJob["PagesPrinted"]);
                string jobId = Convert.ToString(printJob["JobId"]);
                listNums++;
                PrintRW printRW = new PrintRW()
                {
                    documentName = documentName,
                    pagesPrinted = pagesPrinted,
                    jobStatus = printJob["JobStatus"] as string,
                    startTime = Convert.ToDateTime(printJob["StartTime"])

                };
                Console.WriteLine("打印任务: " + documentName);
                Console.WriteLine("打印机名称: " + printerName);
                Console.WriteLine("打印页数: " + pagesPrinted);
                Console.WriteLine();
                printJobsas.Add(printRW);
            }
            if(listNums>0&& listNums < 3)
            {
                statusType = 2;
                statusMessage = "繁忙";
            }
            else if (listNums>3) {
                statusType = 3;
                statusMessage = "忙碌";
            }
            else if(listNums>6) {
                statusType = 4;
                statusMessage = "爆满";
            }
            else
            {
                statusMessage = "空闲";
                statusType = 1;
            }
            PrintDeviceStatus printDeviceStatus = GetPrintDeviceStatus.getPrintDeviceStatu();
            if(printDeviceStatus == null)
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
            Console.WriteLine($"printStatusa = {printStatusa}");

            return R<PrintStatus>.success(printStatusa);
        }

        // POST api/<PrintDeviceController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PrintDeviceController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        //尝试取消该打印机上的指定id的打印任务，不一定成功
        // DELETE api/<PrintDeviceController>/5
        [HttpDelete("cancel/{id}")]
        public R<string> Delete(string id)
        {
            int jobIdToCancel = Convert.ToInt32(id); // 将此替换为要取消的作业的实际 Job ID

            // 使用 WMI 查询来获取打印队列信息
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob");

            // 获取所有打印任务
            ManagementObjectCollection printJobs = searcher.Get();

            foreach (ManagementObject printJob in printJobs)
            {
                int jobId = Convert.ToInt32(printJob["JobId"]);

                if (jobId == jobIdToCancel)
                {
                    try
                    {
                        // 取消打印任务
                        printJob.InvokeMethod("Delete", null);
                        Console.WriteLine("已取消打印任务，Job ID: " + jobId);
                        return R<string>.success("成功取消，但是要是别人的任务等问一声，别给别人取消了！");
                    }
                    catch (ManagementException e)
                    {
                        Console.WriteLine("无法取消打印任务，Job ID: " + jobId);
                        Console.WriteLine("错误消息: " + e.Message);
                        return R<string>.error($"取消失败，{e.Message}");

                    }
                }
            }
            return R<string>.error("取消失败，可能已经打印完了！");
        }
    }
}
