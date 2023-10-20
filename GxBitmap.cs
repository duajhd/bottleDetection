using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using GxIAPINET;
using bottleDetection.Common;

namespace GxIAPINET.Sample.Common
{
    public class GxBitmap
    {

        IGXDevice m_objIGXDevice = null;                ///<设备对像
        PictureBox m_pic_ShowImage = null;                ///<图片显示控件
        bool m_bIsColor = false;               ///<是否支持彩色相机
        byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
        byte[] m_byColorBuffer = null;                ///<彩色相机buffer
        byte[] m_byRawBuffer = null;                ///<用于存储Raw图的Buffer
        int m_nPayloadSize = 0;                   ///<图像数据大小
        int m_nWidth = 0;                   ///<图像宽度
        int m_nHeigh = 0;                   ///<图像高度
        Bitmap m_bitmapForSave = null;                ///<bitmap对象,仅供存储图像使用
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


        /// <summary>
        /// 构造函数用于初始化设备对象与PictureBox控件对象
        /// </summary>
        /// <param name="objIGXDevice">设备对象</param>
        /// <param name="objPictureBox">图像显示控件</param>
        public GxBitmap(IGXDevice objIGXDevice, PictureBox objPictureBox)
        {
            m_objIGXDevice = objIGXDevice;
            m_pic_ShowImage = objPictureBox;
            if (null != objIGXDevice)
            {
                //获得图像原始数据大小、宽度、高度等
                m_nPayloadSize = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("PayloadSize").GetValue();
                m_nWidth = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("Width").GetValue();
                m_nHeigh = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();

                //获取是否为彩色相机
                __IsSupportColor(ref m_bIsColor);
            }

            //申请用于缓存图像数据的buffer
            m_byRawBuffer = new byte[m_nPayloadSize];
            m_byMonoBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
            m_byColorBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];

            __CreateBitmap(out m_bitmapForSave, m_nWidth, m_nHeigh, m_bIsColor);

            m_objGC = m_pic_ShowImage.CreateGraphics();                 //获取显示的HDC
            m_pHDC = m_objGC.GetHdc();
            if (m_bIsColor)
            {
                m_objBitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(CWin32Bitmaps.BITMAPINFOHEADER));
                m_objBitmapInfo.bmiHeader.biWidth = m_nWidth;
                m_objBitmapInfo.bmiHeader.biHeight = m_nHeigh;
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
                m_objBitmapInfo.bmiHeader.biWidth = m_nWidth;
                m_objBitmapInfo.bmiHeader.biHeight = m_nHeigh;
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

