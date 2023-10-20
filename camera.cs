using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using bottleDetect.ConfigInfo;
using bottleDetection.Common;
using System.Drawing.Imaging;
using GxIAPINET;
using System.Windows.Forms;
using System.Drawing;
//Bitmap类处理图像，然后返回到form
namespace bottleDetection
{
    //多个品牌相机使用时，可以设置一个统一的函数接受图像字节数据
    //测试走瓶子，取图像能保证1.打开相机2.能绑定回调函数3.能正常触发回调

    //现在的问题是软件崩溃后，相机没有被正常关闭，再次打开时，就无法打开相机
    //即使干掉了相机控制类，每个不同品牌的相机类也需要有打开相机、关闭相机的成员函数
    //明天做1.动态计算生成显示图像的框2.将正常和条纹区分开，放到两个窗口中
    public class camera
    {


        //l_camList报了可访问性不一致，原因是i_RealCamera属性没有加public
        //可访问性不一致解决方法，将i_RealCamera属性设置为public
        public List<i_RealCamera> l_camList = new List<i_RealCamera>();

        private IGXFactory m_objIGXFactory = null;                            ///<Factory对像
        //
        List<IGXDeviceInfo> l_listGXDeviceInfo = new List<IGXDeviceInfo>();
        public void InitCamera()           //初始化相机，包括执行大恒Init函数，获取相机列表，将相机分为应力，将相机划分工位
        {
            try
            {
                m_objIGXFactory = IGXFactory.GetInstance();
                m_objIGXFactory.Init();
            }
            catch (CGalaxyException ex)
            {
                string strErrorInfo = "错误码为:" + ex.GetErrorCode().ToString() +
                "错误描述信息为:" + ex.Message;


                MessageBox.Show(strErrorInfo + "程序即将退出");

                return;
            }

            IGXFactory.GetInstance().UpdateAllDeviceList(200, l_listGXDeviceInfo);    //  获得所有相机列表
            if (l_listGXDeviceInfo.Count > 0)
            {
                foreach (IGXDeviceInfo singleDeviceInfo in l_listGXDeviceInfo)
                {
                    //
                    i_RealCamera i_RealCamerinstance = new i_RealCamera();          //结构体也需要new？

                    String strSN = singleDeviceInfo.GetSN();
                    String strUserID = singleDeviceInfo.GetUserID();
                    String strMAC = singleDeviceInfo.GetMAC();
                    String IP = singleDeviceInfo.GetIP();


                    //配置相机基础参数
                    i_RealCamerinstance.strIP = IP;
                    i_RealCamerinstance.strMAC = strMAC;
                    i_RealCamerinstance.strUserID = strUserID;
                    i_RealCamerinstance.strSN = strSN;
                    i_RealCamerinstance.objDevice = null;
                    i_RealCamerinstance.o_objIGXFeatureControl = null;
                    i_RealCamerinstance.o_objIGXStream = null;

                    l_camList.Add(i_RealCamerinstance);
                }
            }
            else if (l_listGXDeviceInfo.Count <= 0)
            {
                MessageBox.Show("未找到任何可用设备");   //为什么执行两次、
                return;
            }


        }

        public camera()
        {
            InitCamera();
        }


        ~camera()                             //释放相机类的资源
        {
            IGXFactory.GetInstance().Uninit();

        }


        public bool openCamera(int calIndex, int i_Mode)     //打开索引为calIndex的相机，i_Mode为打开模式(可选IP,MAXC,SN,UserID)


        {


            return true;
        }


