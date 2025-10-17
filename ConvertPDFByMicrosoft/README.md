# ConvertPDFByMicrosoft 类库

这是一个将Office文件（Word、Excel、PowerPoint）和图片转换为PDF的类库。

## 功能特性

- 支持 Word 文件：`.doc`、`.docx`、`.docm`
- 支持 Excel 文件：`.xls`、`.xlsx`、`.xlsm`
- 支持 PowerPoint 文件：`.ppt`、`.pptx`、`.pptm`
- 支持图片文件：`.png`、`.jpg`、`.jpeg`、`.tif`、`.tiff`、`.bmp`

## 使用方法

### 1. 使用扩展方法（推荐）

```csharp
using ConvertPDFByMicrosoft;

// 转换单个文件
string sourceFile = @"C:\Documents\sample.docx";
string targetPdf = @"C:\Documents\sample.pdf";

bool success = sourceFile.ConvertToPdf(targetPdf);
```

### 2. 使用静态方法

```csharp
using ConvertPDFByMicrosoft;

// 转换单个文件
try
{
    bool success = Office2PdfConverter.Convert(
        @"C:\Documents\sample.xlsx", 
        @"C:\Documents\sample.pdf"
    );
    Console.WriteLine("转换成功！");
}
catch (Exception ex)
{
    Console.WriteLine($"转换失败: {ex.Message}");
}
```

### 3. 批量转换文件夹

```csharp
using ConvertPDFByMicrosoft;

// 批量转换文件夹中的所有支持的文件
var results = Office2PdfConverter.ConvertFolder(
    @"C:\Documents\InputFolder",
    @"C:\Documents\OutputFolder",  // 可选，默认为输入文件夹
    recursive: true  // 是否递归子文件夹
);

// 输出结果
foreach (var (sourceFile, success, errorMessage) in results)
{
    if (success)
    {
        Console.WriteLine($"? {sourceFile}");
    }
    else
    {
        Console.WriteLine($"? {sourceFile}: {errorMessage}");
    }
}
```

### 4. 检查文件格式

```csharp
using ConvertPDFByMicrosoft;

string extension = ".docx";
bool isSupported = Office2PdfConverter.IsSupportedFormat(extension);
```

## 系统要求

- .NET 8.0 或更高版本
- Windows 操作系统
- 已安装 Microsoft Office（Word、Excel、PowerPoint）
- 对于图片转换：需要"Microsoft Print to PDF"打印机（Windows 10/11 默认包含）

## 依赖项

- Microsoft.Office.Interop.Word
- Microsoft.Office.Interop.Excel
- System.Drawing.Common

## 注意事项

1. **Office 需要安装**：转换Office文件需要在系统上安装对应的Office应用程序
2. **COM对象释放**：库会自动处理COM对象的释放，无需手动管理
3. **PowerPoint转换**：PowerPoint转换使用反射技术，无需直接引用PowerPoint Interop程序集
4. **图片转换**：使用"Microsoft Print to PDF"打印机，自动处理横向/纵向
5. **异常处理**：建议在调用时使用try-catch捕获可能的异常

## 示例控制台应用

如果要创建一个控制台应用使用此类库：

```csharp
using ConvertPDFByMicrosoft;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("用法: 拖拽文件或文件夹到程序上");
            return;
        }

        foreach (var path in args)
        {
            if (Directory.Exists(path))
            {
                // 批量转换文件夹
                var results = Office2PdfConverter.ConvertFolder(path);
                foreach (var (file, success, error) in results)
                {
                    if (success)
                        Console.WriteLine($"? {file}");
                    else
                        Console.WriteLine($"? {file}: {error}");
                }
            }
            else if (File.Exists(path))
            {
                // 转换单个文件
                try
                {
                    var pdfPath = Path.ChangeExtension(path, ".pdf");
                    path.ConvertToPdf(pdfPath);
                    Console.WriteLine($"? {pdfPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? {path}: {ex.Message}");
                }
            }
        }

        Console.WriteLine("完成！");
    }
}
```

## 许可

请遵守 Microsoft Office 的使用许可协议。
