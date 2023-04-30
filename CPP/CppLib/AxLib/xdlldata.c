// dlldata.c 用のラッパー

#ifdef _MERGE_PROXYSTUB // proxy stub DLL の結合

#define REGISTER_PROXY_DLL //DllRegisterServer、他

#define USE_STUBLESS_PROXY	//MIDL のオプションで /Oicf を指定した場合のみ定義

#pragma comment(lib, "rpcns4.lib")
#pragma comment(lib, "rpcrt4.lib")

#define ENTRY_PREFIX	Prx

#include "dlldata.c"
#include "AxLib_p.c"

#endif //_MERGE_PROXYSTUB
