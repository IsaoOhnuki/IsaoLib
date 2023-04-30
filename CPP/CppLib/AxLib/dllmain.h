// dllmain.h : モジュール クラスの宣言です。

class CAxLibModule : public ATL::CAtlDllModuleT< CAxLibModule >
{
public :
	DECLARE_LIBID(LIBID_AxLibLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_AXLIB, "{0130d584-c3c4-45a6-a0bf-8bad22747ef3}")
};

extern class CAxLibModule _AtlModule;
