using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GxIAPINET;
using System.Drawing;
using System.Drawing.Imaging;
using bottleDetection.Common;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO.Ports;
using HalconDotNet;
using static HalconDotNet.HOperatorSet;
//计算标准差以及计算方差的意义的是什么?


namespace bottleDetect.ConfigInfo
{

    

    //PLC设备相关
    struct s_ConfigPLCInfo
    {
        public short iPlcId;
        public string strPlcName;
        public string strPlcInitFilePath;//设备初始化文件路径

    };

    struct s_SystemInfo
    {
        public Mutex m_mutexSystemInfo;
        public int m_Test;
        public bool m_bDebugMode;
        public int m_iSystemType;           //系统类型 0:瓶身系统，1：瓶口瓶底系统，2：一体机系统(14相机)，3：一体机系统(10相机),4:瓶身2光电，5：药玻一体机,6: 瓶身单独应力一体机。
        public short iRealCamCount;        //真实相机个数
        public short iCamCount;            //切割后相机个数

        public bool m_bIsUsePLC;           //
        public bool m_bIsPLCOk;            // PLC状态是否正常
        public bool m_bIsPLCOK;         //是否可用接口卡
        public bool m_bIsStopNeedPermission;   //停止检测是否需要权限
        public bool m_bIsCarve;            //是否切割
        public bool m_bIsTest;             //是否测试模式 （未使用）
                                           //	BOOL m_bUseHighClock;		//是否使用高精度时间
        public int iIOCardCount;           //接口卡个数
        public int iIsButtomStress;        //是否有瓶底应力
        public int iIsSample;              //是否有取样功能
        public int iIsSaveCountInfoByTime; //是否保存指定时间段内的统计信息
        public int iIsCameraCount;         //是否保存各个相机的统计信息
        public string LastModelName;      //上次使用模板
        public string m_strModelName;     //模板名
        public int m_iSaveNormalErrorImageByTime;
        public int m_iSaveStressErrorImageByTime;
        // bool m_bSaveCamera[35];
        public List<bool> m_bSaveCamera;
        //  bool m_bSaveErrorType[ERRORTYPE_MAX_COUNT];
        public List<bool> m_bSaveErrorType;



    }

 
    struct s_RuningInfo
    {
        public Mutex m_mutexRunningInfo;
        public Mutex m_mutexTestInfo;

        public int m_iLastIOCard1IN0;
        public int m_iLastIOCard2IN0;
        public int m_iPermission;//权限
        public int m_checkedNum;
        public int m_passNum;
        public int m_failureNum;
        public int m_checkedNum2;
        public int m_passNum2;
        public int m_failureNum2;
        public int m_failureNumFromIOcard;
        public int m_kickoutNumber;

        public int m_iKickMode;//踢废模式0:连续踢 1:隔瓶踢	2:持续好 3：正常踢
        public int m_iKickMode2;//踢废模式0:连续踢 1:隔瓶踢	2:持续好 3：正常踢
        public bool m_bCheck;//是否正在检测，0停止检测，1：正在检测。
       // public bool m_bIsCheck[CAMERA_MAX_COUNT];//此相机是否正在检测
        public List<int> nModelReadFailureNumber;//踢废模式0:连续踢 1:隔瓶踢	2:持续好 3：正常踢
       // public int[99] nModelChenkedNumber;//踢废模式0:连续踢 1:隔瓶踢	2:持续好 3：正常踢

        //确定c#中如何定义数组;确定c#中如何定义公共变量
        //现在的问题就是：1.会使用界面布局2.能够从相机读取图片3.界面控件随自动缩放
        //传动、识别、剔除
        //传统算法关键就在于定位
    }
    //相机结构体应包含:1.相机SN号2.相机IP地址3.相机名4.该相机绑定的相机流5.该相机对应的图片队列6.该相机检测的正确的瓶子数7.该相机检测的图片总数8.该相机检测的瑕疵瓶子数9.该相机工作状态(正常or掉线)
    //该相机绑定的回调函数
    //参数应该被初始化
    public struct i_RealCamera
    {
        public String strSN;            //相机SN号
        public String strUserID;        //相机ID
        public String strMAC;           //相机MAC地址
        public String strIP;            //相机IP地址

        public   IGXDevice objDevice;            //相机指针

        public int i_camType;             //相机类型

        public int   i_camCount;        //拍照总数
                                         //
        public int i_imageWidth;          //图片宽
        public int i_imageHeight;         //图片高
        public int i_nPayloadSize;        //图片像素尺寸
        

