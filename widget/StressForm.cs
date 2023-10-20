using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bottleDetect.ConfigInfo;
namespace bottleDetection.widget
{    //排列原则1.能排6列的直接排6列，例如6、12、18
    //小于6的按列数排
    //大于6且不能整除
    //其实排列只有6，12，18种
    public partial class StressForm : Form
    {
        //动态计算成员开始
        private List<Rect> pictureBoxList;
        public List<PictureBox> pictureList;//保存生成的pictureBox列表

        private int gapRow = 15;           //行间距

        private int gapColunm = 15;        //列间距

        private int Row;                    //单元格行数
        private int Column;                 //单元格列数

        private int camerNum;               //相机数


        private int WindowHeight = 0;       //窗口高度
        private int WindowWidth = 0;
        private int PictureBoxWidth = 0;
        private int PictureBoxHeight = 0;

        private int PaddingLeft = 0;
        private int PaddingTop = 0;
        private int PaddingRight = 0;
        private int PaddingBottom = 0;

        //计算原则1.
        public StressForm()
        {
            InitializeComponent();
            pictureBoxList = new List<Rect>();
            pictureList = new List<PictureBox>();
            PaddingLeft = 5;
            PaddingTop = 5;
            PaddingRight = 5;
            PaddingBottom = 5;
        }


        private void calculateLyout()
        {
            if (camerNum <= 6)
            {
                Row = 2;
                Column = 6;
                return;
            }
            if (camerNum <= 12 && camerNum > 6)
            {
                Row = 2;
                Column = 6;
                return;
            }
            if (camerNum > 12 && camerNum <= 18)
            {
                Row = 3;
                Column = 6;
                return;
            }
            //一个立柱相机只发出一个信号,这样线程就必须访问到公共父变量
            //if (pictureBoxList.Count > 12 && pictureBoxList.Count <= 18) //三列式布局
            //{
            //    Row = 3;
            //    Column = (int)Math.Ceiling(Convert.ToDouble(camerNum) / Convert.ToDouble(Row));        //向上取整生成列数
            //    return;
            //}
        }

        private void generatePictureList()
        {
            int pictureBoxNum = 0;

            //if(Column == 1&&Row == 0)
            //{
            //    Rect pictureBox = new Rect();
            //    pictureBox.iLeftPointCol = 0;
            //    pictureBox.iLeftPointRow = 0;
            //    pictureBox.Width = 500;
            //    pictureBox.Height = 500;
            //    pictureBoxList.Add(pictureBox);
            //    return;
            //}
            PictureBoxWidth = (WindowWidth - (PaddingLeft + PaddingRight) - (Column - 1) * gapColunm) / Column;
            PictureBoxHeight = (WindowHeight - (PaddingBottom + PaddingTop) - (Row - 1) * gapRow) / Row;

            if (Row > 0 && Column > 0)
            {
                for (int j = 0; j < Column; j++)
                {
                    for (int i = 0; i < Row; i++)//颠倒是可以的，保持列不变，行不断变化
                    {
                        Rect pictureBox = new Rect();
                        pictureBox.iLeftPointCol = j * PictureBoxWidth + j * gapColunm + PaddingLeft;// 生成行坐标和列坐标
                        pictureBox.iLeftPointRow = i * PictureBoxHeight + i * gapRow + PaddingTop;
                        pictureBox.Width = PictureBoxWidth;
                        pictureBox.Height = PictureBoxHeight;

                        pictureBoxList.Add(pictureBox);                     //将
                        pictureBoxNum += 1;
                        if (pictureBoxNum > camerNum)               //超出最大量直接
                        {
                            return;
                        }
                    }

                }

            }


        }

        private void drawPictureBoxList()
        {
            int camerID = 1;

            foreach (Rect item in pictureBoxList)
            {
                PictureBox ImageContainer = new PictureBox();
                ImageContainer.BackColor = Color.Black;
                ImageContainer.Location = new Point(item.iLeftPointCol, item.iLeftPointRow);
                ImageContainer.Size = new Size(item.Width, item.Height);
                ImageContainer.Name = "camera" + camerID.ToString();
                ImageContainer.Tag = camerID;
                //下一步就是绑定
                camerID += 1;
                this.Controls.Add(ImageContainer);
                pictureList.Add(ImageContainer);
            }
            //看样子就是位置算错了

        }


        public void set(int cameraNum)
        {
            camerNum = cameraNum;
            WindowHeight = Height;
            WindowWidth = Width;

            calculateLyout();
            generatePictureList();
            drawPictureBoxList();
        }
    }
}
