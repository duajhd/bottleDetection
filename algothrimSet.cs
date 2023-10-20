using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;
using bottleDetect.ConfigInfo;
using static HalconDotNet.HOperatorSet;
using HalconDotNet;
using System.IO;
using System.Drawing.Imaging;
//下一步要做1.内存管理2.基于AI的瓶颈、瓶身检测3.
//目前已经基本解决算法问题，打通图像采集到处理的整体流程。下一个问题就是解决.NET内存缓慢增长的问题
//我认为这个问题
namespace bottleDetection
{
   
  
      
    //目标就是双缓冲绘图绘制矩形的升级版，可以绘制一个框加几条框内线
        public partial class algothrimSet : Form
        {
            private Rect detectRec;
            private List<MyLine> LineList = new List<MyLine>();
            private HObject regBottleNeck;


            private List<String> imageList = new List<String>();

            private String imagePath = "";
            private Bitmap backGroundImage = null;
            private Bitmap Test = null;
            private Bitmap image = null;

            private int i_originImageWidth;           //原始图像宽
            private int i_originImageHeight;          //原始图像高
            private int i_scaledImageWidth;           //缩放后的图像宽高
            private int i_scaledImageHeight;          //

            private int DrawingMode = 1;               //绘图模式，0是线模式 

            private bool isMouseBtnPressed;             //鼠标左键是否按下
                                                        //
            private Graphics GDI = null;                //绘图的画布

            private Graphics bitmaapGDI = null;

            private MyLine bottleBodyLine;                //瓶身线
            private MyLine bottleNeckLine;                //瓶颈
            private MyPoint startPoint;                   //起点
            private MyPoint endPoint;                      //结束点
            private static bool drawing = false;//设置一个启动标志

            private int  i_imageIndex = 0;
            private FileStream fs = null;                                   //写入文件流
            private byte[] logText = { };
            private String stfFileName = "";//写入数据
            private StreamWriter sw = null;
        public algothrimSet()
        {
            InitializeComponent();
            
            startPoint = new MyPoint();
            endPoint = new MyPoint();

        }



        //今天先实现在图片上绘制线，如果有时间，尽可能实现几个辅助函数
        private void openalgothrimaDia_Click(object sender, EventArgs e)
        {
            Form alset = new algothrimSetDialog();
            alset.Show();
            
        }

