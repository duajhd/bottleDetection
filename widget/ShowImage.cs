using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bottleDetect.ConfigInfo;
using System.Windows.Forms;
using bottleDetection.algothrim;
using bottleDetection.Tool;
using bottleDetection.widget;
using GxIAPINET;
//现在就是要做到
namespace bottleDetection.widget
{
    //下一步要完成的就是随意调整矩形

    //下一步要实现拖动框

    //36个按钮、图像显示都需要动态添加
    //获取panel3的宽度
    public partial class ShowImage : Form
    {
        public camControl cameraInstance = null;
        public List<detectThread> threadList = new List<detectThread>();
        private readonly float initialWidth;
        private readonly float initialHeight;

        private locationBox LocationBox;

        private int i_originImageWidth;           //原始图像宽
        private int i_originImageHeight;          //原始图像高
        private int i_scaledImageWidth;           //缩放后的图像宽高
        private int i_scaledImageHeight;          //
        private double i_scaledX;                     //x轴缩放
        private double i_scaledY;                     //y轴缩放

        private bool isMouseBtnPressed = false;             //鼠标左键是否按下

        private Bitmap backGroundImage = null;      //背景图像
        private Bitmap image = null;                //缓冲画布
        //
        private Graphics GDI = null;                //绘图的画布

        private Graphics bitmaapGDI = null;         //缓冲画布

      
        private static bool finished = false;
        private static bool draging = false;
        private static int deltaX = 0;              //X方向偏移量
        private static int deltaY = 0;              //Y方向偏移量
        private static int deltaWidth = 0;
        private static int deltaHeight = 0;
        private static int i_status = 0;                //0:绘制1.拖动2.缩放3.X轴拉伸4.Y轴拉伸
        private static int actualX;
        private static int actualY;
        private static int actualWidth;
        private static int actualHeight;
        private MyPoint startDraging;
        private bool testing;

        private FullLight fullLightForm = null;
        private Wave waveForm = null;
        private StressForm stresForm = null;

        //接下来就可以开发定位了
        //确定定位开发的目标:1.实现三线定位2.实现模板定位3.也是要实现裁剪区域4.生成星号

        //必须仔细研究机器视觉算法与应用
        //paint事件在需要重绘时触发，那什么时候触发呢?重绘又是以什么基础作为重绘图形
        //paint 和Onpaint的区别是什么?
        //如何动态生成PictureBox?哪些是已知的，哪些是要计算的。已知的1.相机个数2.窗口的宽高3.每行和每列的数量4.每行最大数 需要计算1.每行的个数(依据预设，小于等于12除以2；大于等于12除以三(每行数量不得超过最大数))2.每列的个数()3.

