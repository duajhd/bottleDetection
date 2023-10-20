using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using bottleDetect.ConfigInfo;
using baseForm;
using GxIAPINET;
using System.Drawing.Imaging;
using bottleDetection.widget;

namespace bottleDetection
{
    
    public partial class Form1 : Form



    {
        Mutex m_mutexmGrab;         //相机操作互斥
        Mutex m_mutexmCarve;                       //相机操作互斥
        Mutex m_mutexmPLCOperation;             //PLC操作互斥
        Mutex m_mutexmSendResult;
        Mutex m_mutexmCheckSet;                    //检测设置互斥
        Mutex m_mutexmLogfile;
        //Mutex mutexListDetect[23];
        List<Mutex> mutexListDetect;

        private camera cameraInstance = null;
        private c_imageDataProcess imageProcess;
        private List<s_imageData> detectImageList = new List<s_imageData>();
        private Form detecForm = null;
        private Form algothrimSetForm = null;
        private Form systemSetForm = null;
        private Form statisticForM = null;
        private Form PLCForM = null;
        private Form realStatisticForM = null;
        private bottleDetection.widget.PLCForm newPLCForm = null;
        private Form showImageForm = null;
        private bottleDetection.widget.PLCStatus PLCStatusForm = null;
        //生成s_imageData类型对象然后放到List中
        //待检测队列


        //List<Mutex> iCalCount;
        //  List<>
        public management tabManagement;
        public PLC tabPLC;

    

        private Form1 pMainForm;  //指向自己的指针


    

        ~Form1()
        {
          
        }
        public Form1()
        {
            InitializeComponent();
            //注意：UIComponetForm是自己需进行适配的窗体名称
            pMainForm = this;
            tabManagement = new management();
            tabPLC = new PLC();
            detecForm = new DetecForm();
            algothrimSetForm = new algothrimSet();
            systemSetForm = new systemSet();
            statisticForM = new StatisticForm();
            PLCForM = new PLCForm();
            realStatisticForM = new RealStatisticForm();
            newPLCForm = new bottleDetection.widget.PLCForm();
            showImageForm = new ShowImage();
            PLCStatusForm = new PLCStatus();
            ///  cameraInstance.InitCamera();

            PLCStatusForm.openPLC += openPLCForm;


        }
      

        





        /// <summary>
        /// 重置窗体布局
        /// </summary>
      

       
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;//获取或设置一个值，该值指示在将键事件传递到具有焦点的控件前，窗体是否将接收此键事件。

             PLCStatusForm.ReWinformLayout(Width,Height);
            //MessageBox.Show(Width.ToString());




            //可能是添加失败

        }




        private void  Camallback()
        {

        }

        public void Init()
        {
            Initialize();

            InitCamera();

            InitPLC();

            InitThread();

        }

        private void  Initialize()      //初始化
        {

        }

        private void InitCamera()       //初始化相机
        {

        }

        private void InitPLC()          //初始化PLC
        {

        }

        private void InitThread()
        {

        }

        private void GlobalGrabOverCallback(object objUserParam, IFrameData objIFrameData) //回调入口      
        {
            //明天将图片放入缓冲区
            //研究c#申请内存

            try
            {

                //保存也可做其他处理
                imageProcess.saveBMP(objIFrameData, "test.bmp");
            }
            catch (Exception)
            {
            }
        }

        private void GrabCallback()         //真正回调函数
        {

        }

       private void ReleasePLC()            //释放PLC
        {

        }

        private void ReleaseImage()         //释放图像
        {

        }

        private void ReleaseCamera()        //释放相机
        {

        }

        public void ReleaseAll()            //释放所有资源
        {

        }
       public void PutImagetoDetectList(int iRealCameraSN, int m_iImageCount)
        {

        }
        public void PutImagetoDetectList(int iRealCameraSN, int m_iImageCount, int iCorveTh){

        }
        public string getVersion(string strFullName)
        {
            return strFullName;
        }

        public void StartDetectThread()//开启检测线程
        {           
        }
        private void initDetectThread() { } //开启检测线程

        private void StartPLCThread() { }  //开启接口卡线程
     //   public void ReadMachineSignal(CMachineSignal& myMachineSingal, int iCameraSN) { 
      //  } //IO卡读取图像编号
    
      //  public void writeLogText(string logText, e_SaveLogType eSaveLogType)
      //  {

       // }
        public  void ShowCheckSet(int nCamIdx = 0, int signalNumber = 0)
        {

        }
      
        public bool changeLanguage(int nLangIdx)
        {
            return true;

        }
        private void InitCamImage(int iCameraNo)
        {

        }
        private void StartCamGrab()
        {

        }

      

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keys.F12 == e.KeyCode)//按下F12退出
            {
               
                this.Close();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

          //  Form1 _that = this;
         //   Action<s_imageData> takeImage = (data) =>
          //  {
          //      _that.detectImageList.Add(data);
         //   };

          
         
        }

      

       

       

     
      

       

      
       

      
     

        private void button7_Click(object sender, EventArgs e)
        {

            loadFrame(new PLCForm());
        }


        private void loadFrame(Object form)
        {
            if (this.BodyContainer.Controls.Count > 0)
            {
                this.BodyContainer.Controls.RemoveAt(0);
               
            }
            Form widget = form as Form;

            widget.TopLevel = false;
            widget.Dock = DockStyle.Fill;
            this.BodyContainer.Controls.Add(widget);
            this.BodyContainer.Tag = widget;
            widget.Show();
           
        }
        private void openPLCForm()
        {
            loadFrame(newPLCForm);
        }
        private void button6_Click(object sender, EventArgs e)            //点击退出
        {
            this.Close();
        }

       

       

        private void StatisticBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

       
      

        private void managementBtn_Click(object sender, EventArgs e)
        {
            loadFrame(realStatisticForM);
        }

       

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void startDetectBtn_Click_1(object sender, EventArgs e)
        {
            //开始检测1.打开相机2.
        }

        private void algothrimSetBtn_Click_1(object sender, EventArgs e)
        {
            loadFrame(algothrimSetForm);
        }

        private void statisticInfoBtn_Click_1(object sender, EventArgs e)
        {
            
            loadFrame(showImageForm);
        }

        private void systemSetBtn_Click_1(object sender, EventArgs e)
        {
            loadFrame(systemSetForm);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            
        }

        private void statisticBtn_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void PLCBtn_Click(object sender, EventArgs e)
        {
            loadFrame(PLCStatusForm);
        }
    }


    

}
