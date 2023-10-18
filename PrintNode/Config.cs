namespace PrintNode
{
    [Serializable]
    public class Config
    {
        public static string toPrintIp { get; set; }
        public static string toPrintTopic { get; set; }

        public static string Accesskey { get; set; }
        public static string Secretkey { get; set; }

        private static string toPrintTopicGroup = toPrintTopic + "_group";

        public static string toPrintTopicR { get; set; }

        private static string toPrintTopicGroupR = toPrintTopicR + "_group";

        public static string getToPrintTopicGroup()
        {
            return toPrintTopicGroup;
        }

        public static string getToPrintTopicGroupR()
        {
            return toPrintTopicGroupR;
        }

        public static string tag { get; set; }


        public static string name { get; set; }

    }
}
