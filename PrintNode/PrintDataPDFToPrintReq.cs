namespace PrintNode
{
    [Serializable]
    public class PrintDataPDFToPrintReq
    {
        public string id {get; set;}

        public string name { get; set;}

        public int copies { get; set; }

        public int isDuplex { get; set; }

        public int startNum { get; set; }

        public int endNum { get; set; }

        public string filePDFUrl { get; set; }

        //是否纵向
        public int landscape;//默认纵向
    }
}
