// CPoint3DCloudDoc.cpp : implementation of the CCPoint3DCloudDoc class
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
// CCPoint3DCloudDoc

IMPLEMENT_DYNCREATE(CCPoint3DCloudDoc, CDocument)

BEGIN_MESSAGE_MAP(CCPoint3DCloudDoc, CDocument)
	//{{AFX_MSG_MAP(CCPoint3DCloudDoc)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	//}}AFX_MSG_MAP
	ON_COMMAND(ID_FILE_SEND_MAIL, OnFileSendMail)
	ON_UPDATE_COMMAND_UI(ID_FILE_SEND_MAIL, OnUpdateFileSendMail)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudDoc construction/destruction

CCPoint3DCloudDoc::CCPoint3DCloudDoc()
{
	// TODO: add one-time construction code here

}

CCPoint3DCloudDoc::~CCPoint3DCloudDoc()
{
}

BOOL CCPoint3DCloudDoc::OnNewDocument()
{
	if (!CDocument::OnNewDocument())
		return FALSE;

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	return TRUE;
}



/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudDoc serialization

void CCPoint3DCloudDoc::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		// TODO: add storing code here
	}
	else
	{
		// TODO: add loading code here
	}
}

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudDoc diagnostics

#ifdef _DEBUG
void CCPoint3DCloudDoc::AssertValid() const
{
	CDocument::AssertValid();
}