        public IGXStream o_objIGXStream;          //相机指向流地址
        public IGXFeatureControl o_objIGXFeatureControl ;     //相机属性控制

        public bool b_isColor;             //是否是彩色相机

        public bool b_isStress;             //是否是应力相机





    }
    //c_imageDataCollect负责图像的处理,处理图像以及保存和显示图像
    //回调函数触发后,Form写入到缓冲区如果需要保存由c_imageDataProcess保存
    //这个类是作为通用的图像处理类，来一个IFrameData处理一个
    //这个类需要改一下。需要保存功能、字节转bitmap、比较传进来的图像是否和默认配置相等、创建bitmap，将字节转为himage
   //目前这个类就是需要来一个处理一个
    public class c_imageDataProcess
    {
      
       public byte[] m_byRawBuffer = null;                ///<用于存储Raw图的Buffer
       public int i_nPayloadSize = 0;                   ///<图像数据大小
       private int i_nWidth = 0;                   ///<图像宽度
       private int i_nHeigh = 0;                   ///<图像高度
       public Bitmap m_bitmapForSave = null;                ///<bitmap对象,仅供存储图像使用
       public byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
       public byte[] m_byColorBuffer = null;                ///<彩色相机buffer

       public int i_imageIndex;                     //图像号
       public int i_RealCamID;                       //采出该图像对应的相机号
       public bool b_IsColor;                       //是否是彩色数据
        //采集时间
       public bool b_isStree;                   //是否是应力数据


        const uint PIXEL_FORMATE_BIT = 0x00FF0000;          ///<用于与当前的数据格式进行与运算得到当前的数据位数
        const uint GX_PIXEL_8BIT = 0x00080000;          ///<8位数据图像格式
                                                        ///
        const int COLORONCOLOR = 3;
        const uint DIB_RGB_COLORS = 0;
        const uint SRCCOPY = 0x00CC0020;
        CWin32Bitmaps.BITMAPINFO m_objBitmapInfo = new CWin32Bitmaps.BITMAPINFO();
        IntPtr m_pBitmapInfo = IntPtr.Zero;
        Graphics m_objGC = null;
        IntPtr m_pHDC = IntPtr.Zero;

