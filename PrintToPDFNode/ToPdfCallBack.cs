using NewLife;
using NewLife.Remoting;
using NewLife.RocketMQ;
using NewLife.RocketMQ.Protocol;
using NewLife.Serialization.Json;
using Newtonsoft.Json;
using NLog.Fluent;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace PrintToPDFNode
{
    public class ToPdfCallBack
    {
        // 定义回调函数和异常处理回调
        public static Action<List<NewLife.RocketMQ.Protocol.MessageExt>> successCallback = async result =>
        {
            //Console.WriteLine("Operation succeeded. Result: " + result);
            //实际一次就一条消息
            foreach (var item in result)
            {
                //string msg = $"消息：msgId={item.MsgId},key={item.Keys},tags={item.Tags}，产生时间【{item.BornTimestamp.ToDateTime()}】，内容>需要序列化";
                PrintDataFileToPDFReq json = JsonConvert.DeserializeObject<PrintDataFileToPDFReq>(item.Body.ToStr());//反序列化对象
                //Console.WriteLine(msg);
                //Console.WriteLine($"消息JSON：{json.ToString}");

                if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                {
                    // 保存源文件到本地
                    string filetemppath = TempFileUtil.saveFileByUrl(json.fileUrl);
                    string lastFileName = ToPDF.getLastFileName(filetemppath);
                    ToPdfResp toPdfResp = null;
                    try
                    {
                        if (lastFileName.EndsWith("pdf"))
                        {
                            toPdfResp = ToPDF.ToPdfByPdf(filetemppath);

                        }
                        else if (lastFileName.EndsWith("doc") || lastFileName.EndsWith("docx"))
                        {
                            toPdfResp = ToPDF.ToPdfByWord(filetemppath);
                        }
                        else
                        {
                            //通用方法
                            toPdfResp = ToPDF.ToPdfByAny(filetemppath);

                        }
                    }catch (Exception ex)
                    {
                        try
                        {
                            // 捕获转换的异常，保证服务不会异常退出,一般造成转换异常说明该文件无法打印，直接返回错误信息即可!
                            TempFileUtil.removeFile(filetemppath);
                        }catch(Exception) { 
                        
                        }
                       
                        PrintDataFromPDFResp prsresp = new PrintDataFromPDFResp();
                        prsresp.id = json.id;
                        prsresp.message = "该文件无法转换为pdf，请检查文件格式再重试";
                        prsresp.status = 0;
                        RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                    }
                   
                    //最终结果
                    if (toPdfResp == null)
                    {
                        TempFileUtil.removeFile(filetemppath);
                        return;
                    }
                    else
                    {
                        //上传文件到上传链接里

                        using (var client = new HttpClient())
                        using (var fileStream = File.OpenRead(toPdfResp.pdfPath))
                        {
                            var content = new StreamContent(fileStream);
                            var response = await client.PutAsync(json.filePDFUploadUrl, content);

                            if (!response.IsSuccessStatusCode)
                            {
                                
                                TempFileUtil.removeFile(filetemppath);
                                TempFileUtil.removeFile(toPdfResp.pdfPath);
                                PrintDataFromPDFResp prsresp1 = new PrintDataFromPDFResp();
                                prsresp1.id = json.id;
                                prsresp1.message = "未知异常，请检查文件再次尝试";
                                prsresp1.status = 0;
                                RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp1), "resp");
                                return;

                            }
                        }

                        //返回消息，否者报错
                        PrintDataFromPDFResp prsresp = new PrintDataFromPDFResp();
                        prsresp.id = json.id;
                        prsresp.pageNums = toPdfResp.pdfPage;
                        prsresp.filePDFUrl = json.filePDFUrl;
                        prsresp.status = 1;
                        
                        RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                        TempFileUtil.removeFile(filetemppath);
                        TempFileUtil.removeFile(toPdfResp.pdfPath);
                        Console.Write("保存成功");
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
                    PrintDataFileToPDFReq json = JsonConvert.DeserializeObject<PrintDataFileToPDFReq>(item.Body.ToStr());//反序列化对象
                    //Console.WriteLine(msg);
                    //Console.WriteLine($"消息JSON：{json.ToString}");

                    if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                    {

                        PrintDataFromPDFResp prsresp = new PrintDataFromPDFResp();
                        prsresp.id = json.id;
                        prsresp.message = "未知异常，请检查文件再次尝试";
                        prsresp.status = 0;
                        RocketMQSendCenter.toPDFRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                    }
                }
                    //发送回执成功就消费消息
                    return true;
            }
            catch(Exception)
            {
                // 发生异常直接消费消息
                return true;
            }
            //只有能发送回执才算消费成功，回执都无法发送的消息就消费失败
            //return true;//如果发送回执消息成功就消费消息
            //否则失败
        };

    }
}
