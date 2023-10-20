using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//窗体初始化时(new 了之后获取窗体的宽度和高度2)获取窗体的初始宽度和高度3.遍历窗体控件进行初始化设置缩放位置
namespace bottleDetection.widget
{
    public partial class PLCForm : Form
    {
        private readonly float initialWidth;
        private readonly float initialHeight;

        public PLCForm()
        {
            InitializeComponent();
            initialWidth = Width;
            initialHeight = Height;
            setTag(this);
        }

        private void setTag(Control control)
        {
            foreach (Control item in control.Controls)
            {
                item.Tag = item.Width + ";" + item.Height + ";" + item.Left + ";" + item.Top + ";" + item.Font.Size;
                if (item.Controls.Count > 0) setTag(item);
            }
        }

        private void setControls(float scaleX, float scaleY,Control control)
        {
            foreach (Control item in control.Controls)
            {
                if(item.Tag != null)
                {
                    var myTag = item.Tag.ToString().Split(';');

                    item.Width = Convert.ToInt32(Convert.ToSingle(myTag[0]) * scaleX);
                    item.Height = Convert.ToInt32(Convert.ToSingle(myTag[1]) * scaleY);
                    item.Left = Convert.ToInt32(Convert.ToSingle(myTag[2]) * scaleX);
                    item.Top = Convert.ToInt32(Convert.ToSingle(myTag[3]) * scaleY);

                 //   var currentSize = Convert.ToSingle(myTag[4])* scaleY;
                 //   if (currentSize > 0) item.Font = new Font(item.Font.Name,currentSize,item.Font.Style,item.Font.Unit);
                 //实验只改变位置，不改变大小成功
                   
                    if (item.Controls.Count > 0) setControls(scaleX, scaleY,item);
                }
            }
        }

        public void ReWinformLayout(int inputWidth, int inputHeight)
        {

            var scaleX = inputWidth / initialWidth;
            var scaleY = inputHeight / initialHeight;
            setControls(scaleX, scaleY, this);
        }

       

        private void SettingSpecialParam_CheckedChanged(object sender, EventArgs e)
        {
            if (settingSpecialParam.Checked)
            {
                groupBoxSpecialParam.Visible = true;
            }
            else
            {
                groupBoxSpecialParam.Visible = false;
            }
        }

        private void SetThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (setThreshold.Checked)
            {
                groupBoxThresholdSetting.Visible = true;
            }
            else
            {
                groupBoxThresholdSetting.Visible = false;
            }
        }

        private void checkBox78_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox78.Checked)
            {
                forearm_checkbox_warning.Checked = true;
                grasp_checkbox_warning.Checked =  true;
                back_checkbox_warning.Checked = true;
                front_checkbox_warning.Checked = true;
                crow_checkbox_warning.Checked = true;
                photoelectricity1_checkbox_warning.Checked = true;
                photoelectricity2_checkbox_warning.Checked = true;
                photoelectricity3_checkbox_warning.Checked = true;
                photoelectricity4_checkbox_warning.Checked = true;
                noteleminate_checkbox_warning.Checked = true;
                confirm_checkbox_warning.Checked = true;
                pressure_checkbox_warning.Checked = true;
                frontsoftware_checkbox_warning.Checked = true;
                graspsoftware_checkbox_warning.Checked = true;
                backsoftware_checkbox_warning.Checked = true;
                photoelectricity5_checkbox_warning.Checked = true;
                electricmachine1_checkbox_warning.Checked = true;
                electricmachine2_checkbox_warning.Checked = true;
                CATfault_checkbox_warning.Checked = true;
                PLCfault_checkbox_warning.Checked = true;
                IO_checkbox_warning.Checked = true;
                frontcamera_checkbox_warning.Checked = true;
                graspcamera_checkbox_warning.Checked = true;
                backcamera_checkbox_warning.Checked = true;
                emergencystoping_checkbox_warning.Checked = true;
                continuousele_checkbox_warning.Checked = true;


















            }
            else
            {
                forearm_checkbox_warning.Checked = false;
                grasp_checkbox_warning.Checked = false;
                back_checkbox_warning.Checked = false;
                front_checkbox_warning.Checked = false;
                crow_checkbox_warning.Checked = false;
                photoelectricity1_checkbox_warning.Checked = false;
                photoelectricity2_checkbox_warning.Checked = false;
                photoelectricity3_checkbox_warning.Checked = false;
                photoelectricity4_checkbox_warning.Checked = false;
                noteleminate_checkbox_warning.Checked = false;
                confirm_checkbox_warning.Checked = false;
                pressure_checkbox_warning.Checked = false;
                frontsoftware_checkbox_warning.Checked = false;
                graspsoftware_checkbox_warning.Checked = false;
                backsoftware_checkbox_warning.Checked = false;
                photoelectricity5_checkbox_warning.Checked = false;
                electricmachine1_checkbox_warning.Checked = false;
                electricmachine2_checkbox_warning.Checked = false;
                CATfault_checkbox_warning.Checked = false;
                PLCfault_checkbox_warning.Checked = false;
                IO_checkbox_warning.Checked = false;
                frontcamera_checkbox_warning.Checked = false;
                graspcamera_checkbox_warning.Checked = false;
                backcamera_checkbox_warning.Checked = false;
                emergencystoping_checkbox_warning.Checked = false;
                continuousele_checkbox_warning.Checked = false;
            }
            
        }
    }
}
