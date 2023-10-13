namespace PrintToPDFNode
{
    [Serializable]
    public class Config
    {
        public static string toPdfIp { get; set; }
        public static string toPdfTopic { get; set; }

        private static string toPdfTopicGroup = toPdfTopic+"_group";

        public static string getToPdfTopicGroup()
        {
            return toPdfTopicGroup;
        }



    }
}
