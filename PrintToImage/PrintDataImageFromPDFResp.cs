namespace PrintToImage
{
    [Serializable]
    public class PrintDataImageFromPDFResp
    {
        public string id {  get; set; }

        /**
         * 1为成功
         * 0为失败
         */
        public int status { get; set; }
        public string message {  get; set; }

        public string filePDFImageUrl { get; set; }


    }
}
