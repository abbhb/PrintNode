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

		// ��ȡĬ�ϴ�ӡ������
		// �����ӡ����

		WCHAR szDevName[MAX_PATH] =L"";
		MultiByteToWideChar(CP_UTF8, 0, deviceaName.c_str(), -1, szDevName, MAX_PATH);
		DWORD dwLength = MAX_PATH;
		if (!GetDefaultPrinter(szDevName, &dwLength))
		{
			return 1;
		}

		ZeroMemory(&pd, sizeof(pd));
		pd.DesiredAccess = PRINTER_ALL_ACCESS;
		// �򿪴�ӡ��
		bFlag = OpenPrinter(szDevName, &hPrinter, &pd);
		if (!bFlag || (hPrinter == NULL))
		{
			return 2;
		}
		// ��ȡpi2���ݽṹ�ĳߴ�
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

		// ȡ����ָ����ӡ���йص���ϢPRINTER_INFO_2
		bFlag = GetPrinter(hPrinter, 2, (LPBYTE)pi2, dwNeeded, &dwNeeded);
		if (!bFlag)
		{
			GlobalFree(pi2);
			ClosePrinter(hPrinter);
			hPrinter = NULL;
			return 5;
		}

		// DEVMODE���ݽṹ�а������й��豸��ʼ���ʹ�ӡ����������Ϣ
		if (pi2->pDevMode == NULL)
		{
			// ��ȡpDevMode���ݽṹ�ĳߴ�
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

			// �����ӡ��������Ϣ��pDevMode
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

		// ָ����ӡ���ĳ�ֽϻ��ÿ̨��ӡ����ֽϻ�Ų�һ����������ǣ�0�Զ�ѡ��, 1��ֽϻ1��2��ֽϻ2
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_DEFAULTSOURCE;
		int iBoxID = 0;
		pi2->pDevMode->dmDefaultSource = iBoxID;

		// ��ӡ��ʹ�õ�ֽ�ųߴ�(���Ҷ�Ӧϵͳ�궨��: DMPAPER_A4  9)
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_PAPERSIZE;
		int iPageSize = DMPAPER_A4;
		pi2->pDevMode->dmPaperSize = iPageSize;

		// ֽ�����ͣ����ݲ�ͬ��ӡ���в�ͬ���ã��������ֽͨ����284��ֽ�ű���Ĭ��
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_MEDIATYPE;
		//int iMediaType = 284;
		//pi2->pDevMode->dmMediaType = iMediaType;

		// ˫���ӡģʽ
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_DUPLEX;
		int iOffset = pi2->pDevMode->dmSize;
		LPBYTE lpByteDevMode = (LPBYTE)pi2->pDevMode;
		// �Ƿ�˫��(1���� / 2˫��)
		
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
			// 3Ŀǰ����һ��
			pi2->pDevMode->dmDuplex = 2;
			lpByteDevMode[iOffset + 84] = 2;
			lpByteDevMode[iOffset + 86] = 1;
		}

		// ��ɫ��ӡ����ɫ����
		pi2->pDevMode->dmFields = pi2->pDevMode->dmFields | DM_COLOR;
		//1��ɫ, 2��ɫ��ӡ
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

		// ���޸ĺ��pDevMode�����ӡ�������������λ��
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

		// ��pi2�����ӡ��
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

		// ʹ��ӡ�������޸���Ч
		SendMessageTimeout(HWND_BROADCAST, WM_DEVMODECHANGE,
			0L,
			(LPARAM)(LPCSTR)szDevName,
			SMTO_NORMAL, 1000, NULL);

		//�رմ�ӡ���ͷ���Դ
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

