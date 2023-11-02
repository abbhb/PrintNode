using NewLife;
using Newtonsoft.Json;

namespace PrintNode
{
    public class ToPrintCallBack
    {
        // 创建哈希映射 id,次数，要求超过3次就直接消费消息
        private static Dictionary<string, int> hashMap = new Dictionary<string, int>();
        // 定义回调函数和异常处理回调
        public static Action<List<NewLife.RocketMQ.Protocol.MessageExt>> successCallback = result =>
        {
            //Console.WriteLine("Operation succeeded. Result: " + result);
            //实际一次就一条消息
            foreach (var item in result)
            {
                //string msg = $"消息：msgId={item.MsgId},key={item.Keys},tags={item.Tags}，产生时间【{item.BornTimestamp.ToDateTime()}】，内容>需要序列化";
                PrintDataPDFToPrintReq json = JsonConvert.DeserializeObject<PrintDataPDFToPrintReq>(item.Body.ToStr());//反序列化对象
                //Console.WriteLine(msg);
                //Console.WriteLine($"消息JSON：{json.ToString}");

                if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                {
                    Console.WriteLine($"JSON?:{json}");
                    // 保存源文件到本地
                    string fileNames = TempFileUtil.saveFileByName(json.filePDFUrl,json.name);
                    string filetemppath = TempFileUtil.tempPath + fileNames;
                    ToPrintResp printR = null;
                    ToPrint toPrintResp = new ToPrint();
                    for (global::System.Int32 i = 0; i < json.copies; i++)
                    {
                        printR = toPrintResp.printAsync(filetemppath, json, json.name);
                    }
                    //最终结果
                    if (printR == null || (!printR.isSuccess))
                    {
                        //打印不成功
                        throw new Exception("服务异常,请重试！");
                    }
                    else
                    {
                        //打印成功，返回消息
                        //返回消息，否者报错
                        PrintDataFromPrintResp prsresp = new PrintDataFromPrintResp();
                        prsresp.id = json.id;
                        prsresp.isSuccess = 1;

                        //返回的消息
                        RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                        //一旦异常会进外面的异常里，否则消费消息
                    }
                }

            }
            //如果异常直接throw
        };

        //所有的异常在这处理，返回就行
        public static Func<MyException<List<NewLife.RocketMQ.Protocol.MessageExt>>, Boolean> errorCallback = (ex) =>
        {
            Console.WriteLine("An error occurred: " + ex.exception.Message);
            try
            {
                //如果多条消息就记录第一条
                foreach (var item in ex.messageExts)
                {
                    //string msg = $"消息：msgId={item.MsgId},key={item.Keys},tags={item.Tags}，产生时间【{item.BornTimestamp.ToDateTime()}】，内容>需要序列化";
                    PrintDataPDFToPrintReq json = JsonConvert.DeserializeObject<PrintDataPDFToPrintReq>(item.Body.ToStr());//反序列化对象
                    //Console.WriteLine(msg);
                    //Console.WriteLine($"消息JSON：{json.ToString}");

                    if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                    {

                        PrintDataFromPrintResp prsresp = new PrintDataFromPrintResp();
                        prsresp.id = json.id;
                        prsresp.isSuccess = 1;

                        //返回的消息
                        RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                    }
                }
                //发送回执成功就消费消息
                return true;
            }
            catch (Exception)
            {
                if (ex.messageExts.Count < 1)
                {
                    //没有消息还消费个屁
                    return true;
                }
                PrintDataPDFToPrintReq errors = JsonConvert.DeserializeObject<PrintDataPDFToPrintReq>(ex.messageExts[0].Body.ToStr());//反序列化对象


                if (hashMap.ContainsKey(errors.id))
                {
                    //之前已经错过了
                    if (hashMap[errors.id] >= 3)
                    {
                        //超过三次直接消费消息,并且清理本地哈希表
                        hashMap.Remove(errors.id);
                        return true;
                    }
                    hashMap[errors.id] += 1;
                    return false;
                }
                hashMap[errors.id] = 1;
                return false;
                //一次失败可能不是一直失败，可以拒绝消费3次
                //失败也在本地缓存，同一id的消息超过多少次就不再重试


            }
            finally
            {
                //最终看看哈希表，要是太多数据，又是那种没有超过三次错误没被回收的，选取时间长的回收掉，最多允许存在50条
                if (hashMap.Count > 50)
                {
                    //直接清空
                    hashMap.Clear();
                }
            }

            //只有能发送回执才算消费成功，回执都无法发送的消息就消费失败
            //return true;//如果发送回执消息成功就消费消息
            //否则失败
        };
    }
}
