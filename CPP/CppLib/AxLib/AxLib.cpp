// AxLib.cpp : DLL エクスポートの実装。


#include "pch.h"
#include "framework.h"
#include "resource.h"
#include "AxLib_i.h"
#include "dllmain.h"
#include "xdlldata.h"


using namespace ATL;

// DLL を OLE によってアンロードできるようにするかどうかを指定します。
_Use_decl_annotations_
STDAPI DllCanUnloadNow(void)
{
#ifdef _MERGE_PROXYSTUB
	HRESULT hr = PrxDllCanUnloadNow();
	if (hr != S_OK)
		return hr;
#endif
	return _AtlModule.DllCanUnloadNow();
}

// 要求された型のオブジェクトを作成するクラス ファクトリを返します。
_Use_decl_annotations_
STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv)
{
#ifdef _MERGE_PROXYSTUB
	HRESULT hr = PrxDllGetClassObject(rclsid, riid, ppv);
	if (hr != CLASS_E_CLASSNOTAVAILABLE)
		return hr;
#endif
	return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}

// DllRegisterServer - エントリをシステム レジストリに追加します。
_Use_decl_annotations_
STDAPI DllRegisterServer(void)
{
	// オブジェクト、タイプ ライブラリおよびタイプ ライブラリ内のすべてのインターフェイスを登録します
	HRESULT hr = _AtlModule.DllRegisterServer();
#ifdef _MERGE_PROXYSTUB
	if (FAILED(hr))
		return hr;
	hr = PrxDllRegisterServer();
#endif
	return hr;
}

// DllUnregisterServer - エントリをシステム レジストリから削除します。
_Use_decl_annotations_
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
#ifdef _MERGE_PROXYSTUB
	if (FAILED(hr))
		return hr;
	hr = PrxDllRegisterServer();
	if (FAILED(hr))
		return hr;
	hr = PrxDllUnregisterServer();
#endif
	return hr;
}

// DllInstall - ユーザーおよびコンピューターごとのシステム レジストリ エントリを追加または削除します。
STDAPI DllInstall(BOOL bInstall, _In_opt_  LPCWSTR pszCmdLine)
{
	HRESULT hr = E_FAIL;
	static const wchar_t szUserSwitch[] = L"user";

	if (pszCmdLine != nullptr)
	{
		if (_wcsnicmp(pszCmdLine, szUserSwitch, _countof(szUserSwitch)) == 0)
		{
			ATL::AtlSetPerUserRegistration(true);
		}
	}

	if (bInstall)
	{
		hr = DllRegisterServer();
		if (FAILED(hr))
		{
			DllUnregisterServer();
		}
	}
	else
	{
		hr = DllUnregisterServer();
	}

	return hr;
}


