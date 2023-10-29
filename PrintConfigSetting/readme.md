# c++设置打印机配置函数

## 需要编译为对应平台的dll，放在printNode项目最终生成的bin目录下使用
### 得单独生成dll，没配置自动生成


只关系到PrintNode项目，转pdf和转图片不需要依赖此项目

```C#
[DllImport("PrintConfigSetting.dll")]
public static extern int Set(string deviceaName, int duplex, int printingDirection, int color);
```