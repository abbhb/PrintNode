#pragma once

#include <stdio.h>
#include <stdlib.h>
#include <iostream>


using namespace std;

// duplex:0/1���� 2˫�� 3˫���Ϸ�
//printingDirection:0��ֱ 1����
//color :0����ɫ 1����ɫ���ÿ���ӡ���Ƿ�֧��

//return 1�ɹ���0ʧ��
extern "C" __declspec(dllexport) int PrintSet(string deviceaName,int duplex, int printingDirection,int color);

extern "C" __declspec(dllexport) int Testasdd(int a, int b);

class PrintConfigSetting
{
public:
};

