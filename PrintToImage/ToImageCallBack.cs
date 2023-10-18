
using NewLife;
using Newtonsoft.Json;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;

namespace PrintToImage
{
    public class ToImageCallBack
    {
        // 创建哈希映射 id,次数，要求超过3次就直接消费消息
        private static Dictionary<string, int> hashMap = new Dictionary<string, int>();
        // 定义回调函数和异常处理回调
        public static Action<List<NewLife.RocketMQ.Protocol.MessageExt>> successCallback = async result =>
        {
            //Console.WriteLine("Operation succeeded. Result: " + result);
            //实际一次就一条消息
            foreach (var item in result)
            {
                //string msg = $"消息：msgId={item.MsgId},key={item.Keys},tags={item.Tags}，产生时间【{item.BornTimestamp.ToDateTime()}】，内容>需要序列化";
                PrintDataPDFToImageReq json = JsonConvert.DeserializeObject<PrintDataPDFToImageReq>(item.Body.ToStr());//反序列化对象
                //Console.WriteLine(msg);
                //Console.WriteLine($"消息JSON：{json.ToString}");

                if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                {
                    // 保存PDF文件到本地
                    string filetemppath = TempFileUtil.saveFileByUrl(json.filePDFUrl);
                    //处理图片
                    if (File.Exists(filetemppath)&&(filetemppath.EndsWith("pdf")|| filetemppath.EndsWith("PDF")))
                    {
                        //真转
                        // 指定要处理的PDF文件路径
                        string pdfFilePath = filetemppath;

                        using (PdfDocument pdfDocument = PdfDocument.Load(pdfFilePath))
                        {
                            int totalWidth = 3 * 200;
                            int totalHeight = 3 * 200;
                            int pageCount = 9;
                            if (pageCount >= pdfDocument.PageCount)
                            {
                                pageCount = pdfDocument.PageCount;
                            }
                            using (Bitmap resultImage = new Bitmap(totalWidth, totalHeight))
                            using (Graphics graphics = Graphics.FromImage(resultImage))
                            {
                                graphics.Clear(Color.White);

                                for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                                {
                                    int row = pageIndex / 3;
                                    int col = pageIndex % 3;

                                    using (Bitmap pageThumbnail = (Bitmap)pdfDocument.Render(pageIndex, 200, 200, true))
                                    {
                                        graphics.DrawImage(pageThumbnail, col * 200, row * 200, 200, 200);
                                    }
                                }
                                

                                // 保存拼接后的图像
                                resultImage.Save(TempFileUtil.tempPath + $"result_image_{json.id}.png", ImageFormat.Png);
                                //上传文件到上传链接里

                                using (var client = new HttpClient())
                                using (var fileStream = File.OpenRead(TempFileUtil.tempPath + $"result_image_{json.id}.png"))
                                {
                                    var content = new StreamContent(fileStream);
                                    var response = await client.PutAsync(json.filePDFImageUploadUrl, content);
                                    //发送消息

                                    PrintDataImageFromPDFResp prsresp = new PrintDataImageFromPDFResp();
                                    prsresp.id = json.id;
                                    prsresp.status = 1;
                                    prsresp.filePDFImageUrl = json.filePDFImageUrl;
                                    RocketMQSendCenter.toImageRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");

                                    if (!response.IsSuccessStatusCode)
                                    {
                                        TempFileUtil.removeFile(TempFileUtil.tempPath + $"result_image_{json.id}.png");
                                        TempFileUtil.removeFile(json.filePDFUrl);
                                        throw new Exception("转换端文件上传失败，请重试！");
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        string errorImageFilePath = ErrorImage.getErrorImagePath();
                        //上传文件到上传链接里

                        using (var client = new HttpClient())
                        using (var fileStream = File.OpenRead(errorImageFilePath))
                        {
                            var content = new StreamContent(fileStream);
                            var response = await client.PutAsync(json.filePDFImageUploadUrl, content);

                            PrintDataImageFromPDFResp prsresp = new PrintDataImageFromPDFResp();
                            prsresp.id = json.id;
                            prsresp.status = 1;
                            prsresp.filePDFImageUrl = json.filePDFImageUrl;

                            RocketMQSendCenter.toImageRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
                            if (!response.IsSuccessStatusCode)
                            {
                                TempFileUtil.removeFile(json.filePDFUrl);
                                throw new Exception("转换端文件上传失败，请重试！");
                            }
                        }
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
                    PrintDataPDFToImageReq json = JsonConvert.DeserializeObject<PrintDataPDFToImageReq>(item.Body.ToStr());//反序列化对象
                    //Console.WriteLine(msg);
                    //Console.WriteLine($"消息JSON：{json.ToString}");

                    if (json != null && !StringHelper.IsNullOrEmpty(json.id))
                    {

                        PrintDataImageFromPDFResp prsresp = new PrintDataImageFromPDFResp();
                        prsresp.id = json.id;
                        prsresp.message = ex.exception.Message;
                        prsresp.status = 0;
                        RocketMQSendCenter.toImageRespSend.Publish(JsonConvert.SerializeObject(prsresp), "resp");
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
                PrintDataImageFromPDFResp errors = JsonConvert.DeserializeObject<PrintDataImageFromPDFResp>(ex.messageExts[0].Body.ToStr());//反序列化对象


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
