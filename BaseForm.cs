using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace baseForm
{
    /// <summary>
    /// 控件自适应窗体大小
    /// </summary>
    public partial class BaseForm : Form
    {
        #region 控件缩放
        double formWidth;//窗体原始宽度
        double formHeight;//窗体原始高度
        double scaleX;//水平缩放比例
        double scaleY;//垂直缩放比例
        Dictionary<string, string> controlInfo = new Dictionary<string, string>();
        //控件中心Left,Top,控件Width,控件Height,控件字体Size
        /// <summary>
        /// 获取所有原始数据
        /// </summary>
        protected void GetAllInitInfo(Control CrlContainer)
        {
            
            if (CrlContainer.Parent == this)
            {
                formWidth = Convert.ToDouble(CrlContainer.Width);
                formHeight = Convert.ToDouble(CrlContainer.Height);
            }
            foreach (Control item in CrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                    controlInfo.Add(item.Name, (item.Left + item.Width / 2) + "," + (item.Top + item.Height / 2) + "," + item.Width + "," + item.Height + "," + item.Font.Size);
                if ((item as UserControl) == null && item.Controls.Count > 0)
                    GetAllInitInfo(item);
            }
        }

        private void ControlsChangeInit(Control CrlContainer)
        {
            scaleX = (Convert.ToDouble(CrlContainer.Width) / formWidth);
            scaleY = (Convert.ToDouble(CrlContainer.Height) / formHeight);
        }

        /// <summary>
        /// 设置控件大小以及样式
        /// </summary>
        /// <param name="CrlContainer"></param>
        private void ControlsChange(Control CrlContainer)
        {

            MessageBox.Show("2");
            double[] pos = new double[5];//pos数组保存当前控件中心Left,Top,控件Width,控件Height,控件字体Size
            foreach (Control item in CrlContainer.Controls)
            {
                //判断有此控件
                if (item.Name.Trim() != "")
                {
                    if ((item as UserControl) == null && item.Controls.Count > 0)
                        ControlsChange(item);
                    //获取窗体改变前的控件信息
                    string[] strs = controlInfo[item.Name].Split(',');
                    for (int j = 0; j < 5; j++)
                    {
                        pos[j] = Convert.ToDouble(strs[j]);
                    }
                    //设置窗体改变之后的控件信息
                    double itemWidth = pos[2] * scaleX;
                    double itemHeight = pos[3] * scaleY;
                    //左边距
                    item.Left = Convert.ToInt32(pos[0] * scaleX - itemWidth / 2);
                    //上边距
                    item.Top = Convert.ToInt32(pos[1] * scaleY - itemHeight / 2);
                    //控件宽度
                    item.Width = Convert.ToInt32(itemWidth);
                   

                    //控件高度
                    item.Height = Convert.ToInt32(itemHeight);
                    try
                    {
                        //字体大小  可更具自己需要选择是否调整
                        item.Font = new Font(item.Font.Name, float.Parse((pos[4] * Math.Min(scaleX, scaleY)).ToString()));
                    }
                    catch
                    {

                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 窗体改变大小
        /// </summary>    
        protected override void OnSizeChanged(EventArgs e)
        {

            //这个代码执行了
            base.OnSizeChanged(e);
            if (controlInfo.Count > 0)
            {
                ControlsChangeInit(this.Controls[0]);
                //获取Controls实例
                ControlsChange(this.Controls[0]);
            }
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {

        }
    }
}

