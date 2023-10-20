using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bottleDetection.widget
{
    public partial class PLCStatus : Form
    {
        private readonly float initialWidth;
        private readonly float initialHeight;
        public PLCStatus()
        {
            InitializeComponent();
            initialWidth = Width;
            initialHeight = Height;
            setTag(panel1);
            
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

        public void ReWinformLayout(int inputWidth, int inputHeight)
        {

            var scaleX = inputWidth / initialWidth;
            var scaleY = inputHeight / initialHeight;
            setControls(scaleX, scaleY, panel1);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void pLC设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //在这里跳转到PLC设置界面
            openPLC();
        }

        public event Action openPLC;            //Action表示事件没有参数

    }
}
