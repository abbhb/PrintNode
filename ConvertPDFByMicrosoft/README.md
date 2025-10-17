# ConvertPDFByMicrosoft ���

����һ����Office�ļ���Word��Excel��PowerPoint����ͼƬת��ΪPDF����⡣

## ��������

- ֧�� Word �ļ���`.doc`��`.docx`��`.docm`
- ֧�� Excel �ļ���`.xls`��`.xlsx`��`.xlsm`
- ֧�� PowerPoint �ļ���`.ppt`��`.pptx`��`.pptm`
- ֧��ͼƬ�ļ���`.png`��`.jpg`��`.jpeg`��`.tif`��`.tiff`��`.bmp`

## ʹ�÷���

### 1. ʹ����չ�������Ƽ���

```csharp
using ConvertPDFByMicrosoft;

// ת�������ļ�
string sourceFile = @"C:\Documents\sample.docx";
string targetPdf = @"C:\Documents\sample.pdf";

bool success = sourceFile.ConvertToPdf(targetPdf);
```

### 2. ʹ�þ�̬����

```csharp
using ConvertPDFByMicrosoft;

// ת�������ļ�
try
{
    bool success = Office2PdfConverter.Convert(
        @"C:\Documents\sample.xlsx", 
        @"C:\Documents\sample.pdf"
    );
    Console.WriteLine("ת���ɹ���");
}
catch (Exception ex)
{
    Console.WriteLine($"ת��ʧ��: {ex.Message}");
}
```

### 3. ����ת���ļ���

```csharp
using ConvertPDFByMicrosoft;

// ����ת���ļ����е�����֧�ֵ��ļ�
var results = Office2PdfConverter.ConvertFolder(
    @"C:\Documents\InputFolder",
    @"C:\Documents\OutputFolder",  // ��ѡ��Ĭ��Ϊ�����ļ���
    recursive: true  // �Ƿ�ݹ����ļ���
);

// ������
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

### 4. ����ļ���ʽ

```csharp
using ConvertPDFByMicrosoft;

string extension = ".docx";
bool isSupported = Office2PdfConverter.IsSupportedFormat(extension);
```

## ϵͳҪ��

- .NET 8.0 ����߰汾
- Windows ����ϵͳ
- �Ѱ�װ Microsoft Office��Word��Excel��PowerPoint��
- ����ͼƬת������Ҫ"Microsoft Print to PDF"��ӡ����Windows 10/11 Ĭ�ϰ�����

## ������

- Microsoft.Office.Interop.Word
- Microsoft.Office.Interop.Excel
- System.Drawing.Common

## ע������

1. **Office ��Ҫ��װ**��ת��Office�ļ���Ҫ��ϵͳ�ϰ�װ��Ӧ��OfficeӦ�ó���
2. **COM�����ͷ�**������Զ�����COM������ͷţ������ֶ�����
3. **PowerPointת��**��PowerPointת��ʹ�÷��似��������ֱ������PowerPoint Interop����
4. **ͼƬת��**��ʹ��"Microsoft Print to PDF"��ӡ�����Զ��������/����
5. **�쳣����**�������ڵ���ʱʹ��try-catch������ܵ��쳣

## ʾ������̨Ӧ��

���Ҫ����һ������̨Ӧ��ʹ�ô���⣺

```csharp
using ConvertPDFByMicrosoft;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("�÷�: ��ק�ļ����ļ��е�������");
            return;
        }

        foreach (var path in args)
        {
            if (Directory.Exists(path))
            {
                // ����ת���ļ���
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
                // ת�������ļ�
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

        Console.WriteLine("��ɣ�");
    }
}
```

## ���

������ Microsoft Office ��ʹ�����Э�顣
