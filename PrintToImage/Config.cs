namespace PrintToImage
{
    [Serializable]
    public class Config
    {
        public static string toImageIp { get; set; }
        public static string toImageTopic { get; set; }

        public static string Accesskey { get; set; }
        public static string Secretkey { get; set; }
        public static string toImageTopicR { get; set; }

        private static string toImageTopicGroupR = toImageTopicR + "_group";

        public static string getToImageTopicGroup()
        {
            return toImageTopic + "_group";
        }
        public static string getToImageTopicGroupR()
        {
            return toImageTopicGroupR;
        }


    }
}
