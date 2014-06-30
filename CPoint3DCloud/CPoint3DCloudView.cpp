// CPoint3DCloudView.cpp : implementation of the CCPoint3DCloudView class
//

#include "stdafx.h"
#include "CPoint3DCloud.h"

#include "CPoint3DCloudDoc.h"
#include "CPoint3DCloudView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView

IMPLEMENT_DYNCREATE(CCPoint3DCloudView, CView)

BEGIN_MESSAGE_MAP(CCPoint3DCloudView, CView)
	//{{AFX_MSG_MAP(CCPoint3DCloudView)
	ON_WM_DESTROY()
	ON_WM_CREATE()
	ON_WM_ERASEBKGND()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_MOUSEWHEEL()
	ON_WM_RBUTTONDOWN()
	ON_WM_RBUTTONUP()
	ON_WM_SIZE()
	ON_WM_MBUTTONDOWN()
	ON_WM_MBUTTONUP()
	//}}AFX_MSG_MAP
	// Standard printing commands
	ON_COMMAND(ID_FILE_PRINT, CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, CView::OnFilePrintPreview)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView construction/destruction

CCPoint3DCloudView::CCPoint3DCloudView()
{
	// TODO: add construction code here
	m_nViewOpenGL.SetProjectionMode(1);
	m_nViewOpenGL.SetTwoView(false);
}

CCPoint3DCloudView::~CCPoint3DCloudView()
{
}

BOOL CCPoint3DCloudView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView drawing

void CCPoint3DCloudView::OnDraw(CDC* pDC)
{
	CCPoint3DCloudDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	// TODO: add draw code for native data here
	m_nViewOpenGL.OnInitRenderOpenGL();
      pDoc->OnDraw();
	m_nViewOpenGL.SwapBufferOpenGL();
}

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView printing

BOOL CCPoint3DCloudView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// default preparation
	return DoPreparePrinting(pInfo);
}

void CCPoint3DCloudView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add extra initialization before printing
}

void CCPoint3DCloudView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add cleanup after printing
}

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView diagnostics

#ifdef _DEBUG
void CCPoint3DCloudView::AssertValid() const
{
	CView::AssertValid();
}

void CCPoint3DCloudView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CCPoint3DCloudDoc* CCPoint3DCloudView::GetDocument() // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CCPoint3DCloudDoc)));
	return (CCPoint3DCloudDoc*)m_pDocument;
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudView message handlers

void CCPoint3DCloudView::OnDestroy() 
{
	CView::OnDestroy();
	
	// TODO: Add your message handler code here
	m_nViewOpenGL.OnDestroyOpenGL();	
}

int CCPoint3DCloudView::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CView::OnCreate(lpCreateStruct) == -1)
		return -1;
	
	// TODO: Add your specialized creation code here
	m_nViewOpenGL.OnInitOpenGL(GetSafeHwnd());	
	return 0;
}

BOOL CCPoint3DCloudView::OnEraseBkgnd(CDC* pDC) 
{
	// TODO: Add your message handler code here and/or call default
	return TRUE;
//	return CView::OnEraseBkgnd(pDC);
}

void CCPoint3DCloudView::OnLButtonDown(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnLButtonDown(nFlags,point);	
	CView::OnLButtonDown(nFlags, point);
}

void CCPoint3DCloudView::OnLButtonUp(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnLButtonUp(nFlags,point);
	InvalidateRect(NULL,FALSE); 	
	CView::OnLButtonUp(nFlags, point);
}

void CCPoint3DCloudView::OnMouseMove(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnMouseMove(nFlags,point);
	InvalidateRect(NULL,FALSE); 	
	CView::OnMouseMove(nFlags, point);
}

BOOL CCPoint3DCloudView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnMouseWheel(nFlags, zDelta, pt);
	InvalidateRect(NULL,FALSE);	
	return CView::OnMouseWheel(nFlags, zDelta, pt);
}



void CCPoint3DCloudView::OnRButtonDown(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnRButtonDown(nFlags, point);	
	CView::OnRButtonDown(nFlags, point);
}

void CCPoint3DCloudView::OnRButtonUp(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnRButtonUp(nFlags,point);
	InvalidateRect(NULL,FALSE); 	
	CView::OnRButtonUp(nFlags, point);
}

void CCPoint3DCloudView::OnSize(UINT nType, int cx, int cy) 
{
	CView::OnSize(nType, cx, cy);
	
	// TODO: Add your message handler code here
    m_nViewOpenGL.OnSizeOpenGL(nType, cx, cy);	
}

void CCPoint3DCloudView::OnMButtonDown(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnMButtonDown(nFlags,point);	
	CView::OnMButtonDown(nFlags, point);
}

void CCPoint3DCloudView::OnMButtonUp(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	m_nViewOpenGL.OnMButtonUp(nFlags,point);
	InvalidateRect(NULL,FALSE); 	
	CView::OnMButtonUp(nFlags, point);
}
