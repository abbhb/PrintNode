namespace PrintToPDFNode
{
    //转pdf的消息返回对象
    [Serializable]
    public class PrintDataFromPDFResp
    {
        public string id {  get; set; }
        /**
         * pdf下载链接
         */
        public string filePDFUrl { get; set; }


        /**
         * 文件有多少页
         */
        public int pageNums { get; set; }

        /**
         * 1为成功
         * 0为失败
         */
        public int status { get; set; }
        /**
        * 失败的话原因
        */
        public string message { get; set; }


    }
}
