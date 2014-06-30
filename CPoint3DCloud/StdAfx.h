// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if !defined(AFX_STDAFX_H__808046AA_B168_44DB_A84F_18E9F62DF76B__INCLUDED_)
#define AFX_STDAFX_H__808046AA_B168_44DB_A84F_18E9F62DF76B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT


#include "COpenGL.h"
#include "CPoint3D.h"

#ifndef _DEBUG  // debug version in MyViewView.cpp
  #pragma comment(lib,"COpenGLd.lib")//
#else
  #pragma comment(lib,"COpenGL.lib")//
#endif

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__808046AA_B168_44DB_A84F_18E9F62DF76B__INCLUDED_)
