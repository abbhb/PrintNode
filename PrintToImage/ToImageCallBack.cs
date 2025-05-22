
using NewLife;
using Newtonsoft.Json;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Drawing2D;
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
                            // 配置参数
                            const int gridSize = 3;           // 3x3网格
                            const int cellSize = 600;         // 每个单元格600x600
                            const int totalSize = cellSize * gridSize; // 总尺寸1800x1800
                            const int renderDPI = 300;        // 高精度渲染DPI
                            const int padding = 4;            // 单元格内边距

                            int pageCount = Math.Min(pdfDocument.PageCount, 9);

                            using (Bitmap resultImage = new Bitmap(totalSize, totalSize))
                            using (Graphics graphics = Graphics.FromImage(resultImage))
                            {
                                graphics.Clear(Color.White);
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                                for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                                {
                                    // 获取PDF原始页面尺寸（单位：英寸）
                                    var pageSize = pdfDocument.PageSizes[pageIndex];

                                    // 计算缩放比例（保留原始宽高比）
                                    float maxContentSize = cellSize - padding * 2;
                                    float scale = Math.Min(
                                        maxContentSize / (pageSize.Width * renderDPI / 72f),
                                        maxContentSize / (pageSize.Height * renderDPI / 72f)
                                    );

                                    // 计算实际渲染尺寸
                                    int renderWidth = (int)(pageSize.Width * renderDPI / 72f);
                                    int renderHeight = (int)(pageSize.Height * renderDPI / 72f);

                                    // 高质量渲染原始页面
                                    using (var pageImage = (Bitmap)pdfDocument.Render(
                                        pageIndex,
                                        renderWidth,
                                        renderHeight,
                                        renderDPI,
                                        renderDPI,
                                        true))
                                    {
                                        // 计算目标尺寸（保持比例）
                                        int targetWidth = (int)(renderWidth * scale);
                                        int targetHeight = (int)(renderHeight * scale);

                                        // 计算绘制位置（居中显示）
                                        int row = pageIndex / gridSize;
                                        int col = pageIndex % gridSize;
                                        int x = col * cellSize + (cellSize - targetWidth) / 2;
                                        int y = row * cellSize + (cellSize - targetHeight) / 2;

                                        // 高质量缩放
                                        using (var scaledImage = new Bitmap(targetWidth, targetHeight))
                                        using (var g = Graphics.FromImage(scaledImage))
                                        {
                                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            g.DrawImage(pageImage, 0, 0, targetWidth, targetHeight);
                                            graphics.DrawImage(scaledImage, x, y);
                                        }
                                    }
                                }

                                // 保存最终图片
                                // 保存拼接后的图像
                                resultImage.Save(TempFileUtil.tempPath + $"result_image_{json.id}.png", ImageFormat.Png);
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
                        // 废弃 2025/05/22 19:24
                        //int baseWidth = 600;
                        //int baseHeight = 600;
                        //using (PdfDocument pdfDocument = PdfDocument.Load(pdfFilePath))
                        //{
                        //    int totalWidth = 3 * baseWidth;
                        //    int totalHeight =3 * baseHeight;
                        //    int pageCount = 9;
                        //    if (pageCount >= pdfDocument.PageCount)
                        //    {
                        //        pageCount = pdfDocument.PageCount;
                        //    }
                        //    using (Bitmap resultImage = new Bitmap(totalWidth, totalHeight))
                        //    using (Graphics graphics = Graphics.FromImage(resultImage))
                        //    {
                        //        graphics.Clear(Color.White);

                        //        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                        //        {
                        //            int row = pageIndex / 3;
                        //            int col = pageIndex % 3;

                        //            using (Bitmap pageThumbnail = (Bitmap)pdfDocument.Render(pageIndex, baseWidth, baseHeight, true))
                        //            {
                        //                graphics.DrawImage(pageThumbnail, col * baseWidth, row * baseHeight, baseWidth, totalHeight);
                        //            }
                        //        }
                                

                        //        // 保存拼接后的图像
                        //        resultImage.Save(TempFileUtil.tempPath + $"result_image_{json.id}.png", ImageFormat.Png);
                        //        //上传文件到上传链接里

                                

                        //    }
                        //}
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