void CCPoint3DCloudDoc::Dump(CDumpContext& dc) const
{
	CDocument::Dump(dc);
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CCPoint3DCloudDoc commands

void CCPoint3DCloudDoc::OnFileOpen() 
{
	// TODO: Add your command handler code here
	CString m_strPathname;
	CString m_strFilename;
	CString m_strFileTitle;
	CString m_strFileext;//文件的扩展名

	LPTSTR lpszFilter = "ASCII Data Files(*.asc , *ASC)|*.asc|ASCII Data Files(*.txt , *TXT)|*.txt|All Files(*.*)|*.*||";
	CFileDialog p_dlg(true,".txt",NULL,OFN_HIDEREADONLY|OFN_OVERWRITEPROMPT,lpszFilter,NULL);
	if (p_dlg.DoModal() == IDOK)
	{
		m_strPathname = p_dlg.GetPathName();
		m_strFilename = p_dlg.GetFileName();
		m_strFileTitle = p_dlg.GetFileTitle();
		m_strFileext = p_dlg.GetFileExt();//文件扩展名对话框
		
	}
	else 
		return ;
	//m_strFileext.MakeUpper();//扩展名转化成大写字母

	m_strFileext.MakeLower();//2005.7.21//
	if (m_strFileext!="txt" && m_strFileext!="asc")
	{
		return ;
	}

		long t1=GetTickCount();



	FILE *p_txt= fopen(m_strPathname,"r");
	if (p_txt == NULL)
	{
	   fclose(p_txt);
	   return ;
	}
//////////////////////////////////////////////////////////////////////////
//检索文件开头
//   int nSize = sizeof('\n');
   TCHAR tHead ;
   int nBegin = 0;
   tHead = fgetc(p_txt);//   tHead = fgetc(p_txt);
   switch(tHead)
   {
		case EOF://
		   nBegin = 0;
		   break;
		case '!'://Catia
		   OnFileOpenCATIA(p_txt);
		   return;
          // nBegin = 50;
		   break;
		case '#': //Geomagic    
			nBegin = 30;
		   break;
//		case '\n':
//			nBegin = 0;
//			break;
		default:
			nBegin = 0;
	 	   break;
   }
   fseek(p_txt , nBegin , SEEK_SET);
//检索文件开头
//////////////////////////////////////////////////////////////////////////
//检索文件结尾 是否有字符串标志
	fseek(p_txt ,-14L , SEEK_END);
	TCHAR  tchar[14]=_T("");
	fgets(tchar , 13 , p_txt);
	tchar[13] = '\0';
	CString str = tchar;
	int nEndCloud = str.Find("cloud");
	int nEnd = -1;
	if (nEndCloud == -1)
		nEnd = EOF;
	else if (nEndCloud != 6)
	{
	   nEndCloud = nEndCloud - 20;
	   nEnd = fseek(p_txt ,nEndCloud , SEEK_END);
	}
//检索文件结尾 是否有字符串标志
//////////////////////////////////////////////////////////////////////////
// 判定数据点格式是三个参数 还是六个参数
	fseek(p_txt , nBegin , SEEK_SET);
	TCHAR tchar1[128] = _T("");
	fgets(tchar1 , 127 , p_txt);
    str = tchar1;
	int nLine = str.Find('\n');
	int nNull = str.Find(' ') , nPos = 0;
	int sum = 2;
	while(nNull < nLine)
	{
		nPos = str.Find(' ' , nNull + 1);
        if (nPos != -1)
		{
			nNull = nPos ;
			++sum;
		}
		else
			break;
	}
// 判定数据点格式是三个参数 还是六个参数
//////////////////////////////////////////////////////////////////////////
	POINT3D prMax , prMin;
	prMax.x = -10000000.0;
	prMax.y = -10000000.0;
	prMax.z = -10000000.0;
	prMin.x = 10000000.0;
	prMin.y = 10000000.0;
	prMin.z = 10000000.0;

//////////////////////////////////////////////////////////////////////////
    fseek(p_txt , nBegin , SEEK_SET);
	AfxGetApp()->BeginWaitCursor();	//	vector <POINT3D> m_prToothPointsVertexList;//2005.7.28添加//定义一个字符向量
	m_prToothPointsVertexList.clear();//	POINT3D f_xyz;

	POINT7D f_xyz;
	POINT3D f_xyzw;
	switch(sum) 
	{
	case 3://
		while (fscanf(p_txt,"%f %f %f",&f_xyzw.x,&f_xyzw.y,&f_xyzw.z) != nEnd)
		{
           f_xyz = f_xyzw;
		   prMax.x=max(prMax.x,f_xyz.x);
		   prMax.y=max(prMax.y,f_xyz.y);
		   prMax.z=max(prMax.z,f_xyz.z);
		   prMin.x=min(prMin.x,f_xyz.x);
		   prMin.y=min(prMin.y,f_xyz.y);
		   prMin.z=min(prMin.z,f_xyz.z);		   
		  // m_prToothPointsVertexList.push_back(f_xyz);	   
		}
		break;
	default://
		while (fscanf(p_txt,"%f %f %f %f %f %f",&f_xyz.x,&f_xyz.y,&f_xyz.z,&f_xyz.nx,&f_xyz.ny,&f_xyz.nz) != nEnd)
		{
		   prMax.x=max(prMax.x,f_xyz.x);
		   prMax.y=max(prMax.y,f_xyz.y);
		   prMax.z=max(prMax.z,f_xyz.z);
		   prMin.x=min(prMin.x,f_xyz.x);
		   prMin.y=min(prMin.y,f_xyz.y);
		   prMin.z=min(prMin.z,f_xyz.z);		   
		   m_prToothPointsVertexList.push_back(f_xyz);	   
		}
		break;
	}

	fclose(p_txt);//	vector <POINT3D>::iterator pItVector1,pItVector2;//为字符数组定义游标iterator

	
		long t2=GetTickCount();
		long t3=t2-t1;
	//////////////////////////////////////////////////////////////////////////
	vector <POINT7D>::iterator pItVector1,pItVector2;
	pItVector1= m_prToothPointsVertexList.end()-1;//除了最后一个元素
	pItVector2= pItVector1-1;
	if (Distance(*pItVector1,*pItVector2)<0.003)
	{
		m_prToothPointsVertexList.erase(pItVector1);
		return ;
	}
//////////////////////////////////////////////////////////////////////////

	prBoxCenter = (prMax+prMin) / 2;
	prBoxSize = (prMax-prMin) / 2;
	CCPoint3DCloudView *pView =(CCPoint3DCloudView*) GetView(RUNTIME_CLASS(CCPoint3DCloudView));
	m_nDocOpenGL = &(pView->m_nViewOpenGL);//	double fmax = max(prBoxSize.x , prBoxSize.y);//	fmax = max(prBoxSize.z , fmax);
	m_nDocOpenGL->SetBox(tagCVector(prBoxCenter.x , prBoxCenter.y ,prBoxCenter.z) , tagCVector(prBoxSize.x , prBoxSize.y , prBoxSize.z));
	AfxGetApp()->EndWaitCursor();
    UpdateAllViews(NULL, 0, 0);	
}

void CCPoint3DCloudDoc::OnDraw()
{
	if (m_prToothPointsVertexList.empty())
		return;
	glColor3f(1.0 , 1.0  , 1.0 );//	vector<POINT3D>::iterator pItPoint3d;
	vector<POINT7D>::iterator pItPoint3d;
	glBegin(GL_POINTS);
	for (pItPoint3d = m_prToothPointsVertexList.begin() ; 
	pItPoint3d != m_prToothPointsVertexList.end() ; ++pItPoint3d)
		if (pItPoint3d->w > 0)
      glVertex3f(pItPoint3d->x , pItPoint3d->y ,pItPoint3d->z);
    glEnd();
}

CView* CCPoint3DCloudDoc::GetView(CRuntimeClass *pViewClass)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL ) 
	{
		CView* pView = GetNextView(pos);
		if (pView->IsKindOf(pViewClass))
			return pView;
	}
    return NULL;
}

