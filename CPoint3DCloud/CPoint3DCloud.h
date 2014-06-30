// CPoint3DCloud.h : main header file for the CPOINT3DCLOUD application
//

#if !defined(AFX_CPOINT3DCLOUD_H__1C153336_7F0F_45FB_BDC4_896744C4C2E1__INCLUDED_)
#define AFX_CPOINT3DCLOUD_H__1C153336_7F0F_45FB_BDC4_896744C4C2E1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudApp:
// See CPoint3DCloud.cpp for the implementation of this class
//

class CCPoint3DCloudApp : public CWinApp
{
public:
	CCPoint3DCloudApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCPoint3DCloudApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation
	//{{AFX_MSG(CCPoint3DCloudApp)
	afx_msg void OnAppAbout();
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CPOINT3DCLOUD_H__1C153336_7F0F_45FB_BDC4_896744C4C2E1__INCLUDED_)