        /// <summary>
        /// 用于图像处理后并显示图像
        /// </summary>
        /// <param name="objCfg">图像处理参数配置对象</param>
        /// <param name="objIBaseData">图像数据对象</param>
        public void ShowImageProcess(IImageProcessConfig objCfg, IBaseData objIBaseData)
        {
            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                {
                    if (m_bIsColor)
                    {
                        objCfg.EnableConvertFlip(true);
                        IntPtr pBufferColor = objIBaseData.ImageProcess(objCfg);
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byColorBuffer);
                    }
                    else
                    {
                        byte[] byMonoBufferTmp = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
                        IntPtr pBufferMono = objIBaseData.ImageProcess(objCfg);
                        Marshal.Copy(pBufferMono, byMonoBufferTmp, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);

                        // 黑白相机需要翻转数据后显示
                        for (int i = 0; i < m_nHeigh; i++)
                        {
                            Buffer.BlockCopy(byMonoBufferTmp, (m_nHeigh - i - 1) * m_nWidth, m_byMonoBuffer, i * m_nWidth, m_nWidth);
                        }

                        __ShowImage(m_byMonoBuffer);
                    }
                }
            }
        }


        /// <summary>
        /// 用于显示图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        /// 显示图像从接受
        public void Show(IBaseData objIBaseData)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);


            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                //获取字节数据的状态
                if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                {
                    if (m_bIsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, true);
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byColorBuffer);
                    }
                    else
                    {
                        IntPtr pBufferMono = IntPtr.Zero;
                        if (__IsPixelFormat8(objIBaseData.GetPixelFormat()))            //获取字节指针
                        {
                            pBufferMono = objIBaseData.GetBuffer();
                        }
                        else
                        {
                            pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                        }
                        //显示图像的步骤0.构建图像显示的bitmap结构1.获取图像的字节数据2.将字节数据保存到数组3.调用showImage函数
                        byte[] byMonoBufferTmp = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
                        Marshal.Copy(pBufferMono, byMonoBufferTmp, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        //这周先补营养，补蛋白(要注意：胃有炎症时补蛋白没有效果)
                        //空瓶检测算法应该也挺简单的
                        // 黑白相机需要翻转数据后显示
                        for (int i = 0; i < m_nHeigh; i++)
                        {
                            Buffer.BlockCopy(byMonoBufferTmp, (m_nHeigh - i - 1) * m_nWidth, m_byMonoBuffer, i * m_nWidth, m_nWidth);
                        }

                        __ShowImage(m_byMonoBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// 存储图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        /// <param name="strFilePath">显示图像文件名</param>
        public void SaveBmp(IBaseData objIBaseData, string strFilePath)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (m_bIsColor)
                {
                    IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
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
                    Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);

                    __UpdateBitmapForSave(m_byMonoBuffer);
                }
                m_bitmapForSave.Save(strFilePath, ImageFormat.Bmp);
            }
        }

        /// <summary>
        /// 存储Raw图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        /// <param name="strFilePath">显示图像文件名</param>
        public void SaveRaw(IBaseData objIBaseData, string strFilePath)
        {
            Stream objFileStream = new FileStream(strFilePath, FileMode.Create);
            BinaryWriter objSW = new BinaryWriter(objFileStream);

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                IntPtr pBufferRaw = objIBaseData.GetBuffer();
                Marshal.Copy(pBufferRaw, m_byRawBuffer, 0, m_nPayloadSize);
            }

            objSW.Write(m_byRawBuffer);
            objSW.Close();
            objFileStream.Close();
        }

        /// <summary>
        /// 检查图像是否改变并更新Buffer
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        private void __UpdateBufferSize(IBaseData objIBaseData)
        {
            if (null != objIBaseData)
            {
                if (__IsCompatible(m_bitmapForSave, m_nWidth, m_nHeigh, m_bIsColor))//判断是否兼容
                {
                    m_nPayloadSize = (int)objIBaseData.GetPayloadSize();
                    m_nWidth = (int)objIBaseData.GetWidth();
                    m_nHeigh = (int)objIBaseData.GetHeight();
                }
                else
                {
                    m_nPayloadSize = (int)objIBaseData.GetPayloadSize();
                    m_nWidth = (int)objIBaseData.GetWidth();
                    m_nHeigh = (int)objIBaseData.GetHeight();

                    m_byRawBuffer = new byte[m_nPayloadSize];
                    m_byMonoBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
                    m_byColorBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];

                    //更新BitmapInfo
                    m_objBitmapInfo.bmiHeader.biWidth = m_nWidth;
                    m_objBitmapInfo.bmiHeader.biHeight = m_nHeigh;
                    Marshal.StructureToPtr(m_objBitmapInfo, m_pBitmapInfo, false);
                }
            }
        }

        /// <summary>
        /// 更新存储数据
        /// </summary>
        /// <param name="byBuffer">图像buffer</param>
        private void __UpdateBitmapForSave(byte[] byBuffer)
        {
            if (__IsCompatible(m_bitmapForSave, m_nWidth, m_nHeigh, m_bIsColor))
            {
                __UpdateBitmap(m_bitmapForSave, byBuffer, m_nWidth, m_nHeigh, m_bIsColor);
            }
            else
            {
                __CreateBitmap(out m_bitmapForSave, m_nWidth, m_nHeigh, m_bIsColor);
                __UpdateBitmap(m_bitmapForSave, byBuffer, m_nWidth, m_nHeigh, m_bIsColor);
            }
        }

        /// <summary>
        /// 显示图像处理
        /// </summary>
        /// <param name="byBuffer">图像数据buffer</param>
        private void __ShowImage(byte[] byBuffer)
        {
            if (null != m_pic_ShowImage)
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
                            m_nWidth,
                            m_nHeigh,
                            byBuffer,
                            m_pBitmapInfo,
                            DIB_RGB_COLORS,
                            SRCCOPY);
            }
        }

        /// <summary>
        /// 判断PixelFormat是否为8位
        /// </summary>
        /// <param name="emPixelFormatEntry">图像数据格式</param>
        /// <returns>true为8为数据，false为非8位数据</returns>
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

        /// <summary>
        /// 通过GX_PIXEL_FORMAT_ENTRY获取最优Bit位
        /// </summary>
        /// <param name="em">图像数据格式</param>
        /// <returns>最优Bit位</returns>
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

        /// <summary>
        /// 获取图像显示格式
        /// </summary>
        /// <param name="bIsColor">是否为彩色相机</param>
        /// <returns>图像的数据格式</returns>
        private PixelFormat __GetFormat(bool bIsColor)
        {
            return bIsColor ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;
        }

        /// <summary>
        /// 计算宽度所占的字节数
        /// </summary>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        /// <returns>图像一行所占的字节数</returns>
        private int __GetStride(int nWidth, bool bIsColor)
        {
            return bIsColor ? nWidth * 3 : nWidth;
        }

        /// <summary>
        /// 判断是否兼容
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        /// <returns>true为一样，false不一样</returns>
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

        /// <summary>
        /// 创建Bitmap
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
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

        /// <summary>
        /// 更新和复制图像数据到Bitmap的buffer
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        private void __UpdateBitmap(Bitmap bitmap, byte[] byBuffer, int nWidth, int nHeight, bool bIsColor)
        {
            //给BitmapData加锁
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            //得到一个指向Bitmap的buffer指针
            IntPtr ptrBmp = bmpData.Scan0;
            int nImageStride = __GetStride(m_nWidth, bIsColor);
            //8位步长就是宽度，彩图步长乘以3
            //图像宽能够被4整除直接copy
            if (nImageStride == bmpData.Stride)
            {
                Marshal.Copy(byBuffer, 0, ptrBmp, bmpData.Stride * bitmap.Height);
            }
            else//图像宽不能够被4整除按照行copy
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(byBuffer, i * nImageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), m_nWidth);
                }
            }
            //BitmapData解锁
            bitmap.UnlockBits(bmpData);
        }

        /// <summary>
        /// 是否支持彩色
        /// </summary>
        /// <param name="bIsColorFilter">是否支持彩色</param>
        private void __IsSupportColor(ref bool bIsColorFilter)
        {
            bool bIsImplemented = false;
            bool bIsMono = false;
            string strPixelFormat = "";

            strPixelFormat = m_objIGXDevice.GetRemoteFeatureControl().GetEnumFeature("PixelFormat").GetValue();
            if (0 == string.Compare(strPixelFormat, 0, "Mono", 0, 4))
            {
                bIsMono = true;
            }
            else
            {
                bIsMono = false;
            }

            bIsImplemented = m_objIGXDevice.GetRemoteFeatureControl().IsImplemented("PixelColorFilter");

            // 若当前为非黑白且支持PixelColorFilter则为彩色
            if ((!bIsMono) && (bIsImplemented))
            {
                bIsColorFilter = true;
            }
            else
            {
                bIsColorFilter = false;
            }
        }
    }
}
