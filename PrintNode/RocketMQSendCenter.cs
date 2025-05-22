using NewLife.RocketMQ;

namespace PrintNode
{
    //每一项都需要start
    public class RocketMQSendCenter
    {
        public static Producer toPDFRespSend = new Producer()
        {
            Topic = Config.toPrintTopicR,
            NameServerAddress = Config.toPrintIp,
            AclOptions = new AclOptions()
            {
                AccessKey = Config.Accesskey,
                SecretKey = Config.Secretkey,
            },
        };
    }
}
