namespace PrintNode
{
    public class MyException<T>
    {
        public Exception exception {  get; set; }
        public List<NewLife.RocketMQ.Protocol.MessageExt> messageExts { get; set; }

        public MyException(Exception exception, List<NewLife.RocketMQ.Protocol.MessageExt> messageExts)
        {
            this.exception = exception;
            this.messageExts = messageExts;
        }

    }
}
