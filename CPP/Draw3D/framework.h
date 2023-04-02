#pragma once

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_APARTMENT_THREADED

#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// 一部の CString コンストラクターは明示的です。

#ifdef _MANAGED
#error ファイルの種類のハンドラーをマネージド アセンブリとしてビルドすることはできません。プロジェクト プロパティで共通言語ランタイム オプションを CLR サポートなしに設定してください。
#endif

#ifndef _UNICODE
#error ファイルの種類のハンドラーは Unicode でビルドする必要があります。プロジェクト プロパティで文字セット オプションを Unicode に設定してください。
#endif

#define SHARED_HANDLERS


#define ATL_NO_ASSERT_ON_DESTROY_NONEXISTENT_WINDOW

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>
