namespace PrintToPDFNode
{
    [Serializable]
    public class Config
    {
        public static string toPdfIp { get; set; }
        public static string toPdfTopic { get; set; }
        public static string Accesskey { get; set; }
        public static string Secretkey { get; set; }

        public static string toPdfTopicR { get; set; }


        public static string getToPdfTopicGroup()
        {
            return toPdfTopic + "_group"; ;
        }

        public static string getToPdfTopicGroupR()
        {
            return toPdfTopicR + "_group";
        }

    }
}
