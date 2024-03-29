﻿// dllmain.cpp : DllMain の実装です。

#include "pch.h"
#include "framework.h"
#include "resource.h"
#include "AxLib_i.h"
#include "dllmain.h"
#include "xdlldata.h"

CAxLibModule _AtlModule;

// DLL エントリ ポイント
extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
#ifdef _MERGE_PROXYSTUB
	if (!PrxDllMain(hInstance, dwReason, lpReserved))
		return FALSE;
#endif
	hInstance;
	return _AtlModule.DllMain(dwReason, lpReserved);
}
