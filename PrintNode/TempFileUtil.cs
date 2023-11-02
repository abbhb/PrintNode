using Microsoft.VisualBasic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace PrintNode
{
    public class TempFileUtil
    {
        
        public static string tempPath = System.IO.Directory.GetCurrentDirectory()+"\\"+ "temp\\";
        public static void isHavePath()
        {

            //判断文件夹是否存在
            if (!Directory.Exists(tempPath))
            {
                //创建文件夹
                try
                {
                    Directory.CreateDirectory(tempPath);
                }
                catch (Exception e)
                {
                }
            }
        }
        private static string GetUrlName(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetRandomFileName();
            string filenameLast = HttpUtility.UrlDecode(uri.Segments.Last());
            return fileName + "."+ filenameLast;
        }
       
        public static string saveFileByUrl(string url)
        {
            var fileName = GetUrlName(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.DownloadFile(url, tempPath+fileName);
            return fileName;
        }

        public static string saveFileByName(string url,string name)
        {
            string zhenshiname = "";
            // var fileName = GetUrlName(url);
            if (File.Exists(tempPath + name))
            {
                // 生成一个新的UUID
                Guid newGuid = Guid.NewGuid();

                // 获取UUID的字节数组
                byte[] guidBytes = newGuid.ToByteArray();

                // 取前四位字节（32位）
                byte[] firstFourBytes = new byte[4];
                Array.Copy(guidBytes, firstFourBytes, 4);

                // 将前四位字节转换为整数
                int result = BitConverter.ToInt32(firstFourBytes, 0);
                zhenshiname = name + result;
            }
            else
            {
                zhenshiname = name;
            }
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.DownloadFile(url, tempPath + zhenshiname);
            return zhenshiname;
        }

        public static void removeFile(string filePath)
        {
            //这样就不会导致异常抛出
            try
            {
                if (File.Exists(filePath))
                {
                    //文件存在
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除文件时发生错误: " + ex.Message);
            }
        }
    }
}
