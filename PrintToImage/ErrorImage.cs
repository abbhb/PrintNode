using System.Drawing.Imaging;
using System.Drawing;

namespace PrintToImage
{
    public class ErrorImage
    {
        private static string errorImageFilePath = "temp\\error.png";

        public static string getErrorImagePath()
        {
            if (File.Exists(errorImageFilePath))
            {
                return errorImageFilePath;
            }
            //先产生在返回
            //制作文字
            // 创建一个Bitmap对象，设置图片的宽度和高度
            using (Bitmap bitmap = new Bitmap(200, 100))
            {
                // 创建一个Graphics对象来绘制文本
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // 设置背景颜色
                    graphics.Clear(Color.White);

                    // 创建一个字体和画刷
                    System.Drawing.Font font = new System.Drawing.Font("Arial", 12);
                    SolidBrush brush = new SolidBrush(Color.Black);

                    // 在图片上绘制文本
                    graphics.DrawString("获取缩略图错误", font, brush, new PointF(10, 10));

                    // 保存图片为PNG文件
                    bitmap.Save(errorImageFilePath, ImageFormat.Png);
                }
            }
            return errorImageFilePath;

        }
    }
}
