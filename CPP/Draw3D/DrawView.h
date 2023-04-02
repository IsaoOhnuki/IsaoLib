// DrawView.h : DrawView クラスの宣言

#pragma once

#include <atlhandlerimpl.h>

using namespace ATL;

class DrawView : public CAtlDocumentImpl
{
public:
	DrawView(void)
	{
	}

	virtual ~DrawView(void)
	{
	}

	virtual HRESULT LoadFromStream(IStream* pStream, DWORD grfMode);
	virtual void InitializeSearchContent();

protected:
	void SetSearchContent(CString& value);
	virtual void OnDrawThumbnail(HDC hDrawDC, LPRECT lprcBounds);
};
