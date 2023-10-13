using NewLife.RocketMQ;

namespace PrintToPDFNode
{
    //每一项都需要start
    public class RocketMQSendCenter
    {
        public static Producer toPDFRespSend = new Producer()
        {
            Topic = Config.toPdfTopic,
            NameServerAddress = Config.toPdfIp,
        };
    }
}
