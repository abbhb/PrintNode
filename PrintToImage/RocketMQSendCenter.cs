using NewLife.RocketMQ;

namespace PrintToImage
{
    //每一项都需要start
    public class RocketMQSendCenter
    {
        public static Producer toImageRespSend = new Producer()
        {
            Topic = Config.toImageTopicR,
            NameServerAddress = Config.toImageIp,
            AclOptions = new AclOptions()
            {
                AccessKey = Config.Accesskey,
                SecretKey = Config.Secretkey,
            },
        };
    }
}
