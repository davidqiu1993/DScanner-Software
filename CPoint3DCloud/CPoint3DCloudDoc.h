// CPoint3DCloudDoc.h : interface of the CCPoint3DCloudDoc class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_CPOINT3DCLOUDDOC_H__AD80BE6E_92A4_44EA_985C_E143306B6A46__INCLUDED_)
#define AFX_CPOINT3DCLOUDDOC_H__AD80BE6E_92A4_44EA_985C_E143306B6A46__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "CPoint3D.h"
class COpenGL;

class CCPoint3DCloudDoc : public CDocument
{
protected: // create from serialization only
	CCPoint3DCloudDoc();
	DECLARE_DYNCREATE(CCPoint3DCloudDoc)

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCPoint3DCloudDoc)
	public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);
	//}}AFX_VIRTUAL

// Implementation
public:
	BOOL OnFileOpenCATIA(FILE *f_txt);
	CView* GetView(CRuntimeClass *pViewClass);
	void OnDraw();
	virtual ~CCPoint3DCloudDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:
//    vector<POINT3D> m_prToothPointsVertexList;
	vector<POINT7D> m_prToothPointsVertexList;
	POINT3D prBoxCenter;
	POINT3D prBoxSize;
	COpenGL *m_nDocOpenGL;
// Generated message map functions
protected:
	//{{AFX_MSG(CCPoint3DCloudDoc)
	afx_msg void OnFileOpen();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CPOINT3DCLOUDDOC_H__AD80BE6E_92A4_44EA_985C_E143306B6A46__INCLUDED_)