        //可以需要多个c_imageDataCollect，需要就实例化
        public c_imageDataProcess(int imageWidth, int imageHeight ,int PayloadSize,bool isColor)
        {
            
            i_nWidth = imageWidth;
            i_nHeigh = imageHeight;
            i_nPayloadSize = PayloadSize;
            b_IsColor = isColor;


            //创建Bitmap结构体，为下一步保存或显示图像
            m_objBitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(CWin32Bitmaps.BITMAPINFOHEADER));
            m_objBitmapInfo.bmiHeader.biWidth = i_nWidth;
            m_objBitmapInfo.bmiHeader.biHeight = i_nHeigh;
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
            m_pBitmapInfo = Marshal.AllocHGlobal(2048);
            Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);


        }
        public void bufferToBitmap(IBaseData objIBaseData)              //将字节转为bitmap
        {
            Bitmap bitmap;
            __CreateBitmap(out bitmap, i_nWidth, i_nHeigh, b_IsColor);

            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            //_UpdateBufferSize
            _UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                //这里验证了objBaseData不为空，因此可以排除空字节的错误
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (b_IsColor)
                {
                    IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                    __UpdateBitmapForSave(m_byColorBuffer);
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
                    Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);          //这一行代码有报错

                    __UpdateBitmapForSave(m_byMonoBuffer);
                }
               
            }


        }


        //IFrameData objIFrameData
        //将采集到的数据转为HAlCON能处理的HObject

        public void bufferToHimage(in IFrameData objIFrameData, out HObject outImage)
        {
            GenEmptyObj(out outImage);


            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;


            //检查图像是否改变并更新Buffer
            //_UpdateBufferSize
            _UpdateBufferSize(objIFrameData);

            if (null != objIFrameData)
            {
                emValidBits = __GetBestValudBit(objIFrameData.GetPixelFormat());
                if (b_IsColor)//转成彩色图HObject
                {
                    IntPtr pBufferColor = objIFrameData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                    //这里为什么要调用Marshal.copy
                    //Marshal.Copy将非托管内存的数据复制到托管内存，这样做目的是图像字节数据可以被系统自动回收
                    __UpdateBitmapForSave(m_byColorBuffer);
                    GenImageInterleaved(out outImage, pBufferColor, "bgr", i_nWidth, i_nHeigh, 0, "byte", 0, 0, 0, 0, -1, 0);
                }
                else
                {
                    IntPtr pBufferMono = IntPtr.Zero;
                    if (__IsPixelFormat8(objIFrameData.GetPixelFormat()))
                    {
                        pBufferMono = objIFrameData.GetBuffer();
                    }
                    else
                    {
                        pBufferMono = objIFrameData.ConvertToRaw8(emValidBits);
                    }
                   GenImage1(out outImage, new HalconDotNet.HTuple("byte"), i_nWidth, i_nHeigh, pBufferMono);
                }

            }


        }
        public void  saveBMP(IBaseData objIBaseData, string strFilePath)
        {
            //saveBMP流程1.拿到数据转成ConvertToRGB24 2.将数据考到内存3.创建bitmap对象
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            //_UpdateBufferSize
            _UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (b_IsColor)
                {
                    IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                    __UpdateBitmapForSave(m_byColorBuffer);
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
                    Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);//拷贝数据
                    //m_byMonoBuffer黑白图
                    __UpdateBitmapForSave(m_byMonoBuffer);
                }
                m_bitmapForSave.Save(strFilePath, ImageFormat.Bmp);
            }
        }
        private void __UpdateBitmapForSave(byte[] byBuffer)                 //检查buffen是否需要更新
        {
            if (__IsCompatible(m_bitmapForSave, i_nWidth, i_nHeigh, b_IsColor))
            {
                __UpdateBitmap(m_bitmapForSave, byBuffer, i_nWidth, i_nHeigh, b_IsColor);
            }
            else
            {
                //m_bitmapForSave由__CreateBitmap创建bitmap对象
                __CreateBitmap(out m_bitmapForSave, i_nWidth, i_nHeigh, b_IsColor);
                __UpdateBitmap(m_bitmapForSave, byBuffer, i_nWidth, i_nHeigh, b_IsColor);
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
        //将buffer数据拷贝到m_bitmapforsave，用来更新待保存的bitmap
        private void __UpdateBitmap(Bitmap bitmap, byte[] byBuffer, int nWidth, int nHeight, bool bIsColor)
        {
            //给BitmapData加锁
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            //得到一个指向Bitmap的buffer指针
            IntPtr ptrBmp = bmpData.Scan0;
            int nImageStride = __GetStride(i_nWidth, bIsColor);
            //图像宽能够被4整除直接copy
            if (nImageStride == bmpData.Stride)
            {
                Marshal.Copy(byBuffer, 0, ptrBmp, bmpData.Stride * bitmap.Height);//
            }
            else//图像宽不能够被4整除按照行copy
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(byBuffer, i * nImageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), i_nWidth);
                }
            }
            //BitmapData解锁
            bitmap.UnlockBits(bmpData);
        }

        private void _UpdateBufferSize(IBaseData objIBaseData)
        {
            if (null != objIBaseData)
            {
                if (__IsCompatible(m_bitmapForSave, i_nWidth, i_nHeigh, b_IsColor))//判断是否兼容，如果匹配
                {
                    i_nPayloadSize = (int)objIBaseData.GetPayloadSize();
                    i_nWidth = (int)objIBaseData.GetWidth();
                    i_nHeigh = (int)objIBaseData.GetHeight();
                }
                else
                {
                    i_nPayloadSize = (int)objIBaseData.GetPayloadSize();//如果不匹配
                    i_nWidth = (int)objIBaseData.GetWidth();
                    i_nHeigh = (int)objIBaseData.GetHeight();

                    
                    m_byRawBuffer = new byte[i_nPayloadSize];
                    m_byMonoBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
                    m_byColorBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];

                    //更新BitmapInfo
                    m_objBitmapInfo.bmiHeader.biWidth = i_nWidth;
                    m_objBitmapInfo.bmiHeader.biHeight = i_nHeigh;
                    Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);          //将数据块从托管内存传输到非托管内存
                }
            }
        }

        private bool __IsCompatible(Bitmap bitmap, int nWidth, int nHeight, bool bIsColor)
        {
            if (bitmap == null
                || bitmap.Height != nHeight
                || bitmap.Width != nWidth
                || bitmap.PixelFormat != __GetFormat(bIsColor)
             )
            {
                return false;
            }
            return true;
        }
        private int __GetStride(int nWidth, bool bIsColor)
        {
            return bIsColor ? nWidth * 3 : nWidth;
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

    }

    public class imageProcesscls
    {

       
        public int i_nPayloadSize = 0;                   ///<图像数据大小
        private int i_nWidth = 0;                   ///<图像宽度
        private int i_nHeigh = 0;                   ///<图像高度
        public bool b_IsColor = false;                       //是否是彩色数据，默认黑白
        const uint PIXEL_FORMATE_BIT = 0x00FF0000;          ///<用于与当前的数据格式进行与运算得到当前的数据位数
        const uint GX_PIXEL_8BIT = 0x00080000;          ///<8位数据图像格式
        const int COLORONCOLOR = 3;
        const uint DIB_RGB_COLORS = 0;
        const uint SRCCOPY = 0x00CC0020;
        CWin32Bitmaps.BITMAPINFO m_objBitmapInfo = new CWin32Bitmaps.BITMAPINFO();
        IntPtr m_pBitmapInfo = IntPtr.Zero;
       

        public imageProcesscls(int imageWidth, int imageHeight, int PayloadSize, bool isColor)
        {
            i_nWidth = imageWidth;
            i_nHeigh = imageHeight;
            i_nPayloadSize = PayloadSize;
            b_IsColor = isColor;


            if (isColor)
            {
                m_objBitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(CWin32Bitmaps.BITMAPINFOHEADER));
                m_objBitmapInfo.bmiHeader.biWidth = i_nWidth;
                m_objBitmapInfo.bmiHeader.biHeight = i_nHeigh;
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
                m_objBitmapInfo.bmiHeader.biWidth = i_nWidth;
                m_objBitmapInfo.bmiHeader.biHeight = i_nHeigh;
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

        }

        private bool __IsCompatible(Bitmap bitmap, int nWidth, int nHeight, bool bIsColor)
        {
            if (bitmap == null
                || bitmap.Height != nHeight
                || bitmap.Width != nWidth
                || bitmap.PixelFormat != __GetFormat(bIsColor)
             )
            {
                return false;
            }
            return true;
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
        private PixelFormat __GetFormat(bool bIsColor)
        {
            return bIsColor ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;
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
            int nImageStride = __GetStride(i_nWidth, bIsColor);
            //图像宽能够被4整除直接copy
            if (nImageStride == bmpData.Stride)
            {
                Marshal.Copy(byBuffer, 0, ptrBmp, bmpData.Stride * bitmap.Height);//
            }
            else//图像宽不能够被4整除按照行copy
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(byBuffer, i * nImageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), i_nWidth);
                }
            }
            //BitmapData解锁
            bitmap.UnlockBits(bmpData);
        }
        private void __ShowImage(byte[] byBuffer)
        {
           
        }

        private void _UpdateBufferSize(IBaseData objIBaseData)
        {
            if (null != objIBaseData)
            {
          
                    i_nPayloadSize = (int)objIBaseData.GetPayloadSize();//如果不匹配
                    i_nWidth = (int)objIBaseData.GetWidth();
                    i_nHeigh = (int)objIBaseData.GetHeight();


                    //更新BitmapInfo
                    m_objBitmapInfo.bmiHeader.biWidth = i_nWidth;
                    m_objBitmapInfo.bmiHeader.biHeight = i_nHeigh;
                    Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);
                
            }
        }
        public void bufferToBitmap(IBaseData objIBaseData,out Bitmap detectBitmap) //将字节转为bitmap
        {

            detectBitmap = null;
            //1.测试传进来的字节数组与创建对象时大小是否相等
            //2.创建bitmap对象
            //3.保存对象
            _UpdateBufferSize(objIBaseData);
            int width = 0, height = 0, payloadsize = 0;
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
            byte[] m_byColorBuffer = null;                ///<彩色相机buffer

            m_byMonoBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            m_byColorBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            if (objIBaseData != null)
            {
                payloadsize = (int)objIBaseData.GetPayloadSize();
                width = (int)objIBaseData.GetWidth();
                height = (int)objIBaseData.GetHeight();
                if (payloadsize != i_nPayloadSize || width != i_nWidth || height != i_nHeigh)//如果不匹配
                {
                    i_nPayloadSize = payloadsize;                                          //更新宽高
                    i_nWidth = width;
                    i_nHeigh = height;
                }
                else
                {
                    __CreateBitmap(out detectBitmap, i_nWidth, i_nHeigh, b_IsColor);
                    //把数据正确传输到bitmap
                    emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                    if (b_IsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);//转换成24位数据
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                        //__UpdateBitmapForSave(m_byColorBuffer);
                        __UpdateBitmap(detectBitmap, m_byColorBuffer, i_nWidth, i_nHeigh, b_IsColor);
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
                        Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);//拷贝数据                    //连续运行这里有个bug（或许需要及时清除掉不需要的变量）
                                                                                                                  //m_byMonoBuffer黑白图
                                                                                                                  //请求的范围扩展超过了数组的结尾。”是不是采集图像的字节长度超出了字节数组的范围2.拍照时图像尺寸一直固定不变吗

                        __UpdateBitmap(detectBitmap, m_byMonoBuffer, i_nWidth, i_nHeigh, b_IsColor);
                    }
                  



                }
            }
            else
            {
                //这里应该写入日志
                return;
            }


        }

        public void Show(IBaseData objIBaseData,ref PictureBox pictureContainer)                    //显示图像
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            _UpdateBufferSize(objIBaseData);
            Graphics m_objGC = null;
            IntPtr m_pHDC = IntPtr.Zero;
            m_objGC = pictureContainer.CreateGraphics();                 //获取显示的HDC
            m_pHDC = m_objGC.GetHdc();
            byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
            byte[] m_byColorBuffer = null;                ///<彩色相机buffer
            m_byMonoBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            m_byColorBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                {
                    if (b_IsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, true);
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                        if (null != pictureContainer)
                        {
                            CWin32Bitmaps.SetStretchBltMode(m_pHDC, COLORONCOLOR);
                            CWin32Bitmaps.StretchDIBits(
                                        m_pHDC,
                                        0,
                                        0,
                                        pictureContainer.Width,
                                        pictureContainer.Height,
                                        0,
                                        0,
                                        i_nWidth,
                                        i_nHeigh,
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

                        byte[] byMonoBufferTmp = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
                        Marshal.Copy(pBufferMono, byMonoBufferTmp, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);

                        // 黑白相机需要翻转数据后显示
                        for (int i = 0; i < i_nHeigh; i++)
                        {
                            Buffer.BlockCopy(byMonoBufferTmp, (i_nHeigh - i - 1) * i_nWidth, m_byMonoBuffer, i * i_nWidth, i_nWidth);
                        }

                        if (null != pictureContainer)
                        {
                            CWin32Bitmaps.SetStretchBltMode(m_pHDC, COLORONCOLOR);
                            CWin32Bitmaps.StretchDIBits(
                                        m_pHDC,
                                        0,
                                        0,
                                        pictureContainer.Width,
                                        pictureContainer.Height,
                                        0,
                                        0,
                                        i_nWidth,
                                        i_nHeigh,
                                        m_byMonoBuffer,
                                        m_pBitmapInfo,
                                        DIB_RGB_COLORS,
                                        SRCCOPY);
                        }
                    }
                }
            }
        }




        public void saveBMP(IBaseData objIBaseData, string strFilePath)
        {
            //1.测试传进来的字节数组与创建对象时大小是否相等
            //2.创建bitmap对象
            //3.保存对象
            Bitmap bitmapForSave = null;
            
            int width = 0, height = 0, payloadsize = 0;
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
            byte[] m_byColorBuffer = null;                ///<彩色相机buffer
            _UpdateBufferSize(objIBaseData);
            m_byMonoBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            m_byColorBuffer = new byte[__GetStride(i_nWidth, b_IsColor) * i_nHeigh];
            if (objIBaseData != null)
            {
                payloadsize = (int)objIBaseData.GetPayloadSize();
                width = (int)objIBaseData.GetWidth();
                height = (int)objIBaseData.GetHeight();
                if (payloadsize != i_nPayloadSize || width != i_nWidth || height != i_nHeigh)//如果不匹配
                {
                    i_nPayloadSize  = payloadsize;                                          //更新宽高
                    i_nWidth =  width;
                    i_nHeigh = height;
                }
                else
                {
                    __CreateBitmap( out bitmapForSave,i_nWidth,i_nHeigh,b_IsColor);
                    //把数据正确传输到bitmap
                    emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                    if (b_IsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);//转换成24位数据
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);
                        //__UpdateBitmapForSave(m_byColorBuffer);
                        __UpdateBitmap(bitmapForSave, m_byColorBuffer, i_nWidth, i_nHeigh, b_IsColor);
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
                        Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(i_nWidth, b_IsColor) * i_nHeigh);//拷贝数据
                                                                                                                  //m_byMonoBuffer黑白图
                        __UpdateBitmap(bitmapForSave, m_byMonoBuffer, i_nWidth, i_nHeigh, b_IsColor);
                    }
                    bitmapForSave.Save(strFilePath, ImageFormat.Bmp);                                               //保存报错可能是因为太快了，也不是因为太快增加PLC触发还是报错



                }
            }
            else
            {
               //这里应该写入日志
                return;
            }

        }

    }



    public struct s_imageData
    {
        public int i_imageIndex;                     //图像号
        public int i_RealCamID;                       //采出该图像对应的相机号(IP)
        public bool b_IsColor;                         //是否是彩色图像，true代表图像
        //采集时间
        public bool b_isStree;                   //是否是应力图像
        public bool b_wave;                       //
        public int i_nPayloadSize;                   ///<图像数据大小
        private int i_nWidth ;                   ///<图像宽度
        private int i_nHeigh ;                   ///<图像高度

       // public byte[] m_byMonoBuffer;                ///<黑白相机buffer
     //   public byte[] m_byRawBuffer;                ///<用于存储Raw图的Buffer

        public Bitmap bitmap_ImageData;                 //待处理位图图像

        public int i_camType;                           //相机类型
        
    }

    public struct s_camerConfig
    {
        //如果要使用反射获取结构体成员，需要加上{get;set}
        public int i_Width { get; set; }
        public int i_Height{ get; set; }

    }

    public enum e_pixelFormat {
         Mono8,
        Mono12,


    }

    //注意大恒相机属性控制器类型和属性数据类型
    //
    public struct s_CameraParam
    {
        static int SensorWidth = 2048;
        static int Sensorheight = 1536;
        static int WidthMax = 2048;
        static int HeightMax = 1536;
        static string PixelSize = "Bpp8";

        public int OffsetX { get; set; }    //垂直偏移
        public int OffsetY { get; set; }    //水平偏移
        public int Width { get; set; }      //宽度
        public int Height { get; set; }     //高度
        public string PixelFormat { get; set; }         //像素深度
        public string BinningVerticalMode { get; set; }
        public string BinningHorizontalMode { get; set; }

        public string TriggerSource { get; set; }         //触发源

        public string TriggerActivation { get; set; }         //触发极性

        
        public int BinningHorizontal { get; set; }
        public int BinningVertical { get; set; }
        public int DecimationHorizontal { get; set; }
        public int DecimationVertical { get; set; }
        public bool ReverseX { get; set; }
        public bool ReverseY { get; set; }





    }


    public struct s_lightConfig
    {

        int i_Width;
        int i_Height;

    }

    //假设现在要设置的字段都已经定义好了，该怎么设置到相机呢?


    enum winStatus
    {
        bottle = 0,
        set,
        start,
        exit,
        management,
        alg,
        plc,
        debug,
        home1,
        lock1






    }


    enum e_camType
    {
        normal = 0,     //正常图像
        stress          //应力图像
    }
    //属性控制可以用来获取初始值从而保存到自定义结构体中

    enum e_bottlePosition { //瓶子位置，瓶口、瓶底
        bottle_mouth = 0,     //瓶口
        bottle_neck,           //瓶脖
        bottle_body,          //瓶身
        bottle_bottom          //瓶底



    }


    enum e_openMode
    {
        SN = 0,
        MAC,
        UserID,
        IP
    }

    enum e_truggerMode
    {
        hard = 0,
        software


    }


    public struct Rect
    { //定义检测矩形
        public int iLeftPointRow;
        public int iLeftPointCol;
        public int Width;
        public int Height;

    }
    public struct MyPoint
    {
        public int Row;
        public int Col;

    }
    public struct MyLine
    {
        public MyPoint startPoint;
        public MyPoint endPoint;
    }
    public struct locationBox
    {
        public Rect rect;
        public List<MyLine> lines;
    }
    //如何继承结构体
    public enum e_controlBit            //图形控制位
    {
        drawing,                        //0:绘制
        draging,                        //1:拖动
        scaling,                        //缩放
        strecthX,                       //X轴拉伸
        strecthY                       //Y轴拉伸
    }


    public enum e_DrawType
    {
        //绘图类型定义
        DRAW_LINE = 1,      //线段
        DRAW_RECTANGLE = 2, //矩形
        DRAW_CIRCLE = 3,        //圆形
        DRAW_POLYGON = 4,       //多边形
        DRAW_REGION = 5      //区域
    }

    public enum e_LineDirect
    {
        L2R = 1,
        R2L = 2,
        T2B = 3,
        B2T = 4
    }
    //重绘事件
}
