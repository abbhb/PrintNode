namespace PrintNode
{
    [Serializable]
    public class PrintDataFromPrintResp
    {
        //打印队列的返回值数据结构

        public string id {  get; set; }

        public int isSuccess { get; set; }


        public string message { get; set; }

    }
}
