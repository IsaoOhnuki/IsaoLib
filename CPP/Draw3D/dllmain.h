// dllmain.h : モジュール クラスの宣言です。

class CDraw3DModule : public ATL::CAtlDllModuleT< CDraw3DModule >
{
public :
	DECLARE_LIBID(LIBID_Draw3DLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_DRAW3D, "{013059f3-606b-49de-8822-6c71cada6c4d}")
};

extern class CDraw3DModule _AtlModule;
