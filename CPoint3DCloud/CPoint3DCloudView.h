// CPoint3DCloudView.h : interface of the CCPoint3DCloudView class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_CPOINT3DCLOUDVIEW_H__0305C427_F3BD_4466_9492_558EC1B3F8CF__INCLUDED_)
#define AFX_CPOINT3DCLOUDVIEW_H__0305C427_F3BD_4466_9492_558EC1B3F8CF__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class COpenGL;

class CCPoint3DCloudView : public CView
{
protected: // create from serialization only
	CCPoint3DCloudView();
	DECLARE_DYNCREATE(CCPoint3DCloudView)

// Attributes
public:
	CCPoint3DCloudDoc* GetDocument();

// Operations
public:
   COpenGL m_nViewOpenGL;
// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCPoint3DCloudView)
	public:
	virtual void OnDraw(CDC* pDC);  // overridden to draw this view
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	protected:
	virtual BOOL OnPreparePrinting(CPrintInfo* pInfo);
	virtual void OnBeginPrinting(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnEndPrinting(CDC* pDC, CPrintInfo* pInfo);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CCPoint3DCloudView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CCPoint3DCloudView)
	afx_msg void OnDestroy();
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg void OnRButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnRButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnMButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnMButtonUp(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#ifndef _DEBUG  // debug version in CPoint3DCloudView.cpp
inline CCPoint3DCloudDoc* CCPoint3DCloudView::GetDocument()
   { return (CCPoint3DCloudDoc*)m_pDocument; }
#endif

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CPOINT3DCLOUDVIEW_H__0305C427_F3BD_4466_9492_558EC1B3F8CF__INCLUDED_)
