namespace PrintToPDFNode
{
    [Serializable]
    public class Config
    {
        public static string toPdfIp { get; set; }
        public static string toPdfTopic { get; set; }
        public static string Accesskey { get; set; }
        public static string Secretkey { get; set; }

        private static string toPdfTopicGroup = toPdfTopic+"_group";

        public static string toPdfTopicR { get; set; }

        private static string toPdfTopicGroupR = toPdfTopicR + "_group";

        public static string getToPdfTopicGroup()
        {
            return toPdfTopicGroup;
        }

        public static string getToPdfTopicGroupR()
        {
            return toPdfTopicGroupR;
        }

    }
}
