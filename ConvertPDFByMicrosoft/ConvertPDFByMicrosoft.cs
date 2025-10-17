using System;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace ConvertPDFByMicrosoft
{
    /// <summary>
    /// 文件转PDF扩展方法
    /// </summary>
    public static class FileConversionExtensions
    {
        /// <summary>
        /// 将文件转换为PDF（扩展方法）
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="targetPdfPath">目标PDF路径</param>
        /// <returns>转换是否成功</returns>
        public static bool ConvertToPdf(this string sourceFilePath, string targetPdfPath)
        {
            return Office2PdfConverter.Convert(sourceFilePath, targetPdfPath);
        }
    }

    /// <summary>
    /// Office文件和图片转PDF转换器
    /// </summary>
    public class Office2PdfConverter
    {
        /// <summary>
        /// 转换文件为PDF
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="targetPdfPath">目标PDF路径</param>
        /// <returns>转换是否成功</returns>
        public static bool Convert(string sourceFilePath, string targetPdfPath)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("源文件不存在", sourceFilePath);
            }

            var ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            
            try
            {
                switch (ext)
                {
                    case ".xlsx":
                    case ".xlsm":
                    case ".xls":
                        Excel2Pdf(sourceFilePath, targetPdfPath);
                        break;
                    case ".pptx":
                    case ".pptm":
                    case ".ppt":
                        Ppt2Pdf(sourceFilePath, targetPdfPath);
                        break;
                    case ".docx":
                    case ".docm":
                    case ".doc":
                        Word2Pdf(sourceFilePath, targetPdfPath);
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".tif":
                    case ".tiff":
                    case ".bmp":
                        Image2Pdf(sourceFilePath, targetPdfPath);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的文件格式: {ext}");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 批量转换文件夹中的文件
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="outputFolder">输出文件夹（可选，默认为源文件夹）</param>
        /// <param name="recursive">是否递归子文件夹</param>
        /// <returns>转换结果列表（文件路径, 是否成功, 错误信息）</returns>
        public static List<(string SourceFile, bool Success, string? ErrorMessage)> ConvertFolder(
            string folderPath, 
            string? outputFolder = null, 
            bool recursive = true)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"文件夹不存在: {folderPath}");
            }

            outputFolder ??= folderPath;
            var results = new List<(string, bool, string?)>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var file in Directory.GetFiles(folderPath, "*.*", searchOption))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!IsSupportedFormat(ext))
                {
                    continue;
                }

                try
                {
                    var relativePath = Path.GetRelativePath(folderPath, file);
                    var targetPath = Path.Combine(outputFolder, Path.ChangeExtension(relativePath, ".pdf"));
                    
                    // 确保目标文件夹存在
                    var targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    Convert(file, targetPath);
                    results.Add((file, true, null));
                }
                catch (Exception ex)
                {
                    results.Add((file, false, ex.Message));
                }
            }

            return results;
        }

        /// <summary>
        /// 检查是否为支持的文件格式
        /// </summary>
        public static bool IsSupportedFormat(string extension)
        {
            var ext = extension.ToLowerInvariant();
            return ext switch
            {
                ".xlsx" or ".xlsm" or ".xls" => true,
                ".pptx" or ".pptm" or ".ppt" => true,
                ".docx" or ".docm" or ".doc" => true,
                ".png" or ".jpg" or ".jpeg" or ".tif" or ".tiff" or ".bmp" => true,
                _ => false
            };
        }

        #region Office 三件套原生导出
        private static void Excel2Pdf(string xls, string pdf)
        {
            Excel.Application? app = null;
            Excel.Workbook? wb = null;
            try
            {
                app = new Excel.Application { Visible = false, DisplayAlerts = false };
                wb = app.Workbooks.Open(xls);
                wb.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, pdf);
                wb.Close(false);
            }
            finally
            {
                if (wb != null) Marshal.ReleaseComObject(wb);
                if (app != null)
                {
                    app.Quit();
                    Marshal.ReleaseComObject(app);
                }
            }
        }

        private static void Ppt2Pdf(string ppt, string pdf)
        {
            object? app = null;
            object? pres = null;
            try
            {
                // 使用反射创建PowerPoint应用程序
                Type? pptAppType = Type.GetTypeFromProgID("PowerPoint.Application");
                if (pptAppType == null)
                {
                    throw new Exception("PowerPoint未安装或无法访问");
                }
                
                app = Activator.CreateInstance(pptAppType);
                if (app == null)
                {
                    throw new Exception("无法创建PowerPoint应用程序实例");
                }
                
                // 设置Visible属性为false
                pptAppType.InvokeMember("Visible",
                    System.Reflection.BindingFlags.SetProperty,
                    null, app, new object[] { 0 }); // 0 = msoFalse
                
                // 获取Presentations集合
                object presentations = pptAppType.InvokeMember("Presentations",
                    System.Reflection.BindingFlags.GetProperty,
                    null, app, null) ?? throw new Exception("无法获取Presentations集合");
                
                // 打开演示文稿
                pres = presentations.GetType().InvokeMember("Open",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, presentations,
                    new object[] { ppt, 0, 0, 0 }); // msoFalse = 0
                
                if (pres == null)
                {
                    throw new Exception("无法打开PowerPoint文件");
                }
                
                // 导出为PDF
                pres.GetType().InvokeMember("ExportAsFixedFormat",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, pres,
                    new object[] { pdf, 2, 1 }); // ppFixedFormatTypePDF = 2, ppFixedFormatIntentPrint = 1
                
                // 关闭演示文稿
                pres.GetType().InvokeMember("Close",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, pres, null);
            }
            finally
            {
                if (pres != null) Marshal.ReleaseComObject(pres);
                if (app != null)
                {
                    // 退出应用程序
                    app.GetType().InvokeMember("Quit",
                        System.Reflection.BindingFlags.InvokeMethod,
                        null, app, null);
                    Marshal.ReleaseComObject(app);
                }
            }
        }

        private static void Word2Pdf(string doc, string pdf)
        {
            Word.Application? app = null;
            Word.Document? wd = null;
            try
            {
                app = new Word.Application { Visible = false };
                wd = app.Documents.Open(doc, ReadOnly: true, AddToRecentFiles: false);
                wd.ExportAsFixedFormat(pdf, Word.WdExportFormat.wdExportFormatPDF);
                wd.Close(false);
            }
            finally
            {
                if (wd != null) Marshal.ReleaseComObject(wd);
                if (app != null)
                {
                    app.Quit(false);
                    Marshal.ReleaseComObject(app);
                }
            }
        }
        #endregion

        #region 图片 → 打印到 PDF
        private static void Image2Pdf(string img, string pdf)
        {
            // 先加载图片以确定方向
            using var bmp = System.Drawing.Image.FromFile(img);
            
            // 利用 .NET 自带的 PrintDocument 把图片画到打印机
            using var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = pdf;
            pd.DocumentName = Path.GetFileName(img);
            
            // 根据图片宽高比决定页面方向（在打印前设置）
            bool imageIsLandscape = bmp.Width > bmp.Height;
            pd.DefaultPageSettings.Landscape = imageIsLandscape;
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            
            pd.PrintPage += (s, e) =>
            {
                if (e.Graphics == null) return;
                
                // 直接使用 PageBounds，系统会自动处理横向/纵向
                var pageWidth = e.PageBounds.Width;
                var pageHeight = e.PageBounds.Height;
                
                // 计算缩放比例，保持宽高比
                float scaleWidth = pageWidth / (float)bmp.Width;
                float scaleHeight = pageHeight / (float)bmp.Height;
                float scale = Math.Min(scaleWidth, scaleHeight);
                
                // 计算缩放后的尺寸
                int scaledWidth = (int)(bmp.Width * scale);
                int scaledHeight = (int)(bmp.Height * scale);
                
                // 居中显示
                int x = (pageWidth - scaledWidth) / 2;
                int y = (pageHeight - scaledHeight) / 2;
                
                // 绘制图片
                e.Graphics.DrawImage(bmp, x, y, scaledWidth, scaledHeight);
                e.HasMorePages = false;
            };
            
            pd.Print();
        }
        #endregion
    }
}