        private void FileDialog_Click(object sender, EventArgs e)
        {
            //1.打开文件夹2.获取文件夹内所有图片文件3.将图片文件列表第一张图片设置到picturebox
            
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show("文件夹路径不能为空", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    return;
                }
                DirectoryInfo folder = new DirectoryInfo(dialog.SelectedPath);
                imageList.Clear();
                foreach (FileInfo file in folder.GetFiles("*.bmp"))
                {
                    imageList.Add(file.FullName);
                }
                backGroundImage = new Bitmap(imageList[0]);//filepath是图片的路径
                Test = new Bitmap(imageList[0]);//filepath是图片的路径
                image = new Bitmap(backGroundImage.Width, backGroundImage.Height);
                bitmaapGDI = Graphics.FromImage(image);
                this.pictureBox1.Image = backGroundImage;
                //为什么一开始只能在一小块区域内绘图?因为获取GDI的函数 GDI = pictureBox1.CreateGraphics();在构造函数中，这是的pictureBox还是默认宽高，还没有根据放大后的窗口调整pictureBox充满剩余部分，因此获取的GDI绘图区只有一小块。解决方法就是当pictureBox调整为充满剩余空间后再获取GDI
                GDI = pictureBox1.CreateGraphics();
                //到这里图像显示成功
                //  textBox.Text = dialog.SelectedPath + "\\";
                //do something
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
           
            drawing = true;
            startPoint.Row = e.Y;
            startPoint.Col = e.X;
            GDI.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)         //双缓冲绘图只在这个函数中写代码
        {
           
            if(e.Button == MouseButtons.Left)
            {
                if (drawing)
                {
                    if(DrawingMode == 0)
                    {
                        endPoint.Row = startPoint.Row;
                        endPoint.Col = e.X;
                        //先把原图画在空图上；再把线画在空图上
                        //2023.7.31双缓冲绘图成功，绘图不再闪烁
                        //从双缓冲绘图可以获得经验，解决问题的方法要正确
                        bitmaapGDI.DrawImage(backGroundImage, new PointF(0, 0));
                        bitmaapGDI.DrawLine(Pens.Blue, startPoint.Col, startPoint.Row, endPoint.Col, endPoint.Row);
                        GDI.DrawImage(image, new PointF(0, 0));
                        // Clear();
                        // DrawLine(startPoint, endPoint);
                    }
                    else if(DrawingMode == 1) {

                        endPoint.Row = e.Y;
                        endPoint.Col = e.X;
                        //先把原图画在空图上；再把线画在空图上
                        //2023.7.31双缓冲绘图成功，绘图不再闪烁
                        //从双缓冲绘图可以获得经验，解决问题的方法要正确
                        bitmaapGDI.DrawImage(backGroundImage, new PointF(0, 0));
                        Rectangle srcRect = new Rectangle(startPoint.Col, startPoint.Row, endPoint.Col- startPoint.Col, endPoint.Row - startPoint.Row);
                        bitmaapGDI.DrawRectangle(Pens.Blue,srcRect);
                        GDI.DrawImage(image, new PointF(0, 0));

                    }

                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)//这里也执行双缓冲绘图，鼠标键弹起后记录末尾坐标并且再执行一次双缓冲绘图将图片和线绘制到缓冲区
        {
            drawing = false;

            if (DrawingMode == 0)
            {
                endPoint.Row = startPoint.Row;
                endPoint.Col = e.X;
                //先把原图画在空图上；再把线画在空图上
                //2023.7.31双缓冲绘图成功，绘图不再闪烁
                //从双缓冲绘图可以获得经验，解决问题的方法要正确
                bitmaapGDI.DrawImage(backGroundImage, new PointF(0, 0));
                bitmaapGDI.DrawLine(Pens.Blue, startPoint.Col, startPoint.Row, endPoint.Col, endPoint.Row);
                GDI.DrawImage(image, new PointF(0, 0));
                // Clear();
                // DrawLine(startPoint, endPoint);
            }
            else if (DrawingMode == 1)
            {

                endPoint.Row = e.Y;
                endPoint.Col = e.X;
                //先把原图画在空图上；再把线画在空图上
                //2023.7.31双缓冲绘图成功，绘图不再闪烁
                //从双缓冲绘图可以获得经验，解决问题的方法要正确
                bitmaapGDI.DrawImage(backGroundImage, new PointF(0, 0));
                Rectangle srcRect = new Rectangle(startPoint.Col, startPoint.Row, endPoint.Col - startPoint.Col, endPoint.Row - startPoint.Row);
                bitmaapGDI.DrawRectangle(Pens.Blue, srcRect);
                GDI.DrawImage(image, new PointF(0, 0));

            }
            MyLine line = new MyLine();
            line.startPoint = startPoint;
            line.endPoint = endPoint;
            //仍然是关于内存管理的，这里line是局部变量，但是添加到LineList后line这个变量会在pictureBox1_MouseUp执行结束后被释放掉吗
            LineList.Add(line);
        }
        //现在要解决的就是GDI绘图区域只有一块必须要覆盖整个屏幕
        private void  DrawLine(MyPoint startPoint, MyPoint endPoint)
        {
            GDI.DrawLine(new Pen(Color.Blue, 2), startPoint.Col,startPoint.Row, endPoint.Col,endPoint.Row);
        }
        //清屏。并且重新显示图片
        private void Clear()
        {
            GDI.Clear(Color.White);
            

        }
        //清除绘图
        private void button4_Click(object sender, EventArgs e)
        {
           
        }
        //前一张图片
        private void button1_Click(object sender, EventArgs e)
        {
            if (i_imageIndex <= 0)
            {
                return;
            }
            i_imageIndex--;
            backGroundImage.Dispose();
            backGroundImage = new Bitmap(imageList[i_imageIndex]);
            this.pictureBox1.Image = backGroundImage;
        }
        //后一张图片
        private void button2_Click(object sender, EventArgs e)
        {

            //总是分配内存不释放，会不会导致栈溢出
            //使用了Dispose()释放内存后，内存仍然在增长，这是为什么?
            //看来之前采集图像软件崩掉也是因为内存溢出
            i_imageIndex++;
            if(i_imageIndex == imageList.Count())
            {
                return;
            }
            backGroundImage.Dispose();
            backGroundImage = new Bitmap(imageList[i_imageIndex]);
            //这里应该是实现了get和set作为钩子函数
            this.pictureBox1.Image = backGroundImage;
           // DrawALLLine();
        }

        private void DrawALLLine()
        {
           

            foreach (MyLine line in LineList)
            {
                GDI.DrawLine(new Pen(Color.Blue, 2), line.startPoint.Col, line.startPoint.Row, line.endPoint.Col, line.endPoint.Row);
            }
        }
        private void DrawAll()
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

            MyLine  linseg = new MyLine();
            linseg.startPoint.Row = 760;
            linseg.startPoint.Col = 0;
            linseg.endPoint.Row = 760;
            linseg.endPoint.Col = backGroundImage.Width;
            int[] rowPT ;
            int[] colPT;
            findEdgePointDouble(backGroundImage, linseg, out rowPT, out colPT);
            
        }
        //如何允许在项目中使用不安全代码?
     

        //只适用8位位图
        unsafe private int findEdgePointSingle(Bitmap Image, MyLine LineSeg,//直接写成重载吧
    out int[] RowPt, out int[] ColPt, int nEdge = 120, int nDirect = (int)e_LineDirect.L2R, int nType = 0,
        bool bMean = false, bool bAllPoint = false)
        {
            //1.获取bitmap字节数组2.遍历线生成x,y坐标4.判断坐标值是否大于阈值，大于则认为是边缘点

            
            int width = Image.Width;
            int nLength = LineSeg.endPoint.Col - LineSeg.startPoint.Col;
            
            int height = Image.Height;
            int nGrayDiff1, nGrayDiff2, nGrayDiff3, nCount;
            int[] Row,Col;

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData srcBmData = Image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            System.IntPtr srcPtr = srcBmData.Scan0;
            
            int src_bytes = srcBmData.Stride * height;
            
            byte[] srcValues = new byte[src_bytes];
            byte[] GrayValue ;
           
            RowPt = new int[width];
            ColPt = new int[width];
            nCount = 0;
            //瓶检用分类似乎也行，先把要区域截出来，在利用分类模块
            //这里需要判断是否整除4
            if (srcBmData.Stride == width)
            {
                System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues, 0, src_bytes);
            }
            else
            {
                for(int i = 0;i< height; i++)
                {
                    //这里没问题，这是把原图数据移动到不足4整数倍的内存地址，new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride)表示符合stride的内存地址
                    //System.Runtime.InteropServices.Marshal.Copy((int)srcPtr, i * width, new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride), width);

                   // System.Runtime.InteropServices.Marshal.Copy(srcPtr, new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride), i * width, width);
                }
            }
            //瓶检用分类似乎也行，先把要区域截出来，在利用分类模块
            if (LineSeg.startPoint.Row == LineSeg.endPoint.Row)              //平行与X轴
            {
                Row = new int[width];
                Col = new int[width];
                nLength = width;
                GrayValue = new byte[width];
                for (int i = 0; i < width; i++)
                {
                    Row[i] = LineSeg.startPoint.Row;
                    Col[i] = LineSeg.startPoint.Col + i;
                    GrayValue[i] = srcValues[Row[i]*width + Col[i]];       //计算点的
                }
                if(nType == 0)
                {
                    for (int i = 0;i<nLength - 5; i++)
                    {
                        Console.WriteLine(GrayValue[i]);
                        nGrayDiff1 = GrayValue[i] - GrayValue[i + 3];
                        nGrayDiff2 = GrayValue[i + 1] - GrayValue[i + 4];
                        nGrayDiff3 = GrayValue[i + 2] - GrayValue[i + 5];
                        if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)         //找到边缘点
                        {
                            RowPt[nCount] = Row[i];
                            ColPt[nCount] = Col[i];
                            ++nCount;
                            if (!bAllPoint)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else if (LineSeg.startPoint.Col == LineSeg.endPoint.Col)
            {
                Row = new int[height];
                Col = new int[height];
                nLength = height;
                GrayValue = new byte[height];
                for (int i = 0; i < height; i++)
                {
                    Row[i] = LineSeg.startPoint.Row + i;                
                    Col[i] = LineSeg.startPoint.Col;
                    GrayValue[i] = srcValues[Row[i] * width + Col[i]];        //计算点的


                }
                if (nType == 0)
                {
                    for (int i = 0; i < nLength - 5; ++i)
                    {
                        Console.WriteLine(GrayValue[i]);
                        nGrayDiff1 = GrayValue[i] - GrayValue[i + 3];
                        nGrayDiff2 = GrayValue[i + 1] - GrayValue[i + 4];
                        nGrayDiff3 = GrayValue[i + 2] - GrayValue[i + 5];
                        if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)         //找到边缘点
                        {
                            RowPt[nCount] = Row[i];
                            ColPt[nCount] = Col[i];
                            ++nCount;
                            if (!bAllPoint)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            //验证维生素b6会导致饥饿
          

         
           
            return nCount;
        }
        //这个代码还要仔细检查，2023.8.9,但是大体上是完成了定位
        //nEdge这个值一定要设置正确
        unsafe private int findEdgePointDouble(Bitmap Image, MyLine LineSeg,//直接写成重载吧
    out int[] RowPt, out int[] ColPt, int nEdge = 50, int nDirect = (int)e_LineDirect.L2R, int nType = 0,
        bool bMean = false)
        {


            int width = Image.Width;
            int nLength = LineSeg.endPoint.Col - LineSeg.startPoint.Col;

            int height = Image.Height;
            int nGrayDiff1, nGrayDiff2, nGrayDiff3, nCount;
            int[] Row, Col;

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData srcBmData = Image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            System.IntPtr srcPtr = srcBmData.Scan0;

            int src_bytes = srcBmData.Stride * height;

            byte[] srcValues = new byte[src_bytes];
            byte[] GrayValue;

            RowPt = new int[width];
            ColPt = new int[width];
            nCount = 0;
            int index = 0;
            //瓶检用分类似乎也行，先把要区域截出来，在利用分类模块
            //这里需要判断是否整除4
            if (srcBmData.Stride == width)
            {
                System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues, 0, src_bytes);
            }
            else
            {
                for (int i = 0; i < height; i++)
                {
                    //这里没问题，这是把原图数据移动到不足4整数倍的内存地址，new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride)表示符合stride的内存地址
                    //System.Runtime.InteropServices.Marshal.Copy((int)srcPtr, i * width, new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride), width);

                    // System.Runtime.InteropServices.Marshal.Copy(srcPtr, new IntPtr(srcPtr.ToInt64() + i * srcBmData.Stride), i * width, width);
                }
            }
            //瓶检用分类似乎也行，先把要区域截出来，在利用分类模块
            if (LineSeg.startPoint.Row == LineSeg.endPoint.Row)              //平行与X轴
            {
                Row = new int[width];
                Col = new int[width];
                nLength = width;
                GrayValue = new byte[width];
                for (int i = 0; i < width; i++)
                {
                    Row[i] = LineSeg.startPoint.Row;
                    Col[i] = LineSeg.startPoint.Col + i;
                    GrayValue[i] = srcValues[Row[i] * width + Col[i]];       //计算点的
                }
                if (nType == 0)
                {
                    for (int i = 0; i < nLength - 5; i++)
                    {
                        index = i;
                      
                        nGrayDiff1 = GrayValue[i] - GrayValue[i + 3];
                        nGrayDiff2 = GrayValue[i + 1] - GrayValue[i + 4];
                        nGrayDiff3 = GrayValue[i + 2] - GrayValue[i + 5];
                        if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)         //找到边缘点
                        {
                            RowPt[nCount] = Row[i];
                            ColPt[nCount] = Col[i];
                            ++nCount;
                            break;
                        }
                    }

                    if (nCount > 0)
                    {
                        for (int j = nLength - 1; j > index + 8; --j)//大于i+8是防止找到前面找到的点
                        {
                            Console.WriteLine(GrayValue[j]);
                            nGrayDiff1 = GrayValue[j] - GrayValue[j - 3];
                            nGrayDiff2 = GrayValue[j - 1] - GrayValue[j - 4];
                            nGrayDiff3 = GrayValue[j - 2] - GrayValue[j - 5];
                            if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)
                            {
                                RowPt[nCount] = LineSeg.startPoint.Row;
                                ColPt[nCount] = j;

                                ++nCount;
                                break;
                            }
                        }
                    }
                }
            }
            else if (LineSeg.startPoint.Col == LineSeg.endPoint.Col)
            {
                Row = new int[height];
                Col = new int[height];
                nLength = height;
                GrayValue = new byte[height];
                for (int i = 0; i < height; i++)
                {
                    Row[i] = LineSeg.startPoint.Row + i;
                    Col[i] = LineSeg.startPoint.Col;
                    GrayValue[i] = srcValues[Row[i] * width + Col[i]];        //计算点的


                }
                if (nType == 0)
                {
                    for (int i = 0; i < nLength - 5; ++i)
                    {
                        index = i;
                       
                        nGrayDiff1 = GrayValue[i] - GrayValue[i + 3];
                        nGrayDiff2 = GrayValue[i + 1] - GrayValue[i + 4];
                        nGrayDiff3 = GrayValue[i + 2] - GrayValue[i + 5];
                        if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)         //找到边缘点
                        {
                            RowPt[nCount] = Row[i];
                            ColPt[nCount] = Col[i];
                            ++nCount;
                            break;
                        }
                    }

                    if (nCount > 0)
                    {
                        for (int j = nLength - 1; j > index + 8; --j)//大于i+8是防止找到前面找到的点
                        {
                            Console.WriteLine(GrayValue[j]);
                            nGrayDiff1 = GrayValue[j] - GrayValue[j - 3];
                            nGrayDiff2 = GrayValue[j - 1] - GrayValue[j - 4];
                            nGrayDiff3 = GrayValue[j - 2] - GrayValue[j - 5];
                            if (nGrayDiff1 > nEdge && nGrayDiff2 > nEdge && nGrayDiff3 > nEdge)
                            {
                                RowPt[nCount] = LineSeg.startPoint.Row;
                                ColPt[nCount] = j;

                                ++nCount;
                                break;
                            }
                        }
                    }
                }
            }
            //验证维生素b6会导致饥饿




            return nCount;
        }
       

    }
}
