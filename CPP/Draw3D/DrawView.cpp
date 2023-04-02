// DrawView.cpp : DrawView クラスの実装

#include "pch.h"
#include "framework.h"
#include <propkey.h>
#include "DrawView.h"

HRESULT DrawView::LoadFromStream(IStream* pStream, DWORD grfMode)
{
	// ドキュメント データをストリームから読み込みます
	UNREFERENCED_PARAMETER(pStream);
	UNREFERENCED_PARAMETER(grfMode);
	return S_OK;
}

void DrawView::InitializeSearchContent()
{
	// ドキュメントのデータからの検索コンテンツを、次の値で初期化します
	CString value = _T("test;content;");
	SetSearchContent(value);
}

void DrawView::SetSearchContent(CString& value)
{
	// 検索コンテンツに PKEY_Search_Contents キーを割り当てます
	if (value.IsEmpty())
	{
		RemoveChunk(PKEY_Search_Contents.fmtid, PKEY_Search_Contents.pid);
	}
	else
	{
		CFilterChunkValueImpl *pChunk = nullptr;
		ATLTRY(pChunk = new CFilterChunkValueImpl);
		if (pChunk != nullptr)
		{
			pChunk->SetTextValue(PKEY_Search_Contents, value, CHUNK_TEXT);
			SetChunkValue(pChunk);
		}
	}
}

void DrawView::OnDrawThumbnail(HDC hDrawDC, LPRECT lprcBounds)
{
	HBRUSH hDrawBrush = CreateSolidBrush(RGB(255, 255, 255));
	FillRect(hDrawDC, lprcBounds, hDrawBrush);

	HFONT hStockFont = (HFONT) GetStockObject(DEFAULT_GUI_FONT);
	LOGFONT lf;

	GetObject(hStockFont, sizeof(LOGFONT), &lf);
	lf.lfHeight = 34;

	HFONT hDrawFont = CreateFontIndirect(&lf);
	HFONT hOldFont = (HFONT) SelectObject(hDrawDC, hDrawFont);

	CString strText = _T("TODO:ここで縮小版描画を実装します");
	DrawText(hDrawDC, strText, strText.GetLength(), lprcBounds, DT_CENTER | DT_WORDBREAK);

	SelectObject(hDrawDC, hDrawFont);
	SelectObject(hDrawDC, hOldFont);

	DeleteObject(hDrawBrush);
	DeleteObject(hDrawFont);
}