        //那么计算过程是怎样的?
        public ShowImage()
        {
            InitializeComponent();
            initialWidth = Width;
            initialHeight = Height;
            setTag(panel4);
            setTag(panel5);
            LocationBox = new locationBox();
            LocationBox.rect.iLeftPointCol = 0;
            LocationBox.rect.iLeftPointRow = 0;
            LocationBox.rect.Height = 0;
            LocationBox.rect.Width = 0;
            testing = false;
            
            LocationBox.lines = new List<MyLine>(); //分配空间
            fullLightForm = new FullLight();
            waveForm = new Wave();
            stresForm = new StressForm();
        }
        ~ShowImage()
        {
            if(cameraInstance != null)
            {
                cameraInstance.closeAllCamera();
            }
            
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void setTag(Control control)
        {
            foreach (Control item in control.Controls)
            {
                item.Tag = item.Width + ";" + item.Height + ";" + item.Left + ";" + item.Top + ";" + item.Font.Size;
                if (item.Controls.Count > 0) setTag(item);
            }
        }

        private void setControls(float scaleX, float scaleY, Control control)
        {
            foreach (Control item in control.Controls)
            {
                if (item.Tag != null)
                {
                    var myTag = item.Tag.ToString().Split(';');

                    item.Width = Convert.ToInt32(Convert.ToSingle(myTag[0]) * scaleX);
                    item.Height = Convert.ToInt32(Convert.ToSingle(myTag[1]) * scaleY);
                    item.Left = Convert.ToInt32(Convert.ToSingle(myTag[2]) * scaleX);
                    item.Top = Convert.ToInt32(Convert.ToSingle(myTag[3]) * scaleY);

                    //   var currentSize = Convert.ToSingle(myTag[4])* scaleY;
                    //   if (currentSize > 0) item.Font = new Font(item.Font.Name,currentSize,item.Font.Style,item.Font.Unit);
                    //实验只改变位置，不改变大小成功

                    if (item.Controls.Count > 0) setControls(scaleX, scaleY, item);
                }
            }
        }

        private void ReWinformLayout()
        {
            var scaleX = Width / initialWidth;
            var scaleY = Height / initialHeight;
            
            setControls(scaleX, scaleY, panel5);
            setControls(scaleX, scaleY, panel4);
        }

        private void ShowImage_Resize(object sender, EventArgs e)
        {
            //ReWinformLayout();
            //不是一定要用控件缩放尺寸
        }

      
       
        private void DrawRect(Graphics GDI, locationBox LocationBoxLocationBox)
        {
            GDI.DrawRectangle(new Pen(Color.Blue, 2), LocationBox.rect.iLeftPointCol, LocationBox.rect.iLeftPointRow, LocationBox.rect.Width, LocationBox.rect.Height);

            if (LocationBoxLocationBox.lines.Count>0)
            {
                foreach (MyLine line in LocationBox.lines)
                {
                    GDI.DrawLine(new Pen(Color.Blue, 2), line.startPoint.Col, line.startPoint.Row, line.endPoint.Col, line.endPoint.Row);
                }
            }
           
        }

        private void getALLPictureBox(Object parentWindow, List<PictureBox> pictureList)
        {
            Control widgrt = parentWindow as Control;

            if (widgrt == null) return;                    //递归出口，没有子控件返回

            foreach (Control ctrl in widgrt.Controls)
            {

                if (ctrl is PictureBox)
                {
                    //相关操作代码
                    pictureList.Add(ctrl as PictureBox);


                }
                else
                {
                    getALLPictureBox(ctrl, pictureList);
                }

            }
        }
       

        private void numericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDownX.Value + LocationBox.rect.Width > originImageContainer.Width)
            {
                numericUpDownX.Value = LocationBox.rect.iLeftPointCol + LocationBox.rect.Width;//保证数值越界时，控件数值恒定
                return;
            }
            LocationBox.rect.iLeftPointCol = (int)numericUpDownX.Value;
            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
            DrawRect(bitmaapGDI, LocationBox);
            GDI.DrawImage(image, new PointF(0, 0));
        }

       

