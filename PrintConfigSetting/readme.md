# c++���ô�ӡ�����ú���

## ��Ҫ����Ϊ��Ӧƽ̨��dll������printNode��Ŀ�������ɵ�binĿ¼��ʹ��
### �õ�������dll��û�����Զ�����


ֻ��ϵ��PrintNode��Ŀ��תpdf��תͼƬ����Ҫ��������Ŀ

```C#
[DllImport("PrintConfigSetting.dll")]
public static extern int Set(string deviceaName, int duplex, int printingDirection, int color);
```