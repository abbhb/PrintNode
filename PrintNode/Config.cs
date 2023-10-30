using System.Reflection.Metadata;

namespace PrintNode
{
    [Serializable]
    public class Config
    {
        public static string toPrintIp { get; set; }
        public static string toPrintTopic { get; set; }

        public static string Accesskey { get; set; }
        public static string Secretkey { get; set; }

        public static string toPrintTopicR { get; set; }


        public static string getToPrintTopicGroup()
        {
            return toPrintTopic + "_group";
        }

        public static string getToPrintTopicGroupR()
        {
            return toPrintTopicR + "_group";
        }

        public static string tag { get; set; }


        public static string name { get; set; }
        public static string myip { get; set; }
        public static int myport { get; set; }
        public static string consulip { get; set; }
        public static int consulport { get; set; }

        public static string printDeviceSettingExeName { get; set; }

        public static string driveName { get; set; }

    }
}
