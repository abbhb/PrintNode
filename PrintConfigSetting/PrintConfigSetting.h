#pragma once

#include <stdio.h>
#include <stdlib.h>
#include <iostream>


using namespace std;

// duplex:0/1单面 2双面 3双面上翻
//printingDirection:0竖直 1横向
//color :0：单色 1：彩色，得看打印机是否支持

//return 1成功，0失败
extern "C" __declspec(dllexport) int PrintSet(string deviceaName,int duplex, int printingDirection,int color);

extern "C" __declspec(dllexport) int Testasdd(int a, int b);

class PrintConfigSetting
{
public:
};