        public bool openALLCamera(int i_Mode)                         //打开所有相机,i_Mode为打开模式(可选IP,MAXC,SN,UserID)
        {
            i_RealCamera temp;                                         //中间变量，为结构体成员赋值
            Boolean bIsImplemented;
            try
            {
                if (l_camList.Count > 0)
                {



                    for (int i = 0; i < l_camList.Count; i++)
                    {

                        switch (i_Mode)
                        {
                            case (int)e_openMode.IP:
                                // l_camList[i].objDevice = IGXFactory.GetInstance().OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_EXCLUSIVE);
                                temp = l_camList[i];
                                //c#中，当List中的成员是结构体时，不能通过遍历List对结构体中某个成员赋值，会报错:C#无法修改“List＜T＞.this[int]“的返回值，因为它不是变量
                                //但是c#中结构体是引用类型，因此可以引入zhongjian

                                temp.objDevice = m_objIGXFactory.OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                temp.o_objIGXFeatureControl = temp.objDevice.GetRemoteFeatureControl();                   //获取属性器
                                temp.o_objIGXStream = temp.objDevice.OpenStream(0);                                       //打开流


                                break;

                            case (int)e_openMode.SN:
                                // l_camList[i].objDevice = IGXFactory.GetInstance().OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_EXCLUSIVE);
                                temp = l_camList[i];
                                //c#中，当List中的成员是结构体时，不能通过遍历List对结构体中某个成员赋值，会报错:C#无法修改“List＜T＞.this[int]“的返回值，因为它不是变量
                                //但是c#中结构体是引用类型，因此可以引入zhongjian

                                temp.objDevice = m_objIGXFactory.OpenDeviceBySN(l_camList[i].strSN, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                temp.o_objIGXFeatureControl = temp.objDevice.GetRemoteFeatureControl();                   //获取属性器
                                temp.o_objIGXStream = temp.objDevice.OpenStream(0);

                                break;

                            case (int)e_openMode.MAC:
                                // l_camList[i].objDevice = IGXFactory.GetInstance().OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_EXCLUSIVE);
                                temp = l_camList[i];
                                //c#中，当List中的成员是结构体时，不能通过遍历List对结构体中某个成员赋值，会报错:C#无法修改“List＜T＞.this[int]“的返回值，因为它不是变量
                                //但是c#中结构体是引用类型，因此可以引入zhongjian

                                temp.objDevice = m_objIGXFactory.OpenDeviceByMAC(l_camList[i].strMAC, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                temp.o_objIGXFeatureControl = temp.objDevice.GetRemoteFeatureControl();                   //获取属性器
                                temp.o_objIGXStream = temp.objDevice.OpenStream(0);

                                //打开流
                                break;

                            case (int)e_openMode.UserID:
                                // l_camList[i].objDevice = IGXFactory.GetInstance().OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_EXCLUSIVE);
                                temp = l_camList[i];
                                //c#中，当List中的成员是结构体时，不能通过遍历List对结构体中某个成员赋值，会报错:C#无法修改“List＜T＞.this[int]“的返回值，因为它不是变量
                                //但是c#中结构体是引用类型，因此可以引入zhongjian

                                temp.objDevice = m_objIGXFactory.OpenDeviceByMAC(l_camList[i].strUserID, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                temp.o_objIGXFeatureControl = temp.objDevice.GetRemoteFeatureControl();                   //获取属性器
                                temp.o_objIGXStream = temp.objDevice.OpenStream(0);

                                //打开流
                                break;
                        }

                    }

                }
                return true;
            }
            catch (CGalaxyException ex)
            {
                return false;
            }



        }

        public void bingCallbackFun()
        {

        }
        public void closeALLCam()                   //关闭所有打开的流并且关闭所有相机
        {

        }
        private bool __initCalParam()
        {
            i_RealCamera temp;                                         //中间变量
            Boolean bIsImplemented;
            try
            {
                if (l_camList.Count > 0)
                {



                    for (int i = 0; i < l_camList.Count; i++)
                    {

                        temp = l_camList[i];
                        bIsImplemented = temp.o_objIGXFeatureControl.IsImplemented("Width");
                        if (bIsImplemented)
                        {
                            IIntFeature objInt = temp.o_objIGXFeatureControl.GetIntFeature("Width");
                            temp.i_imageWidth = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                            temp.i_imageHeight = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                            temp.i_nPayloadSize = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                        }

                        temp.b_isColor = __IsSupportColor(temp.objDevice);                      //确定是否是彩色相机

                        temp.i_nPayloadSize = __getStride(temp.i_imageWidth, temp.b_isColor) * temp.i_imageHeight;        //获取图像存储的字节数




                    }

                }
                return true;
            }
            catch (CGalaxyException ex)
            {
                return false;
            }


        }

        private bool __IsSupportColor(IGXDevice o_GXDevice)   //计算相机是否支持彩色模式
        {
            bool bIsImplemented = false;
            bool bIsMono = false;
            string strPixelFormat = "";

            strPixelFormat = o_GXDevice.GetRemoteFeatureControl().GetEnumFeature("PixelFormat").GetValue();
            if (0 == string.Compare(strPixelFormat, 0, "Mono", 0, 4))
            {
                bIsMono = true;
            }
            else
            {
                bIsMono = false;
            }

            bIsImplemented = o_GXDevice.GetRemoteFeatureControl().IsImplemented("PixelColorFilter");

            // 若当前为非黑白且支持PixelColorFilter则为彩色
            if ((!bIsMono) && (bIsImplemented))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int __getStride(int i_iWidth, bool b_isColor)
        {
            return b_isColor ? i_iWidth * 3 : i_iWidth;             //b_isColor表示是彩色相机
        }
    }



    public class camControl {           //使用相机控制类代替camera类 camcontrol应该有读取配置文件的功能

        public List<realCameraInfo> l_camList = new List<realCameraInfo>();         //每个真实相机数组
        static IGXFactory m_objIGXFactory = IGXFactory.GetInstance();
        List<IGXDeviceInfo> l_listGXDeviceInfo = new List<IGXDeviceInfo>();         //相机信息列表
        static int openMode = 3;


        private Dictionary<string, int> cameraIndex;                                //保存相机IP和序号的对应


        //学习c#反射和数据库
        //不设置为静态方法，只从ini中读取配置(初始化camControl类时从ini读取配置并设置相机)
     public    void setCamerParam(s_camerConfig configInfo,int camIndex)                 //设置相机参数
        {

            //静态函数可以执行
            //遍历这个结构体

            realCameraInfo temp;
            Boolean bIsImplemented;

            temp = l_camList[camIndex];

            foreach (System.Reflection.PropertyInfo property in configInfo.GetType().GetProperties())
            {
                MessageBox.Show(property.Name);

                switch (property.Name)
                {
                    case "i_Width":


                        //在这个case块下可以设置结构体
                     
                        bIsImplemented = temp.o_objIGXFeatureControl.IsImplemented("Width");
                        if (bIsImplemented)
                        {
                            IIntFeature objInt = temp.o_objIGXFeatureControl.GetIntFeature("Width");
                            temp.i_imageWidth = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                            temp.i_imageHeight = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                            //  MessageBox.Show(temp.i_imageHeight.ToString());       //获取图片大小正常
                            temp.i_nPayloadSize = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();

                            if (temp.triggerMode == (int)e_truggerMode.hard)                         //触发模式选
                            {
                                temp.o_objIGXFeatureControl.GetEnumFeature("TriggerMode").SetValue("On");
                                temp.o_objIGXFeatureControl.GetEnumFeature("TriggerSource").SetValue("Line0");                  //设置为外触发
                            }

                        }

                        break;

                    case "i_Height":

                        break;

                }
            }
        }
        //静态变量初始化
        public camControl()
        {
            cameraIndex = new Dictionary<string, int>();
            cameraIndex.Add("192.168.53.1", 1);
            cameraIndex.Add("192.168.53.2", 2);
            cameraIndex.Add("192.168.53.14", 3);
            cameraIndex.Add("192.168.53.16", 4);
            cameraIndex.Add("192.168.53.15", 5);
            cameraIndex.Add("192.168.53.11", 6);
            cameraIndex.Add("192.168.53.13", 7);
            cameraIndex.Add("192.168.53.18", 8);
            cameraIndex.Add("192.168.53.12", 9);
            cameraIndex.Add("192.168.53.20", 10);
            cameraIndex.Add("192.168.53.19", 11);
            cameraIndex.Add("192.168.53.21", 12);
            initCamera();
            cameraIndex = new Dictionary<string, int>();

 
            

        }
        ~camControl()
        {
            IGXFactory.GetInstance().Uninit();
        }
        public void initCamera()
        {
           
            try
            {
                m_objIGXFactory.Init();
            }
            catch (CGalaxyException ex)
            {
                string strErrorInfo = "错误码为:" + ex.GetErrorCode().ToString() +
                "错误描述信息为:" + ex.Message;


                MessageBox.Show(strErrorInfo + "程序即将退出");

                return;
            }
            


            IGXFactory.GetInstance().UpdateAllDeviceList(200, l_listGXDeviceInfo);    //  获得所有相机列表
            if (l_listGXDeviceInfo.Count > 0)
            {
                int i = 0;  //循环变量,用于将相机编号
                                          //这里必须思考List传递pictureBox是否合适
                foreach (IGXDeviceInfo singleDeviceInfo in l_listGXDeviceInfo)
                {
                    //
                    String strSN = singleDeviceInfo.GetSN();
                    String strUserID = singleDeviceInfo.GetUserID();
                    String strMAC = singleDeviceInfo.GetMAC();
                    String IP = singleDeviceInfo.GetIP();
                    
                    realCameraInfo i_RealCamerinstance = new realCameraInfo(0, openMode, strSN, strUserID, strMAC, IP);

                    i = cameraIndex[IP];                                    //设置相机号
                    i_RealCamerinstance.CameraID = i;
                    //配置相机基础参数

                    i_RealCamerinstance.objDevice = null;
                    i_RealCamerinstance.o_objIGXFeatureControl = null;
                    i_RealCamerinstance.o_objIGXStream = null;

                    l_camList.Add(i_RealCamerinstance);

                    
                }

               
            }
            else if (l_listGXDeviceInfo.Count <= 0)
            {
                MessageBox.Show("未找到任何可用设备");
                return;
            }
        }


        public bool openAllCamera()
        {
            try
            {
                if (l_camList.Count > 0)
                {
                    //closeAllCamera();//先把所有相机关闭再重新打开(这里只能针对流对象未空的条件下)
                  //  MessageBox.Show(l_camList.Count.ToString());

                    for (int i = 0; i < l_camList.Count; i++)
                    {
                        //调试日志:打开相机失败
                        //原因:目标是ip打开，ip对应openmode3但是打开模式写成了0

                        switch (openMode)
                        {
                            case (int)e_openMode.IP:
                                l_camList[i].objDevice = m_objIGXFactory.OpenDeviceByIP(l_camList[i].strIP, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                l_camList[i].o_objIGXStream = l_camList[i].objDevice.OpenStream(0);
                                l_camList[i].o_objIGXFeatureControl = l_camList[i].objDevice.GetRemoteFeatureControl();  //获取属性器
                                l_camList[i].m_objIGXStreamFeatureControl  = l_camList[i].o_objIGXStream.GetFeatureControl();       //获取流属性控制
                                //打开流
                                break;

                            case (int)e_openMode.UserID:
                                l_camList[i].objDevice = m_objIGXFactory.OpenDeviceByIP(l_camList[i].strUserID, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                l_camList[i].o_objIGXFeatureControl = l_camList[i].objDevice.GetRemoteFeatureControl();  //获取属性器
                                l_camList[i].o_objIGXStream = l_camList[i].objDevice.OpenStream(0);                       //打开流
                                l_camList[i].m_objIGXStreamFeatureControl = l_camList[i].o_objIGXStream.GetFeatureControl();       //获取流属性控制
                                break;
                            case (int)e_openMode.SN:
                                l_camList[i].objDevice = m_objIGXFactory.OpenDeviceByIP(l_camList[i].strSN, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                l_camList[i].o_objIGXFeatureControl = l_camList[i].objDevice.GetRemoteFeatureControl();  //获取属性器
                                l_camList[i].o_objIGXStream = l_camList[i].objDevice.OpenStream(0);                       //打开流
                                l_camList[i].m_objIGXStreamFeatureControl = l_camList[i].o_objIGXStream.GetFeatureControl();       //获取流属性控制
                                break;
                            case (int)e_openMode.MAC:
                                l_camList[i].objDevice = m_objIGXFactory.OpenDeviceByIP(l_camList[i].strMAC, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                                l_camList[i].o_objIGXFeatureControl = l_camList[i].objDevice.GetRemoteFeatureControl();  //获取属性器
                                l_camList[i].o_objIGXStream = l_camList[i].objDevice.OpenStream(0);                       //打开流
                                l_camList[i].m_objIGXStreamFeatureControl = l_camList[i].o_objIGXStream.GetFeatureControl();       //获取流属性控制
                                break;
                        }
                    }

                }
                return true;
            }
            catch (CGalaxyException ex)
            {
                //closeAllCamera();
                return false;
            }
        }

        public void closeAllCamera()                //这里应该重写，函数功能是关闭所有相机
        {
            try
            {
                if (l_camList.Count>0)
                {
                    for (int i = 0; i < l_camList.Count; i++)
                    {
                        if (l_camList[i].o_objIGXFeatureControl != null)
                        {
                            l_camList[i].o_objIGXFeatureControl.GetCommandFeature("AcquisitionStop").Execute();
                        }

                        if (null != l_camList[i].o_objIGXStream)
                        {
                            l_camList[i].o_objIGXStream.StopGrab();                 //停止采集
                            //注销采集回调函数
                            l_camList[i].o_objIGXStream.UnregisterCaptureCallback();
                            l_camList[i].o_objIGXStream.Close();
                            l_camList[i].o_objIGXStream = null;

                            l_camList[i].objDevice.Close();             //关闭相机
                            l_camList[i].objDevice = null;
                        }
                    }
                }
               


               
            }
            catch(CGalaxyException ex)
            {
                MessageBox.Show("关闭相机失败");
            }
        }
        //1.读取ini数据生成结构体2.遍历结构体成员并在遍历代码中设置相机属性值
        public void initCamParam()                                  //初始化参数包括，图像宽高
        {                                                          //初始哈相机参数改成从ini读取数据
            realCameraInfo temp;
            Boolean bIsImplemented;
            if (l_camList.Count > 0)
            {
                for(int i = 0;i< l_camList.Count; i++)
                {

                    temp = l_camList[i];
                    bIsImplemented = temp.o_objIGXFeatureControl.IsImplemented("Width");
                    if (bIsImplemented)
                    {
                        IIntFeature objInt = temp.o_objIGXFeatureControl.GetIntFeature("Width");
                        temp.i_imageWidth = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                        temp.i_imageHeight = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();
                      //  MessageBox.Show(temp.i_imageHeight.ToString());       //获取图片大小正常
                        temp.i_nPayloadSize = (int)temp.objDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();

                        if (temp.triggerMode == (int)e_truggerMode.hard)                         //触发模式选
                        {
                            temp.o_objIGXFeatureControl.GetEnumFeature("TriggerMode").SetValue("On");
                            temp.o_objIGXFeatureControl.GetEnumFeature("TriggerSource").SetValue("Line0");                  //设置为外触发
                        }
                        
                    }

                    temp.b_isColor = __IsSupportColor(temp.objDevice);                      //确定是否是彩色相机
                 

                    temp.i_nPayloadSize = __getStride(temp.i_imageWidth, temp.b_isColor) * temp.i_imageHeight;        //获取图像存储的字节数
                    temp.initBimapInfo();                                                                             //初始化bitmap结构信息
                    //
                }
            }
        }

       

        public void bindCaptrueCallbak()                    //为所有相机绑定回调
        {
            realCameraInfo temp;
            if (l_camList.Count > 0)
            {
                for(int i = 0; i < l_camList.Count; i++)
                {
                    temp = l_camList[i];                    //获取图像大小都正常
                   
                    temp.bingCallbackFunc();
                }
            }
        }
        private bool __IsSupportColor(IGXDevice o_GXDevice)   //计算相机是否支持彩色模式
        {
            bool bIsImplemented = false;
            bool bIsMono = false;
            string strPixelFormat = "";

            strPixelFormat = o_GXDevice.GetRemoteFeatureControl().GetEnumFeature("PixelFormat").GetValue();
            if (0 == string.Compare(strPixelFormat, 0, "Mono", 0, 4))
            {
                bIsMono = true;
            }
            else
            {
                bIsMono = false;
            }

            bIsImplemented = o_GXDevice.GetRemoteFeatureControl().IsImplemented("PixelColorFilter");

            // 若当前为非黑白且支持PixelColorFilter则为彩色
            if ((!bIsMono) && (bIsImplemented))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int __getStride(int i_iWidth, bool b_isColor)
        {
            return b_isColor ? i_iWidth * 3 : i_iWidth;             //b_isColor表示是彩色相机
        }




    }
    //这里必须要改写，改写相同组的保存到同一
    //先确定思路，明确要干什么，达到什么样的目的。思维决定行为
    //最好的办法还是每一个类型相机一个类
    public class realCameraInfo          //该相机采集图片缓冲区数组，回调函数
    {
       // static c_imageDataProcess imageProcess = new imageProcess();

        public Queue<s_imageData> q_imageDataQueue = new Queue<s_imageData>(256);    //保存采集的图像，最大可存储256张待处理图像
        public Queue<s_imageData> q_imageDataQueuepublic = null;
        //这些应该改成private
        public String strSN;            //相机SN号
        public String strUserID;        //相机ID
        public String strMAC;           //相机MAC地址
        public String strIP;            //相机IP地址
        public int CameraID;              //相机编号，从0开始

        public IGXDevice objDevice;            //相机指针

        public int i_camType;             //相机类型

        public int i_camCount;        //拍照总数
                                      //
        public int i_imageWidth = 0;          //图片宽
        public int i_imageHeight = 0;         //图片高
        public int i_nPayloadSize;        //图片像素尺寸

        private  int actualX;
        private  int actualY;
        private  int actualWidth;
        private  int actualHeight;




        public double i_scaledX;                     //x轴缩放
        public double i_scaledY;                     //y轴缩放

        public int pictureBoxWidth;                     //该相机对应的图像显示框的宽度

        public int pictureBoxHeight;                       //该相机对应的图像显示框的高度

        public int i_openingMode;          //相机打开模式

        public bool FullLight = true;      //为true显示全亮，false时显示条纹光

        public IGXStream o_objIGXStream;          //相机指向流地址
        public IGXFeatureControl o_objIGXFeatureControl;     //相机属性控制

        public IGXFeatureControl m_objIGXStreamFeatureControl = null;                            ///<流层属性控制器对象

        public bool b_isColor;             //是否是彩色相机
        private Bitmap image = null;                //缓冲画布
        private Graphics bitmaapGDI = null;         //缓冲画布
        private Font font;
        private SolidBrush brush;
        public bool b_isStress;             //是否是应力相机
        public int triggerMode = 0;       //触发模式，可选硬触发
        public int i_camIndex = 0;           //相机索引初始化相机对象时
        public bool b_isShowImage = false;           //是否显示图像
        PictureBox m_pic_ShowImage = null;                ///<图片显示控件
        public Graphics m_objGC = null;

        public Graphics GCStree = null;            //应力相机显示

        public Graphics GCWave = null;             //条纹光显示
        private IntPtr m_pHDC = IntPtr.Zero;
        private const uint PIXEL_FORMATE_BIT = 0x00FF0000;          ///<用于与当前的数据格式进行与运算得到当前的数据位数
        private const uint GX_PIXEL_8BIT = 0x00080000;          ///<8位数据图像格式
        private const int COLORONCOLOR = 3;
        private const uint DIB_RGB_COLORS = 0;
        private const uint SRCCOPY = 0x00CC0020;
        public bool b_isSaved = false;
       
        public int indexer = 0;

        private int nimageNum = 0;                      //用来记录全亮和条纹的图像号        
        CWin32Bitmaps.BITMAPINFO m_objBitmapInfo = new CWin32Bitmaps.BITMAPINFO();
        IntPtr m_pBitmapInfo = IntPtr.Zero;
        byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
        byte[] m_byColorBuffer = null;                ///<彩色相机buffer
        //现在是所有公用一个图像处理类，会不会导致什么问题?最好还是代码隔离，把保存为bitmap的函数集成到realCameraInfo中
     //   static imageProcesscls c_imageProcessor = new imageProcesscls(0,0,0,false);

        

        public realCameraInfo(int i_triggerMode,int i_Mode, String SN, String UserID, String MAC,String IP)          //相机打开模式(IP、SN、MAC)
        {
            //执行了构造函数后，objDevice还是空
            i_openingMode = i_Mode;
            strSN = SN;
            strUserID = UserID;
            strMAC = MAC;
            strIP = IP;
            //m_pic_ShowImage = o_showImagePictureBox;              //先不做显示
            triggerMode = i_triggerMode;
            
            b_isStress = false;
            b_isColor = false;
            
            i_camCount = 0;
            font = new Font("宋体",16,GraphicsUnit.Pixel);
            brush = new SolidBrush(Color.Red);

        }
        public void initBimapInfo()
        {
            if (b_isColor)
            {
                m_objBitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(CWin32Bitmaps.BITMAPINFOHEADER));
                m_objBitmapInfo.bmiHeader.biWidth = i_imageWidth;
                m_objBitmapInfo.bmiHeader.biHeight = i_imageHeight;
                m_objBitmapInfo.bmiHeader.biPlanes = 1;
                m_objBitmapInfo.bmiHeader.biBitCount = 24;
                m_objBitmapInfo.bmiHeader.biCompression = 0;
                m_objBitmapInfo.bmiHeader.biSizeImage = 0;
                m_objBitmapInfo.bmiHeader.biXPelsPerMeter = 0;
                m_objBitmapInfo.bmiHeader.biYPelsPerMeter = 0;
                m_objBitmapInfo.bmiHeader.biClrUsed = 0;
                m_objBitmapInfo.bmiHeader.biClrImportant = 0;
            }
            else
            {
                m_objBitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(CWin32Bitmaps.BITMAPINFOHEADER));
                m_objBitmapInfo.bmiHeader.biWidth = i_imageWidth;
                m_objBitmapInfo.bmiHeader.biHeight = i_imageHeight;
                m_objBitmapInfo.bmiHeader.biPlanes = 1;
                m_objBitmapInfo.bmiHeader.biBitCount = 8;
                m_objBitmapInfo.bmiHeader.biCompression = 0;
                m_objBitmapInfo.bmiHeader.biSizeImage = 0;
                m_objBitmapInfo.bmiHeader.biXPelsPerMeter = 0;
                m_objBitmapInfo.bmiHeader.biYPelsPerMeter = 0;
                m_objBitmapInfo.bmiHeader.biClrUsed = 0;
                m_objBitmapInfo.bmiHeader.biClrImportant = 0;

                m_objBitmapInfo.bmiColors = new CWin32Bitmaps.RGBQUAD[256];
                // 黑白图像需要初始化调色板
                for (int i = 0; i < 256; i++)
                {
                    m_objBitmapInfo.bmiColors[i].rgbBlue = (byte)i;
                    m_objBitmapInfo.bmiColors[i].rgbGreen = (byte)i;
                    m_objBitmapInfo.bmiColors[i].rgbRed = (byte)i;
                    m_objBitmapInfo.bmiColors[i].rgbReserved = 0;
                }
            }
            m_pBitmapInfo = Marshal.AllocHGlobal(2048);
            Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);
            if (i_imageWidth > 0 && i_imageHeight > 0)
            {
                m_byMonoBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];//这里可以直接初始化，因为高度和宽度在初始化相机时已经赋值
                m_byColorBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
            }
        }
        private bool __IsPixelFormat8(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            bool bIsPixelFormat8 = false;
            uint uiPixelFormatEntry = (uint)emPixelFormatEntry;
            if ((uiPixelFormatEntry & PIXEL_FORMATE_BIT) == GX_PIXEL_8BIT)
            {
                bIsPixelFormat8 = true;
            }
            return bIsPixelFormat8;
        }
        //初始化参数后，再初始化静态变量
        //相机触发是正常的;流打开也是正常的
        //加上程序退出关闭相机的代码
        //目标1.完成条纹和全亮的编码2.全亮和条纹的选择性显示
        //原则1.对与同一个瓶子，全亮和条纹图像号应该一致，因此要有一个变量记录拍照总数，另一个变量记录图像号。图像号从零开始，拍照总数从1开始2.全亮时，说明新瓶子来了，图像号和拍照总数均加1；条纹时说明还是同一个瓶子，拍照总数加1图像号使用上次的图像号
        private void captrueCallbackFunc(object objUserParam, IFrameData objIFrameData)  //相机采集回调
        {
            //现在是软触发，需要改成硬触发

            //触发相机回调，开始创建图像对象，并保存到该相机对应的缓冲区
           
           
            Bitmap bitmap;
            Bitmap waveBitmap;
            //应力没有条纹，因此
            s_imageData imageData = new s_imageData();
            //showImage(objIFrameData);                   //显示图像

            
           

            // MessageBox.Show(objIFrameData.GetBuffer().ToString());
            // showImage(objIFrameData);
            // c_imageProcessor.saveBMP(objIFrameData, strSN + indexer.ToString()+".bmp");

            if (q_imageDataQueue.Count >= 256)
            {
                MessageBox.Show("队列满");
                return;
            }
            //目标1.把图像裁剪裁剪出来2.把裁剪出来的图像填充到与该相机绑定的pictureBox
            switch (i_openingMode)
            {
                case  (int)e_openMode.IP:
                    lock (q_imageDataQueue)
                    {
                        if (!b_isStress)                        //非应力图像
                        {
                            try 
                            {
                                if ((i_camCount % 2) == 0)       //偶数全亮
                                {
                                    bufferToBitmap(objIFrameData, out bitmap);
                                    imageData.b_IsColor = b_isColor;
                                    imageData.bitmap_ImageData = bitmap;
                                    imageData.b_isStree = false;
                                    imageData.i_imageIndex = nimageNum;    //指定图像的序号
                                    imageData.b_wave = false;
                                    nimageNum += 1;
                                    if (bitmap == null)
                                    {
                                        return;
                                    }
                                    if (FullLight && (i_camCount % 2 == 0))          //显示全亮
                                    {
                                        //i_camCount是拍照总数，不是瓶子总数。之前想成了全亮奇数时不执行。但实际上是执行的，因为奇数时和偶数还是同一个瓶子
                                        //对于一个奇数号瓶，第一次拍全亮时进入回调时拍照总数是偶数。
                                        //每次进入一个新瓶子的回调，拍照总数总是偶数

                                        //bitmap.Save(strSN + indexer.ToString() + ".bmp");
                                        //DrawImage本身就有图像裁剪的功能
                                        bitmaapGDI.DrawImage(bitmap, new Rectangle(0, 0, pictureBoxWidth, pictureBoxHeight), new Rectangle(actualX, actualY, actualWidth, actualHeight), GraphicsUnit.Pixel);
                                        bitmaapGDI.DrawString("相机号" + CameraID.ToString(), font, brush, 0, 0);
                                        bitmaapGDI.DrawString("图像号" + nimageNum.ToString(), font, brush, 0, 40);
                                        m_objGC.DrawImage(image, new PointF(0, 0));
                                    }
                                }
                                else
                                {
                                    bufferToBitmap(objIFrameData, out waveBitmap);
                                    imageData.b_IsColor = b_isColor;
                                    imageData.bitmap_ImageData = waveBitmap;
                                    imageData.b_isStree = false;
                                    imageData.b_wave = true;   //奇数条纹光
                                    imageData.i_imageIndex = nimageNum;    //条纹光时说明还是全量那个瓶子，图像序号不变
                                    if (waveBitmap == null)
                                    {
                                        return;
                                    }
                                    if (!FullLight && (i_camCount % 2 == 1))          //条纹并且是奇数
                                    {

                                        //bitmap.Save(strSN + indexer.ToString() + ".bmp");
                                        bitmaapGDI.DrawImage(waveBitmap, new Rectangle(0, 0, pictureBoxWidth, pictureBoxHeight), new Rectangle(actualX, actualY, actualWidth, actualHeight), GraphicsUnit.Pixel);
                                        bitmaapGDI.DrawString("相机号" + CameraID.ToString(), font, brush, 0, 0);
                                        bitmaapGDI.DrawString("图像号" + nimageNum.ToString(), font, brush, 0, 40);
                                        m_objGC.DrawImage(image, new PointF(0, 0));
                                    }
                                }
                            }
                            catch
                            {

                            }

                        }
                        else
                        {   //应力
                            try
                            {
                                bufferToBitmap(objIFrameData, out bitmap);
                                if (bitmap == null)
                                {
                                    return;
                                }
                                imageData.b_IsColor = b_isColor;
                                imageData.bitmap_ImageData = bitmap;
                                imageData.b_isStree = true;
                                imageData.i_imageIndex = nimageNum;    //指定图像的序号
                                imageData.b_wave = false;
                                nimageNum += 1;

                                bitmaapGDI.DrawImage(bitmap, new Rectangle(0, 0, pictureBoxWidth, pictureBoxHeight), new Rectangle(actualX, actualY, actualWidth, actualHeight), GraphicsUnit.Pixel);
                                bitmaapGDI.DrawString("相机号" + CameraID.ToString(), font, brush, 0, 0);
                                bitmaapGDI.DrawString("图像号" + nimageNum.ToString(), font, brush, 0, 40);
                                m_objGC.DrawImage(image, new PointF(0, 0));
                            }
                            catch
                            {

                            }

                        }
                        
                       
                      
                        q_imageDataQueue.Enqueue(imageData);
                    }
                  
                    
                    break;
                case (int)e_openMode.MAC:
                    break;
                case (int)e_openMode.UserID:
                    break;
                case (int)e_openMode.SN:
                    break;
            }
            //每一个瓶子都要显示，下面的逻辑不对。经过验证，是对的
            //类中是设置显示全亮或条纹，但是对于同一个瓶子要拍两次照
            //一是要保证下列两个if每次必须只触发一个二是必须保证每次显示都是正确的
            //原则1.全亮偶数时，显示

           

            

            i_camCount += 1; //拍照总数加1
        }
        //明天过来研究研究算法，客户端没实验环境开发不了
        public void bingCallbackFunc()      //绑定回调

           

        {
            if (null != m_objIGXStreamFeatureControl)
            {
                //设置流层Buffer处理模式为OldestFirst
                m_objIGXStreamFeatureControl.GetEnumFeature("StreamBufferHandlingMode").SetValue("OldestFirst");
            }
            //打开相机后立马就开始采集了，没有触发
            if (o_objIGXStream != null)
            {   //RegisterCaptureCallback
                o_objIGXStream.RegisterCaptureCallback(this, captrueCallbackFunc);
                o_objIGXStream.StartGrab();                         //开启流采集
               
            }

          
            else
            {
                MessageBox.Show("相机采集流未打开");
                return;
            }


            if (null != o_objIGXFeatureControl)
            {
                o_objIGXFeatureControl.GetCommandFeature("AcquisitionStart").Execute();
            }
        }
        public void showWidth()
        {

        }
        //ping 192.168.53.2
        public void initImage()                     //初始化图像
        {

        }

        public void setShowWidget() { //设置显示图像的控件，这里要先于显示图像的函数调用

         //   m_pic_ShowImage = pictureContainer;
           
           // m_objGC = m_pic_ShowImage.CreateGraphics();                 //获取显示的HDC
                                                                        ///m_pHDC = m_objGC.GetHdc();
            image = new Bitmap(pictureBoxWidth, pictureBoxHeight);          //创建缓冲位图
            bitmaapGDI = Graphics.FromImage(image);
        }

        private void _UpdateBufferSize(IBaseData objIBaseData)
        {
            if (null != objIBaseData)
            {

                i_nPayloadSize = (int)objIBaseData.GetPayloadSize();//如果不匹配
                i_imageWidth = (int)objIBaseData.GetWidth();
                i_imageHeight = (int)objIBaseData.GetHeight();


                //更新BitmapInfo
                m_objBitmapInfo.bmiHeader.biWidth = i_imageWidth;
                m_objBitmapInfo.bmiHeader.biHeight = i_imageHeight;
                Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);

            }
        }
        private PixelFormat __GetFormat(bool bIsColor)
        {
            return bIsColor ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;
        }
        private void __CreateBitmap(out Bitmap bitmap, int nWidth, int nHeight, bool bIsColor)
        {
            bitmap = new Bitmap(nWidth, nHeight, __GetFormat(bIsColor));
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette colorPalette = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                {
                    colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = colorPalette;
            }
        }
        private void __UpdateBitmap(Bitmap bitmap, byte[] byBuffer, int nWidth, int nHeight, bool bIsColor)
        {
            //给BitmapData加锁
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //得到一个指向Bitmap的buffer指针
            IntPtr ptrBmp = bmpData.Scan0;
            int nImageStride = __GetStride(i_imageWidth, bIsColor);
            //图像宽能够被4整除直接copy
            if (nImageStride == bmpData.Stride)
            {
                Marshal.Copy(byBuffer, 0, ptrBmp, bmpData.Stride * bitmap.Height);//
            }
            else//图像宽不能够被4整除按照行copy
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(byBuffer, i * nImageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), i_imageWidth);
                }
            }
            //BitmapData解锁
            bitmap.UnlockBits(bmpData);
        }
        //我明白了，就是共享代码的原因，这一次的代码访问到了上一次的数据，导致报错
        public void bufferToBitmap(IBaseData objIBaseData, out Bitmap detectBitmap)
        {
            detectBitmap = null;
            _UpdateBufferSize(objIBaseData);//到这没有问题
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            m_byMonoBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
            m_byColorBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
            if (objIBaseData != null)
            {
                int width = 0, height = 0, payloadsize = 0;
                payloadsize = (int)objIBaseData.GetPayloadSize();
                width = (int)objIBaseData.GetWidth();
                height = (int)objIBaseData.GetHeight();
                if (payloadsize != i_nPayloadSize || width != i_imageWidth || height != i_imageHeight)//如果不匹配
                {
                    i_nPayloadSize = payloadsize;                                          //更新宽高
                    i_imageWidth = width;
                    i_imageHeight = height;
                }
                else
                {
                    __CreateBitmap(out detectBitmap, i_imageWidth, i_imageHeight, b_isColor);
                    //把数据正确传输到bitmap
                    emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                    if (b_isColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);//转换成24位数据
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_imageWidth, b_isColor) * i_imageHeight);
                        //__UpdateBitmapForSave(m_byColorBuffer);
                        __UpdateBitmap(detectBitmap, m_byColorBuffer, i_imageWidth, i_imageHeight, b_isColor);
                    }
                    else
                    {
                        IntPtr pBufferMono = IntPtr.Zero;
                        if (__IsPixelFormat8(objIBaseData.GetPixelFormat()))
                        {
                            pBufferMono = objIBaseData.GetBuffer();
                        }
                        else
                        {
                            pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                        }
                        //到这有问题，提示m_byMonoBuffer为空
                        Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(i_imageWidth, b_isColor) * i_imageHeight);//拷贝数据                    //连续运行这里有个bug（或许需要及时清除掉不需要的变量）
                                                                                                                           //m_byMonoBuffer黑白图
                                                                                                                           //请求的范围扩展超过了数组的结尾。”是不是采集图像的字节长度超出了字节数组的范围2.拍照时图像尺寸一直固定不变吗
                        Console.WriteLine(1);
                        __UpdateBitmap(detectBitmap, m_byMonoBuffer, i_imageWidth, i_imageHeight, b_isColor);
                    }




                }
            }

        }
        public void calculateRealSize(int X,int Y,int scaledWidth,int scaledHeight)                 //根据输入的缩放系数，计算图像裁剪的真实尺寸
        {
            actualX = (int)((double)X / i_scaledX);
            actualY = (int)((double)Y / i_scaledY);
            actualWidth = (int)((double)scaledWidth / i_scaledX);
            actualHeight = (int)((double)scaledHeight / i_scaledY);
        }

        private void showImage(IBaseData objIBaseData)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            //_UpdateBufferSize(objIBaseData);



            //  MessageBox.Show(m_pic_ShowImage.Width.ToString());          //picture是否传递成功?
            //          //bbuffer可以拿到

            //      m_byMonoBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
            // m_byColorBuffer = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
            try
            {
                if (null != objIBaseData)
                {
                    Console.WriteLine("显示图像");
                    emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                    if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                    {
                        if (b_isColor)
                        {
                            IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, true);
                            Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_imageWidth, b_isColor) * i_imageHeight);
                            if (null != m_pHDC)
                            {
                                CWin32Bitmaps.SetStretchBltMode(m_pHDC, COLORONCOLOR);
                                CWin32Bitmaps.StretchDIBits(
                                            m_pHDC,
                                            0,
                                            0,
                                            m_pic_ShowImage.Width,
                                            m_pic_ShowImage.Height,
                                            0,
                                            0,
                                            i_imageWidth,
                                            i_imageHeight,
                                            m_byColorBuffer,
                                            m_pBitmapInfo,
                                            DIB_RGB_COLORS,
                                            SRCCOPY);
                            }
                        }
                        else
                        {

                            IntPtr pBufferMono = IntPtr.Zero;
                            if (__IsPixelFormat8(objIBaseData.GetPixelFormat()))
                            {
                                pBufferMono = objIBaseData.GetBuffer();
                            }
                            else
                            {
                                pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                            }

                            byte[] byMonoBufferTmp = new byte[__GetStride(i_imageWidth, b_isColor) * i_imageHeight];
                            Marshal.Copy(pBufferMono, byMonoBufferTmp, 0, __GetStride(i_imageWidth, b_isColor) * i_imageHeight);

                            // 黑白相机需要翻转数据后显示
                            for (int i = 0; i < i_imageHeight; i++)
                            {
                                Buffer.BlockCopy(byMonoBufferTmp, (i_imageHeight - i - 1) * i_imageWidth, m_byMonoBuffer, i * i_imageWidth, i_imageWidth);                  //这个地方有bug
                            }

                            if (null != m_pHDC)
                            {
                                CWin32Bitmaps.SetStretchBltMode(m_pHDC, COLORONCOLOR);
                                CWin32Bitmaps.StretchDIBits(
                                            m_pHDC,
                                            0,
                                            0,
                                            m_pic_ShowImage.Width,
                                            m_pic_ShowImage.Height,
                                            0,
                                            0,
                                            i_imageWidth,
                                            i_imageHeight,
                                            m_byMonoBuffer,
                                            m_pBitmapInfo,
                                            DIB_RGB_COLORS,
                                            SRCCOPY);
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine(1);
            }

      
        }
        private GX_VALID_BIT_LIST __GetBestValudBit(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            switch (emPixelFormatEntry)
            {
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG8:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG10:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_2_9;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG12:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_4_11;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO14:
                    {
                        //暂时没有这样的数据格式待升级
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG16:
                    {
                        //暂时没有这样的数据格式待升级
                        break;
                    }
                default:
                    break;
            }
            return emValidBits;
        }
        private int __GetStride(int nWidth, bool bIsColor)
        {
            return bIsColor ? nWidth * 3 : nWidth;
        }



        private void resetCameraParam(s_CameraParam cameraParam)
        {
            bool bIsImplemented = false;
            try
            {
                foreach (System.Reflection.PropertyInfo p in cameraParam.GetType().GetProperties())
                {
                    switch (p.Name)
                    {
                        case "Width":
                            bIsImplemented = o_objIGXFeatureControl.IsImplemented("Width");
                            if (bIsImplemented)
                            {
                                IIntFeature objInt = o_objIGXFeatureControl.GetIntFeature("Width");
                                objInt.SetValue(cameraParam.Width);//设置当前值

                            }

                            break;
                        case "Height":
                            bIsImplemented = o_objIGXFeatureControl.IsImplemented("Height");
                            if (bIsImplemented)
                            {
                                IIntFeature objInt = o_objIGXFeatureControl.GetIntFeature("Height");
                                objInt.SetValue(cameraParam.Height);//设置当前值

                            }
                            break;
                        case "PixelFormat":
                            IEnumFeature objEnum = o_objIGXFeatureControl.GetEnumFeature("PixelFormat");
                            objEnum.SetValue(cameraParam.PixelFormat);//设置当前项
                            break;

                    }
                }
            }
            catch
            {

            }

        }





    }
}
////还得有专门的瓶口的相机、瓶底相机数组，瓶身相机数组
///


