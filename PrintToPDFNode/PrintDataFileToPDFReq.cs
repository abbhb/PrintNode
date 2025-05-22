namespace PrintToPDFNode
{
    /**
     * v1版本 生产者发送给转pdf的数据内容
     */
    [Serializable]
    public class PrintDataFileToPDFReq
    {
        /**
        * 该id作为uuid
        */
        public string id {  get; set; }

        /**
         * 源文件下载链接
         */
        public string fileUrl {  get; set; }

        /**
        * pdf下载链接
        */
        public string filePDFUrl {  get; set; }
        /**
         * pdf OSS上传链接
         */
        public string filePDFUploadUrl { get; set; }

    }
}
