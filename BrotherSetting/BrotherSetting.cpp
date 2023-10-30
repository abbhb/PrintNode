// BrotherSetting.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <windows.h>
#include <string.h>
#include <stdio.h>
#include <iostream>
int main(int argc, char** argv)
{
	HANDLE hPrinter = NULL;
	PRINTER_INFO_2* pi2 = NULL;
	DEVMODE* pDevMode = NULL;
	PRINTER_DEFAULTS pd;
	DWORD dwNeeded = 0;

	BOOL bFlag;
	LONG lFlag;

	if (argc != 4) {
		printf_s("参数不对,请先传入打印机名称，在传入单双面int，在传入打印方向int");
		int i;

		for (i = 0; i < argc; i++)

			printf("Argument %d is %s.\n", i, argv[i]);
		return 0;
	}
	// 获取默认打印机名称
	// 传入打印机名

	const char* charString123 = argv[2]; // 获取char参数

	// 1为单面 2双面 3双面商贩
	int myInteger1111 = std::atoi(charString123); // 使用 atoi 将 char 转换为整数

	if (myInteger1111 == 0 && charString123[1] != '0')
	{
		std::cerr << "无效的整数参数" << std::endl;
		return 0;
	}



	const char* asda22222 = argv[3]; // 获取char参数

	// 1纵向 2横向
	int myInteger2222 = std::atoi(asda22222); // 使用 atoi 将 char 转换为整数

	if (myInteger2222 == 0 && asda22222[0] != '0')
	{
		std::cerr << "无效的整数参数" << std::endl;
		return 0;
	}

	const char* charString1 = argv[0];
	int wcharSize = MultiByteToWideChar(CP_UTF8, 0, charString1, -1, nullptr, 0);

	if (wcharSize == 0)
	{
		std::cerr << "转换失败" << std::endl;
		return 0;
	}
	// 分配WCHAR缓冲区
	wchar_t* szDevName = new wchar_t[wcharSize];
	// 执行转换
	// 执行转换
	if (MultiByteToWideChar(CP_ACP, 0, charString1, -1, szDevName, wcharSize) == 0)
	{
		std::cerr << "转换失败" << std::endl;
		delete[] szDevName;
		return 0;
	}
	//WCHAR szDevName[MAX_PATH] = wcharString;
	DWORD dwLength = MAX_PATH;

	if (!GetDefaultPrinter(szDevName, &dwLength))
	{
		return 1;
	}

	ZeroMemory(&pd, sizeof(pd));
	pd.DesiredAccess = PRINTER_ALL_ACCESS;
	// 打开打印机
	bFlag = OpenPrinter(szDevName, &hPrinter, &pd);
	if (!bFlag || (hPrinter == NULL))
	{
		return 2;
	}
	// 获取pi2数据结构的尺寸
	bFlag = GetPrinter(hPrinter, 2, 0, 0, &dwNeeded);
	if ((!bFlag) && (GetLastError() != ERROR_INSUFFICIENT_BUFFER) || (dwNeeded == 0))
	{
		ClosePrinter(hPrinter);
		hPrinter = NULL;
		return 3;
	}

	pi2 = (PRINTER_INFO_2*)GlobalAlloc(GPTR, dwNeeded);
	if (pi2 == NULL)
	{
		ClosePrinter(hPrinter);
		hPrinter = NULL;
		return 4;
	}

	// 取得与指定打印机有关的信息PRINTER_INFO_2
	bFlag = GetPrinter(hPrinter, 2, (LPBYTE)pi2, dwNeeded, &dwNeeded);
	if (!bFlag)
	{
		GlobalFree(pi2);
		ClosePrinter(hPrinter);
		hPrinter = NULL;
		return 5;
	}

	// DEVMODE数据结构中包含了有关设备初始化和打印机环境的信息
	if (pi2->pDevMode == NULL)
	{
		// 获取pDevMode数据结构的尺寸
		dwNeeded = DocumentProperties(NULL, hPrinter,
			szDevName,
			NULL, NULL, 0);
		if (dwNeeded <= 0)
		{
			GlobalFree(pi2);
			ClosePrinter(hPrinter);
			hPrinter = NULL;
			return 6;
		}

		pDevMode = (DEVMODE*)GlobalAlloc(GPTR, dwNeeded);
		if (pDevMode == NULL)
		{
			GlobalFree(pi2);
			ClosePrinter(hPrinter);
			hPrinter = NULL;
			return 7;
		}

		// 输出打印机设置信息到pDevMode
		lFlag = DocumentProperties(NULL, hPrinter,
			szDevName,
			pDevMode, NULL,
			DM_OUT_BUFFER);
		if (lFlag != IDOK || pDevMode == NULL)
		{
			GlobalFree(pDevMode);
			GlobalFree(pi2);
			ClosePrinter(hPrinter);
			hPrinter = NULL;
			return 8;
		}

		pi2->pDevMode = pDevMode;
	}

	// 指定打印机的出纸匣，每台打印机的纸匣号不一样，我这边是，0自动选择, 1是纸匣1，2是纸匣2
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_DEFAULTSOURCE;
	int iBoxID = 0;
	pi2->pDevMode->dmDefaultSource = iBoxID;

	// 打印机使用的纸张尺寸(查找对应系统宏定义: DMPAPER_A4  9)
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_PAPERSIZE;
	int iPageSize = DMPAPER_A4;
	pi2->pDevMode->dmPaperSize = iPageSize;

	// 纸张类型，根据不同打印机有不同配置，我这边普通纸张是284，纸张保持默认
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_MEDIATYPE;
	//int iMediaType = 284;
	//pi2->pDevMode->dmMediaType = iMediaType;
	// 纵向
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_ORIENTATION;
	if (myInteger2222 != 1) {
		pi2->pDevMode->dmOrientation = DMORIENT_LANDSCAPE;
	}
	else {
		pi2->pDevMode->dmOrientation = DMORIENT_PORTRAIT;

	}

	// 双面打印模式
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_DUPLEX;
	int iOffset = pi2->pDevMode->dmSize;
	LPBYTE lpByteDevMode = (LPBYTE)pi2->pDevMode;
	// 是否双面(1单面 / 2双面)
	int iDuplex = myInteger1111;
	if (iDuplex == 1)
	{
		pi2->pDevMode->dmDuplex = 1;
		lpByteDevMode[iOffset + 84] = 1;
		lpByteDevMode[iOffset + 86] = 0;
	}
	else if (iDuplex == 2)
	{
		pi2->pDevMode->dmDuplex = 2;
		lpByteDevMode[iOffset + 84] = 2;
		lpByteDevMode[iOffset + 86] = 1;
	}
	else if (iDuplex == 3)
	{
		pi2->pDevMode->dmDuplex = 2;
		lpByteDevMode[iOffset + 84] = 2;
		lpByteDevMode[iOffset + 86] = 1;
	}

	// 彩色打印机颜色设置
	pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_COLOR;
	//1单色, 2彩色打印
	int	iColor = 1;
	if (iColor == 1)
	{
		pi2->pDevMode->dmColor = DMCOLOR_MONOCHROME;
	}
	else if (iColor == 2)
	{
		pi2->pDevMode->dmColor = DMCOLOR_COLOR;
	}

	pi2->pSecurityDescriptor = NULL;

	// 将修改后的pDevMode载入打印机驱动程序的新位置
	lFlag = DocumentProperties(NULL, hPrinter,
		szDevName,
		pi2->pDevMode,
		pi2->pDevMode,
		DM_IN_BUFFER | DM_OUT_BUFFER);
	if (lFlag != IDOK)
	{
		GlobalFree(pi2);
		ClosePrinter(hPrinter);
		if (pDevMode)
		{
			GlobalFree(pDevMode);
		}
		hPrinter = NULL;
		return 10;
	}

	// 将pi2载入打印机
	bFlag = SetPrinter(hPrinter, 2, (LPBYTE)pi2, 0);
	if (!bFlag)
	{
		GlobalFree(pi2);
		ClosePrinter(hPrinter);
		if (pDevMode)
		{
			GlobalFree(pDevMode);
		}
		hPrinter = NULL;
		return 11;
	}

	// 使打印机配置修改生效
	SendMessageTimeout(HWND_BROADCAST, WM_DEVMODECHANGE,
		0L,
		(LPARAM)(LPCSTR)szDevName,
		SMTO_NORMAL, 2000, NULL);

	//关闭打印机释放资源
	if (hPrinter != NULL)
	{
		int iC = ClosePrinter(hPrinter);
		hPrinter = NULL;

		if (pi2 != NULL)
		{
			GlobalFree(pi2);
			pi2 = NULL;
		}

		if (pDevMode != NULL)
		{
			GlobalFree(pDevMode);
			pDevMode = NULL;
		}
	}

	return 0;
}

// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
