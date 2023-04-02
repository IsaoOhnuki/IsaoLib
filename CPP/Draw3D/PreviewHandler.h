// PreviewHandler.h : プレビュー ハンドラーの宣言です

#pragma once
#include "resource.h"       // メイン シンボル

#define AFX_PREVIEW_STANDALONE
#include <atlhandler.h>
#include <atlhandlerimpl.h>
#include "DrawView.h"
#include <atlpreviewctrlimpl.h>

#include "Draw3D_i.h"

using namespace ATL;

// CPreviewCtrl 実装
class CPreviewCtrl : public CAtlPreviewCtrlImpl
{
protected:
	virtual void DoPaint(HDC hdc)
	{
		// 次の要領で、IDocument へのポインターを取得できます
		// CMyDoc* pDoc = (CMyDoc*)m_pDocument;
		CString strData = _T("リッチ プレビュー コンテンツをここに描画します。");
		TextOut(hdc, 10, 20, strData, strData.GetLength());
	}
};

// CPreviewHandler

class ATL_NO_VTABLE CPreviewHandler :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CPreviewHandler, &CLSID_Preview>,
	public CPreviewHandlerImpl <CPreviewHandler>
{
public:
	CPreviewHandler()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_PREVIEW_HANDLER)
DECLARE_NOT_AGGREGATABLE(CPreviewHandler)

BEGIN_COM_MAP(CPreviewHandler)
	COM_INTERFACE_ENTRY(IObjectWithSite)
	COM_INTERFACE_ENTRY(IOleWindow)
	COM_INTERFACE_ENTRY(IInitializeWithStream)
	COM_INTERFACE_ENTRY(IPreviewHandler)
	COM_INTERFACE_ENTRY(IPreviewHandlerVisuals)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		CPreviewHandlerImpl<CPreviewHandler>::FinalRelease();
	}

protected:
	virtual IPreviewCtrl* CreatePreviewControl()
	{
		// このクラスは、このヘッダーの先頭に定義されています
		CPreviewCtrl *pPreviewCtrl = nullptr;
		ATLTRY(pPreviewCtrl = new CPreviewCtrl());
		return pPreviewCtrl;
	}

	virtual IDocument* CreateDocument()
	{
		DrawView *pDocument = nullptr;
		ATLTRY(pDocument = new DrawView());
		return pDocument;
	}

};

OBJECT_ENTRY_AUTO(__uuidof(Preview), CPreviewHandler)
