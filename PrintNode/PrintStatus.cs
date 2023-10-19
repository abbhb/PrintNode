using iTextSharp.text.pdf;

namespace PrintNode
{

    //打印机状态返回类
    [Serializable]
    public class PrintStatus
    {

        //唯一tag
        public string printName {  get; set; }

        public string printDescription { get; set; }

        //当前有多少任务已经在windows队列了
        public int listNums { get; set; }

        //繁忙啥的
        public string statusTypeMessage { get; set; }
        // 0:异常 1:空闲 2:繁忙 3:忙碌 4:爆满
        public int statusType {  get; set; }

        public List<PrintRW> printJobs;


    }
    [Serializable]
    public class PrintRW
    {
        //任务名
        public string documentName { get; set; }

        //页数
        public int pagesPrinted { get; set; }

        public string jobStatus { get; set; }

        public DateTime startTime { get; set; }


    }
}