        private void numericUpDownW_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownW.Value + LocationBox.rect.iLeftPointCol> originImageContainer.Width)
            {
                numericUpDownW.Value = LocationBox.rect.Width;
                return;
            }
            LocationBox.rect.Width = (int)numericUpDownW.Value;
            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
            DrawRect(bitmaapGDI, LocationBox);
            GDI.DrawImage(image, new PointF(0, 0));
        }

        private void numericUpDownH_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownH.Value + LocationBox.rect.iLeftPointRow > originImageContainer.Height)
            {
                numericUpDownH.Value = LocationBox.rect.Height;
                return;
            }
            LocationBox.rect.Height = (int)numericUpDownH.Value;
            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
            DrawRect(bitmaapGDI, LocationBox);
            GDI.DrawImage(image, new PointF(0, 0));
        }

       

        private void ShowImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cameraInstance == null)
            {
                cameraInstance = new camControl();
            }

        }




        //可以从这个函数开始获得相机数，然后再动态计算
        private void openCamera_Click(object sender, EventArgs e)
        {
            if(cameraInstance == null)
            {
                cameraInstance = new camControl();
            }

            MessageBox.Show("相机总数" + cameraInstance.l_camList.Count.ToString());
            //获取PictureBox列表
            List<PictureBox> pictureList = new List<PictureBox>();
            List<PictureBox> wavepictureList = new List<PictureBox>();

            bool openStatus = cameraInstance.openAllCamera();
            //1.打开相机2.初始化参数3.绑定pictureBox4.绑定回调
            if (openStatus == true)
            {
                MessageBox.Show("打开成功");

                cameraInstance.openAllCamera();
                cameraInstance.initCamParam();
                //打开相机后，建立窗口组件

                //显示上有逻辑问题

                setCameraParam();               //设置相机参数

                loadFrame(fullLightForm);
                fullLightForm.set(18);

                loadFrame(waveForm);
                waveForm.set(18);

                loadFrame(stresForm);
                stresForm.set(6);

                int j = 0;                                                        //应力相机循环变量
                try {
                //    if (pictureList.Count != wavepictureList.Count) throw ("");//抛出异常
                   // int pictureNum = pictureList.Count;
                    for (int i = 0; i < cameraInstance.l_camList.Count; i++)         //设置相机显示widget,用于显示图像
                    {
                        //遍历所有相机，应力相机往后排且应跳过
                        if (cameraInstance.l_camList[i].b_isStress)
                        {
                           // continue;//跳过应力相机
                            cameraInstance.l_camList[i].GCStree = stresForm.pictureList[j].CreateGraphics();
                            if (cameraInstance.l_camList[i].i_imageWidth != 0 && cameraInstance.l_camList[i].i_imageHeight != 0)
                            {
                                //为相机指定图像显示框的宽高
                                cameraInstance.l_camList[i].pictureBoxWidth = stresForm.pictureList[i].Width;
                                cameraInstance.l_camList[i].pictureBoxHeight = stresForm.pictureList[i].Height;
                                //这里不用显示窗口来计算缩放系数其实是合理的，因为图像显示窗口计算系数后足以将瓶子裁剪出来。pictureBox只是将瓶子显示出来

                                cameraInstance.l_camList[i].i_scaledX = (double)originImageContainer.Width / (double)cameraInstance.l_camList[i].i_imageWidth;
                                cameraInstance.l_camList[i].i_scaledY = (double)originImageContainer.Height / (double)cameraInstance.l_camList[i].i_imageHeight;
                                cameraInstance.l_camList[i].setShowWidget();//创建双缓冲绘图区



                            }
                            j +=1;
                        }
                        else
                        {
                            cameraInstance.l_camList[i].m_objGC = fullLightForm.pictureList[i].CreateGraphics();            //正常图像GDI
                            cameraInstance.l_camList[i].GCWave = waveForm.pictureList[i].CreateGraphics();                  //应力图像GDI
                            if (cameraInstance.l_camList[i].i_imageWidth!=0&& cameraInstance.l_camList[i].i_imageHeight != 0)
                            {
                                //为相机指定图像显示框的宽高
                                cameraInstance.l_camList[i].pictureBoxWidth = fullLightForm.pictureList[i].Width;
                                cameraInstance.l_camList[i].pictureBoxHeight = fullLightForm.pictureList[i].Height;
                                //这里不用显示窗口来计算缩放系数其实是合理的，因为图像显示窗口计算系数后足以将瓶子裁剪出来。pictureBox只是将瓶子显示出来

                                cameraInstance.l_camList[i].i_scaledX = (double)originImageContainer.Width / (double)cameraInstance.l_camList[i].i_imageWidth;
                                cameraInstance.l_camList[i].i_scaledY = (double)originImageContainer.Height / (double)cameraInstance.l_camList[i].i_imageHeight;
                                cameraInstance.l_camList[i].setShowWidget();//创建双缓冲绘图区



                            }
              
                        }
                        //还是要将相机与pictureBox对应起来

                    }



                    cameraInstance.bindCaptrueCallbak();//绑定回调
                    testing = true;         //因为打开相机后默认执行startGrab，因此测试状态为true
                    startTestBtn.Text = "停止测试";
                    for (int i = 0; i < cameraInstance.l_camList.Count; i++)
                    {
                        detectThread detectObject = new detectThread(cameraInstance.l_camList[i]);

                        threadList.Add(detectObject);
                        detectObject.startThread();
                    }
                }
                catch
                {

                }
                //做一个IP与picturebox映射的map
               
            }
            else
            {
                MessageBox.Show("打开失败");
                return;
            }

        }

        private void originImageContainer_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseBtnPressed = true;
            if (image == null)//初始化缓冲区，初始化背景图片
            {
                //缓冲区大小必须和目标绘图区大小完全相等
                image = new Bitmap(originImageContainer.Width, originImageContainer.Height);
                backGroundImage = new Bitmap("image number1_1593.bmp");
                i_originImageHeight = backGroundImage.Height;
                i_originImageWidth = backGroundImage.Width;
                i_scaledX = (double)originImageContainer.Width / (double)i_originImageWidth;
                i_scaledY = (double)originImageContainer.Height / (double)i_originImageHeight;
                bitmaapGDI = Graphics.FromImage(image);
                //目标是任何一张图像都能
                GDI = originImageContainer.CreateGraphics();
            }
            //瞬间改变大小可能是触发重绘
            if (finished)
            {
                double distance;
                distance = Math.Sqrt(Math.Pow(e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width), 2.0) + Math.Pow(e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow), 2.0));
                if (e.X > LocationBox.rect.iLeftPointCol && e.Y > LocationBox.rect.iLeftPointRow && e.X < (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width - 30) && e.Y < (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow - 30))
                {
                    draging = true;             //绘制完成状态，draging为true
                    startDraging = new MyPoint();
                    startDraging.Col = e.X;
                    startDraging.Row = e.Y;
                    i_status = (int)e_controlBit.draging;
                    return;
                }
                if (distance <= 6.0)//把判定scaling放在前面，防止误判
                {
                    i_status = (int)e_controlBit.scaling;
                    startDraging = new MyPoint();
                    startDraging.Col = e.X;
                    startDraging.Row = e.Y;
                    return;
                }
                if (e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width) <= 5 && e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width) >= 0)
                {
                    i_status = (int)e_controlBit.strecthX;
                    startDraging = new MyPoint();
                    startDraging.Col = e.X;
                    startDraging.Row = e.Y;

                    return;
                }
                if (e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow) <= 5 && e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow) >= 0)
                {
                    i_status = (int)e_controlBit.strecthY;
                    startDraging = new MyPoint();
                    startDraging.Col = e.X;
                    startDraging.Row = e.Y;
                    return;
                }


            }





            else
            {

                //未完成绘制，表示开始绘制

                i_status = (int)e_controlBit.drawing;
                LocationBox.rect.iLeftPointCol = e.X;
                LocationBox.rect.iLeftPointRow = e.Y;

                GDI.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
            }

        }

       

        private void originImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseBtnPressed)
            {
                if (e.Button == MouseButtons.Left)
                {





                    //一开始的设置是，如果不判定为任何一个，默认就是绘制
                    //弹起后finished应该是被设置为真，但是由于未匹配到switch任何一个，导致再按下移动时还是执行绘制。解决方法就是判定要正确，还有就是判定为一类后立即返回
                    switch (i_status)
                    {
                        case (int)e_controlBit.drawing:
                            //大概率是判别错误
                            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
                            try
                            {
                                if (e.X - LocationBox.rect.iLeftPointCol > 0 && e.Y - LocationBox.rect.iLeftPointRow > 0)
                                {
                                    //那个框还是出现了，不应该画框
                                    LocationBox.rect.Width = e.X - LocationBox.rect.iLeftPointCol;
                                    LocationBox.rect.Height = e.Y - LocationBox.rect.iLeftPointRow;
                                    numericUpDownX.Value = LocationBox.rect.iLeftPointCol;
                                    numericUpDownY.Value = LocationBox.rect.iLeftPointRow;
                                    numericUpDownW.Value = LocationBox.rect.Width;
                                    //这里经常出错是因为点下了键后向左上移动导致出现负值，以后解决
                                    numericUpDownH.Value = LocationBox.rect.Height;
                                    actualX = (int)((double)numericUpDownX.Value / i_scaledX);
                                    actualY = (int)((double)numericUpDownY.Value / i_scaledY);
                                    actualWidth = (int)((double)numericUpDownW.Value / i_scaledX);
                                    actualHeight = (int)((double)numericUpDownH.Value / i_scaledY);
                                    DrawRect(bitmaapGDI, LocationBox);
                                    GDI.DrawImage(image, new PointF(0, 0));
                                }
                            }
                            catch
                            {
                                if (cameraInstance != null)
                                {
                                    cameraInstance.closeAllCamera();
                                }
                            }



                            break;
                        case (int)e_controlBit.draging:

                            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
                            deltaX = e.X - startDraging.Col;
                            deltaY = e.Y - startDraging.Row;
                            //LocationBox.rect.iLeftPointCol = LocationBox.rect.iLeftPointCol + (e.X - LocationBox.rect.iLeftPointCol);
                            // LocationBox.rect.iLeftPointRow = LocationBox.rect.iLeftPointRow + (e.Y - LocationBox.rect.iLeftPointRow);
                            //不应该以起点作为相减
                            LocationBox.rect.iLeftPointCol = (LocationBox.rect.iLeftPointCol + deltaX) <= 0 ? 0 : LocationBox.rect.iLeftPointCol + deltaX;

                            LocationBox.rect.iLeftPointRow = (LocationBox.rect.iLeftPointRow + deltaY) <= 0 ? 0 : LocationBox.rect.iLeftPointRow + deltaY;
                            //  if (LocationBox.rect.iLeftPointCol<=0 || LocationBox.rect.iLeftPointRow<=0 || LocationBox.rect.iLeftPointRow + LocationBox.rect.Height >= (pictureBox19.Height) || LocationBox.rect.iLeftPointCol + LocationBox.rect.Width >= (pictureBox19.Width))
                            if (LocationBox.rect.iLeftPointRow + LocationBox.rect.Height >= (originImageContainer.Height)) //如果末尾坐标值达到极限
                            {
                                LocationBox.rect.iLeftPointRow = originImageContainer.Height - LocationBox.rect.Height;
                            }


                            if (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width >= (originImageContainer.Width))
                            {
                                LocationBox.rect.iLeftPointCol = originImageContainer.Width - LocationBox.rect.Width;
                            }
                            try
                            {
                                numericUpDownX.Value = LocationBox.rect.iLeftPointCol;
                                numericUpDownY.Value = LocationBox.rect.iLeftPointRow;
                                actualX = (int)((double)numericUpDownX.Value / i_scaledX);
                                actualY = (int)((double)numericUpDownY.Value / i_scaledY);
                                actualWidth = (int)((double)numericUpDownW.Value / i_scaledX);
                                actualHeight = (int)((double)numericUpDownH.Value / i_scaledY);
                                DrawRect(bitmaapGDI, LocationBox);
                                GDI.DrawImage(image, new PointF(0, 0));
                                startDraging.Col = e.X;
                                startDraging.Row = e.Y;
                            }
                            catch
                            {
                                if (cameraInstance != null)
                                {
                                    cameraInstance.closeAllCamera();
                                }
                            }

                            break;
                        case (int)e_controlBit.scaling: //完全成功
                            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
                            deltaHeight = e.Y - startDraging.Row;
                            deltaWidth = e.X - startDraging.Col;
                            LocationBox.rect.Height = LocationBox.rect.Height + deltaHeight;
                            if (LocationBox.rect.Height + deltaHeight >= originImageContainer.Height - LocationBox.rect.iLeftPointRow)//如果高度拉伸到极限值
                            {
                                LocationBox.rect.Height = originImageContainer.Height - LocationBox.rect.iLeftPointRow;
                            }
                            LocationBox.rect.Width = LocationBox.rect.Width + deltaWidth;
                            if (LocationBox.rect.Width + deltaWidth >= originImageContainer.Width - LocationBox.rect.iLeftPointCol)   //如果宽度拉伸达到极限值
                            {
                                LocationBox.rect.Width = originImageContainer.Width - LocationBox.rect.iLeftPointCol;
                            }

                            try
                            {
                                numericUpDownW.Value = LocationBox.rect.Width;
                                numericUpDownH.Value = LocationBox.rect.Height;
                                actualX = (int)((double)numericUpDownX.Value / i_scaledX);
                                actualY = (int)((double)numericUpDownY.Value / i_scaledY);
                                actualWidth = (int)((double)numericUpDownW.Value / i_scaledX);
                                actualHeight = (int)((double)numericUpDownH.Value / i_scaledY);
                                DrawRect(bitmaapGDI, LocationBox);
                                GDI.DrawImage(image, new PointF(0, 0));
                                startDraging.Col = e.X;
                                startDraging.Row = e.Y;
                            }
                            catch
                            {
                                if (cameraInstance != null)
                                {
                                    cameraInstance.closeAllCamera();
                                }
                            }


                            break;
                        case (int)e_controlBit.strecthX:
                            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
                            deltaWidth = e.X - startDraging.Col;
                            LocationBox.rect.Width = LocationBox.rect.Width + deltaWidth;
                            if (LocationBox.rect.Width + deltaWidth >= originImageContainer.Width - LocationBox.rect.iLeftPointCol)
                            {
                                LocationBox.rect.Width = originImageContainer.Width - LocationBox.rect.iLeftPointCol;
                            }
                            try
                            {
                                numericUpDownW.Value = LocationBox.rect.Width;
                                numericUpDownH.Value = LocationBox.rect.Height;
                                actualX = (int)((double)numericUpDownX.Value / i_scaledX);
                                actualY = (int)((double)numericUpDownY.Value / i_scaledY);
                                actualWidth = (int)((double)numericUpDownW.Value / i_scaledX);
                                actualHeight = (int)((double)numericUpDownH.Value / i_scaledY);
                                DrawRect(bitmaapGDI, LocationBox);
                                GDI.DrawImage(image, new PointF(0, 0));
                                startDraging.Col = e.X;
                                startDraging.Row = e.Y;
                            }
                            catch
                            {
                                if (cameraInstance != null)
                                {
                                    cameraInstance.closeAllCamera();
                                }
                            }

                            break;
                        case (int)e_controlBit.strecthY:
                            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
                            deltaHeight = e.Y - startDraging.Row;
                            LocationBox.rect.Height = LocationBox.rect.Height + deltaHeight;
                            if (LocationBox.rect.Height + deltaHeight >= originImageContainer.Height - LocationBox.rect.iLeftPointRow)
                            {
                                LocationBox.rect.Height = originImageContainer.Height - LocationBox.rect.iLeftPointRow;
                            }
                            try
                            {
                                numericUpDownW.Value = LocationBox.rect.Width;
                                numericUpDownH.Value = LocationBox.rect.Height;
                                actualX = (int)((double)numericUpDownX.Value / i_scaledX);
                                actualY = (int)((double)numericUpDownY.Value / i_scaledY);
                                actualWidth = (int)((double)numericUpDownW.Value / i_scaledX);
                                actualHeight = (int)((double)numericUpDownH.Value / i_scaledY);
                                DrawRect(bitmaapGDI, LocationBox);
                                GDI.DrawImage(image, new PointF(0, 0));
                                startDraging.Col = e.X;
                                startDraging.Row = e.Y;
                            }
                            catch
                            {
                                if (cameraInstance != null)
                                {
                                    cameraInstance.closeAllCamera();
                                }
                            }

                            break;


                    }
                }
            }
            else
            {
                if (finished)                   //如果是完成状态的移动
                {
                    double distance;
                    distance = Math.Sqrt(Math.Pow(e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width), 2.0) + Math.Pow(e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow), 2.0));
                    if (e.X > LocationBox.rect.iLeftPointCol && e.Y > LocationBox.rect.iLeftPointRow && e.X < (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width - 30) && e.Y < (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow - 30))
                    {

                        this.Cursor = Cursors.Hand;
                        return;
                    }
                    if (distance <= 6.0)
                    {
                        this.Cursor = Cursors.SizeNWSE;
                        return;
                    }
                    if (e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width) <= 5 && e.X - (LocationBox.rect.iLeftPointCol + LocationBox.rect.Width) >= 0)
                    {
                        this.Cursor = Cursors.SizeWE;

                        return;
                    }
                    if (e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow) <= 5 && e.Y - (LocationBox.rect.Height + LocationBox.rect.iLeftPointRow) >= 0)
                    {
                        this.Cursor = Cursors.SizeNS;
                        return;
                    }


                }
            }

        }

        private void originImageContainer_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseBtnPressed = false;
            if (Math.Abs(e.X - LocationBox.rect.iLeftPointCol) < 10 || Math.Abs(e.Y - LocationBox.rect.iLeftPointRow) < 10)  //原地点击直接返回
            {
                return;
            }
            if (LocationBox.rect.Width > 0)//当确实产生了高度值才将完成状态finished置为true
            {
                finished = true; //完成状态置为true
            }

        }

        private void originImageContainer_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;              //移出originImageContainer后鼠标指针变成默认
        }

        private void numericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownY.Value + LocationBox.rect.Height > originImageContainer.Height)
            {
                numericUpDownY.Value = LocationBox.rect.iLeftPointRow + LocationBox.rect.Height;
                return;
            }
            LocationBox.rect.iLeftPointRow = (int)numericUpDownY.Value;
            bitmaapGDI.DrawImage(backGroundImage, new Rectangle(0, 0, originImageContainer.Width, originImageContainer.Height), new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height), GraphicsUnit.Pixel); //用这句代码实现了缩放
            DrawRect(bitmaapGDI, LocationBox);
            GDI.DrawImage(image, new PointF(0, 0));
        }

        private void startTestBtn_Click(object sender, EventArgs e)
        {
            //  MessageBox.Show(actualWidth.ToString());


            testing = !testing;     //测试状态取反
            try
            {
                if (testing == true)        //原来没有在测试，现在是要开启测试
                {

                    
                    if (cameraInstance != null)
                    {
                        foreach (realCameraInfo item in cameraInstance.l_camList)
                        {
                            item.o_objIGXStream.StartGrab();     //停止抓取但是不关闭相机，这样就可以重复测试采集
                        }
                    }
                    startTestBtn.Text = "停止测试";

                }
                //原来正在测试，现在是要停止测试
                else
                {
                    
                    if (cameraInstance != null)
                    {
                        foreach (realCameraInfo item in cameraInstance.l_camList)
                        {
                            item.o_objIGXStream.StopGrab();     //重新开始采集
                        }
                    }
                    startTestBtn.Text = "开始测试";
                }
            }
            catch
            {
                cameraInstance.closeAllCamera();            //出现异常关闭所有相机
            }
        }

        private void ShowImage_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cameraInstance != null)
            {
                cameraInstance.closeAllCamera();
            }
        }

        private void Button39_Click(object sender, EventArgs e)
        {
            if (cameraInstance != null)
            {
                cameraInstance.closeAllCamera();
            }
        }

        private void normalBtn_Click(object sender, EventArgs e)        //显示全亮
        {
            if (cameraInstance.l_camList.Count > 0)
            {
                foreach (realCameraInfo item in cameraInstance.l_camList)
                {
                    item.FullLight = true;
                }
            }
            stresForm.Visible = false;
            waveForm.Visible = false;
            fullLightForm.Visible = true;
           

            //隐藏条纹窗口


        }

        private void waveBtn_Click(object sender, EventArgs e)          //显示条纹
        {
            if (cameraInstance.l_camList.Count > 0)
            {
                foreach (realCameraInfo item in cameraInstance.l_camList)
                {
                    item.FullLight = false;
                }
            }
            stresForm.Visible = false;
            fullLightForm.Visible = false;
            waveForm.Visible = true;
            //隐藏全亮窗口
        }


        private void setCameraParam(string ConfigPath = "./Config/Config.ini")
        {
            Setting parseSet = new Setting(ConfigPath);
            int DeviceNum = 0;
            int DeviceRealLine = 0;
            int DeviceID = 0;
            string DeviceName = "";
            string DeviceMark = "";
            string DeviceInitFile = "";
            int DeviceStation = 1;
            int DeviceFristTrigger = 1;
            int DeviceSecondTrigger = 2;
            parseSet.beginGroup("GarbCardParameter");
            parseSet.value("DeviceNum", 12,out DeviceNum);  //  读相机数
            //+""+
            //+ (i + 1).ToString() +

            for (int i = 0;i< DeviceNum; i++)
            {
                parseSet.value("Device" + (i + 1).ToString() + "RealLine",0,out DeviceRealLine);
                parseSet.value("Device"+(i+1).ToString()+"ID",0,out DeviceID);
                parseSet.value("Device" + (i + 1).ToString() + "Name","",out DeviceName);
                parseSet.value("Device" + (i + 1).ToString() + "Mark","",out DeviceMark);
                parseSet.value("Device" + (i + 1).ToString() + "InitFile","",out DeviceInitFile);
                parseSet.value("Device" + (i + 1).ToString() + "Station",1,out DeviceStation);
                parseSet.value("Device" + (i + 1).ToString() + "FristTrigger",1,out DeviceFristTrigger);
                parseSet.value("Device" + (i + 1).ToString() + "SecondTrigger", 1, out DeviceSecondTrigger);

                foreach (realCameraInfo item in cameraInstance.l_camList)
                {
                    if(item.strMAC == DeviceMark)
                    {
                        item.CameraID = DeviceID;           //指定相机号
                    }
                }


            }






            parseSet.endGroup();

        }
        private void loadFrame(Object form)
        {
            //if (this.BodyContainer.Controls.Count > 0)
            //{
            //    this.BodyContainer.Controls.RemoveAt(0);//只显示一个应该是加载时把第一个移出了

            //}
            Form widget = form as Form;

            widget.TopLevel = false;
            widget.Dock = DockStyle.Fill;

            this.BodyContainer.Controls.Add(widget);
            this.BodyContainer.Tag = widget;
            widget.Show();

        }

        private void setExposureTime_Click(object sender, EventArgs e)                      //设置曝光时间
        {
            double ExposureTime = (double)numericUpDownForTime.Value;
            int camID = (int)numericUpDownForcamID.Value;
            Boolean bIsImplemented;

            double dMIn;
            double dMax;
            foreach (realCameraInfo item in cameraInstance.l_camList)
            {
                if (item.CameraID == camID)
                {
                    bIsImplemented = item.o_objIGXFeatureControl.IsImplemented("ExposureTime");//获得曝光时间属性读写限制
                    if (bIsImplemented)                                                         //如果属性可以被读写
                    {
                        IFloatFeature objFloat = item.o_objIGXFeatureControl.GetFloatFeature("ExposureTime");
                        dMIn = objFloat.GetMin();
                        dMax = objFloat.GetMax();
                        if(ExposureTime>= dMIn&& ExposureTime<= dMax)                           //大于最小，小于最大那么可以设置值
                        {
                            objFloat.SetValue(ExposureTime);
                        }
                    }
                }
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {

            fullLightForm.Visible = false;
            waveForm.Visible = false;
            stresForm.Visible = true;
        }
    }
}
