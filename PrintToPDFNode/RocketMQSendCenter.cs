using NewLife.RocketMQ;

namespace PrintToPDFNode
{
    //每一项都需要start
    public class RocketMQSendCenter
    {
        public static Producer toPDFRespSend = new Producer()
        {
            Topic = Config.toPdfTopicR,
            NameServerAddress = Config.toPdfIp,
            AclOptions = new AclOptions()
            {
                AccessKey = Config.Accesskey,
                SecretKey = Config.Secretkey,
            }
        };
    }
}