BOOL CCPoint3DCloudDoc::OnFileOpenCATIA(FILE *f_txt)
{
	AfxGetApp()->BeginWaitCursor();	//	vector <POINT3D> m_prToothPointsVertexList;//2005.7.28添加//定义一个字符向量
	m_prToothPointsVertexList.clear();
	fseek(f_txt , 0L , SEEK_SET);
//////////////////////////////////////////////////////////////////////////
//catia head
	TCHAR tchar[128] = _T("");
	CString str ;
	int i = 0;
	for (i = 0 ; i < 5 ; ++i)
	{
		memset(tchar , 128 , 0);
		fgets(tchar , 127 , f_txt);
		tchar[127] = '\0';	
	}
	/////////////////////////////////////////////////////////////////////////
	//获取文件数据格式
	str = tchar;
    int nFirstChar = str.Find('\'') + 1;
    int nEndChar = str.ReverseFind('\'');
	int nChar = nEndChar - nFirstChar ;
	CString strchar = str.Mid(nFirstChar , nChar);
	nChar = (nChar + 1) / 5;
	CString *strXYZ = new CString[nChar];
	CString *strFormat = new CString[nChar];
	for ( i = 0 ; i < nChar ; ++i)
	{
		strXYZ[i] = strchar.Mid(i * 5 , 1);
		strFormat[i] = strchar.Mid(i * 5 + 3 , 1);
	}
	//获取文件数据格式
	//////////////////////////////////////////////////////////////////////////
	//点个数
	for (i = 0 ; i < 2 ; ++i)
	{
		memset(tchar , 128 , 0);
		fgets(tchar , 127 , f_txt);
		tchar[127] = '\0';	
	} 
	str = tchar;  
    nFirstChar = str.Find('=') + 1;
 	nEndChar = str.GetLength() - nFirstChar;
    strchar = str.Mid(nFirstChar , nEndChar);
	int nPointSize = atoi(strchar);
	m_prToothPointsVertexList.resize(nPointSize);
	//
	memset(tchar , 128 , 0);
	fgets(tchar , 127 , f_txt);
	tchar[127] = '\0';
	
//catia head
//////////////////////////////////////////////////////////////////////////
	int *nXYZ = new int[nChar + 1];
    int j = 0;

	POINT3D prMax , prMin;
	prMax.x = -10000000.0;
	prMax.y = -10000000.0;
	prMax.z = -10000000.0;
	prMin.x = 10000000.0;
	prMin.y = 10000000.0;
	prMin.z = 10000000.0;

	vector<POINT7D>::iterator pItPoint7D = m_prToothPointsVertexList.begin();
    for (j = 0 ; j < nPointSize ; ++j , ++pItPoint7D)
	{
		memset(tchar , 128 , 0);
		fgets(tchar , 127 , f_txt);
		tchar[127] = '\0';
		str = tchar;
		for ( i = 0 ; i < nChar ; ++i)
			nXYZ[i] = str.Find(strXYZ[i]);
		nXYZ[i] = str.GetLength();
		for (i = 0 ; i < 3 ; ++i)
		{
			strchar = str.Mid(nXYZ[i] +1 , nXYZ[i + 1] - nXYZ[i] - 1);
			switch(i)
			{
			case 0:	  
				pItPoint7D->x = atof(strchar);				
				prMax.x=max(prMax.x,pItPoint7D->x); 
				prMin.x=min(prMin.x,pItPoint7D->x); 
				break;
			case 1:  
				pItPoint7D->y = atof(strchar);
				prMax.y=max(prMax.y,pItPoint7D->y);
				prMin.y=min(prMin.y,pItPoint7D->y);
				break;
			case 2:
				pItPoint7D->z = atof(strchar);   
				prMax.z=max(prMax.z,pItPoint7D->z); 
				prMin.z=min(prMin.z,pItPoint7D->z);
				break;
			}
		}
	}
//////////////////////////////////////////////////////////////////////////

	delete [] strXYZ;
	delete []strFormat;
	delete []nXYZ;

	strXYZ = NULL;
	strFormat = NULL;
	nXYZ = NULL;
    fclose(f_txt);
	prBoxCenter = (prMax+prMin) / 2;
	prBoxSize = (prMax-prMin) / 2;
	CCPoint3DCloudView *pView =(CCPoint3DCloudView*) GetView(RUNTIME_CLASS(CCPoint3DCloudView));
	m_nDocOpenGL = &(pView->m_nViewOpenGL);//	double fmax = max(prBoxSize.x , prBoxSize.y);//	fmax = max(prBoxSize.z , fmax);
	m_nDocOpenGL->SetBox(tagCVector(prBoxCenter.x , prBoxCenter.y ,prBoxCenter.z) , tagCVector(prBoxSize.x , prBoxSize.y , prBoxSize.z));
	AfxGetApp()->EndWaitCursor();
    UpdateAllViews(NULL, 0, 0);	
	return TRUE;
}
