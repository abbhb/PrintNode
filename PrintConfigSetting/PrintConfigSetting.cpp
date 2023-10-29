#include "PrintConfigSetting.h"
#include <stdlib.h>
#include <iostream>
#include <windows.h>
#include <string.h>

__declspec(dllexport) int Testasdd(int a, int b) {
	return a + b;
}



__declspec(dllexport) int PrintSet(string deviceaName,int duplex, int printingDirection, int color) {

	try
	{
		HANDLE hPrinter = NULL;
		PRINTER_INFO_2* pi2 = NULL;
		DEVMODE* pDevMode = NULL;
		PRINTER_DEFAULTS pd;
		DWORD dwNeeded = 0;

		BOOL bFlag;
		LONG lFlag;

		// 获取默认打印机名称
		// 传入打印机名

		WCHAR szDevName[MAX_PATH] =L"";
		MultiByteToWideChar(CP_UTF8, 0, deviceaName.c_str(), -1, szDevName, MAX_PATH);
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

		// 双面打印模式
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_DUPLEX;
		int iOffset = pi2->pDevMode->dmSize;
		LPBYTE lpByteDevMode = (LPBYTE)pi2->pDevMode;
		// 是否双面(1单面 / 2双面)
		
		int iDuplex = 2;
		if (duplex == 1|| duplex==0)
		{
			pi2->pDevMode->dmDuplex = 1;
			lpByteDevMode[iOffset + 84] = 1;
			lpByteDevMode[iOffset + 86] = 0;
		}
		else if (duplex == 2)
		{
			pi2->pDevMode->dmDuplex = 2;
			lpByteDevMode[iOffset + 84] = 2;
			lpByteDevMode[iOffset + 86] = 1;
		}
		else if (duplex == 3)
		{
			// 3目前保持一致
			pi2->pDevMode->dmDuplex = 2;
			lpByteDevMode[iOffset + 84] = 2;
			lpByteDevMode[iOffset + 86] = 1;
		}

		// 彩色打印机颜色设置
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_COLOR;
		//1单色, 2彩色打印
		int	iColor = 1;
		if (color == 0)
		{
			pi2->pDevMode->dmColor = DMCOLOR_MONOCHROME;
		}
		else if (color == 1)
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
			SMTO_NORMAL, 1000, NULL);

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
	}
	catch (const std::exception&)
	{
		return 0;
	}
	return 1;
}

