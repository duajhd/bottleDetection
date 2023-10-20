using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;
using HalconDotNet;
//在这里开发类似大恒的算法测试界面可实现功能1.图像显示2.图像绘制矩形、实线3.矩形、实线的可拖动4.瓶身定位5.dyn_threshold算法检测5.自适应图片大小
//当前的任务是把大恒的客户端仿出来
//自定向下的分析，一个传统瓶检算法需要什么
//需要检的项目1.黑点2.裂纹3.气泡4.结石5.歪脖6.瓶全高7.瓶全宽8.
//接下来要仔细研究缺陷图片
//cam0下的缺陷可以做全局上下文异常检测
namespace bottleDetection.widget
{

    public struct Rect { //定义检测矩形
        public int iLeftPointRow;
        public int iLeftPointCol;
        public int Width;
        public int Height;

    }
    public struct Point
    {
        int Row;
        int Col;

    }
    public struct Line
    {
        Point startPoint;
        Point endPoint;
    }

    public class MyPictureBox:PictureBox
    {
        private Rect detectRec;
        private List<Line> LineList = new List<Line>();
        private HObject regBottleNeck;




        private String imagePath  = "";

        private int i_originImageWidth;           //原始图像宽
        private int i_originImageHeight;          //原始图像高
        private int i_scaledImageWidth;           //缩放后的图像宽高
        private int i_scaledImageHeight;          //

        private bool isMouseBtnPressed;             //鼠标左键是否按下
                                                    //
        private Graphics GDI = null;                //绘图的画布

        private Line bottleBodyLine;                //瓶身线
        private Line bottleNeckLine;                //瓶颈

        public MyPictureBox()
        {
            TabIndexChanged += kr;
            MouseMove += kr;
            //MouseMove事件原来是定义在Controls中
            //下一步还是要仔细研究基于halcon的传统瓶检算法

            GDI = this.CreateGraphics();            //创建画布
            
        }
        //继承自系统控件的自定义组件，也可以绑定事件。那么怎么绑定时间呢，答案是委托和事件。具体使用方法是1先定义一个委托，这个委托有参数类型和返回值类型2.再定义一个事件，该事件的委托约束为第一步定义的委托3.定义一个类内成员函数，
       //自定义事件还是由基础事件触发
        private void kr(object sender, EventArgs e)
        {

        }
        //我到底需要做什么?
        //如何通过事件将类内部数据传到类外
        //这种传统算法到底能不能用，还得看具体的图片
        protected override void OnPaint(PaintEventArgs pe)              //重写重绘事件
        {
            base.OnPaint(pe);

        }

        private void DrawLine()
        {
            this.CreateGraphics();
        }

        private void DrawRect()
        {

        }
        
        private void mouseMove()
        {

        }


        private void mouseKeyPress()
        {

        }

        private void mouseKeyUp()
        {

        }
        

        private void meanData()   //数据平滑
        {

        }
    }
}
