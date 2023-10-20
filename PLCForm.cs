using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OmronFinsTCP.Net;
using bottleDetection.Tool;
//首先我要先把每个框都给一个独一无二且意义明确的命名2.然后要设置一个字符串命名与地址的对应map3.最后要遍历编辑框数据的结构体，使用反射获得名在map中找到该名对应的地址将数据写入
//可以直接用数字序号和地址对应，不需要名_等命名，直接设置一个map，将序号和PLC地址对应即可
namespace bottleDetection
{
    public struct s_isCheckbox {

        public bool warning;
        public bool stopping;
        public bool trimmer;



    }
    public struct PLCData
    {
        public s_isCheckbox forearm;
        public s_isCheckbox gripper;
        public s_isCheckbox realarm;
        public s_isCheckbox frontinvertedbottle;
    }
    //
    public partial class PLCForm : Form
    {
        public PLCForm()
        {
            InitializeComponent();
        }

        private Dictionary<string ,int> map = new Dictionary<string, int>();
        private EtherNetPLC plc = new EtherNetPLC();
        private short D100;
        private short W100_01; //W100.01

        private void button1_Click(object sender, EventArgs e)
        {
            //获取所有的checkbox
            List<CheckBox> checkbox = new List<CheckBox>();
            //获取所有的子控件
            List<Control> contros = new List<Control>();
            getALLCheckbox(PLCWarningSet, checkbox);
            PLCData plc = new PLCData();
            //变量的命名要凸显两点:1.变量的类型2.变量的意义(作用)
            String[] checkboxType ;
            String iniStatus = "";
            //遍历获取到的checkbox列表
            foreach (CheckBox item in checkbox)
            {
                checkboxType = Regex.Split(item.Name, "_", RegexOptions.IgnoreCase);
                switch (checkboxType[0])
                {
                    case "forearm":
                        {
                            
                            switch (checkboxType[2])
                            {
                                case "warn":
                                    //可以在这里生成ini字符串
                                    if (item.Checked)
                                    {
                                        plc.forearm.warning = true;
                                        iniStatus += "1,";
                                    }
                                    else
                                    {
                                        plc.forearm.warning = false;
                                        iniStatus += "0,";
                                    }
                                    break;
                            }

                        }




                        break;

                    case "gripper":
                        {

                            switch (checkboxType[2])
                            {
                                case "warn":
                                    if (item.Checked)
                                    {
                                        plc.forearm.warning = true;
                                    }
                                    else
                                    {
                                        plc.forearm.warning = false;
                                    }
                                    break;

                                case "stopping":
                                    break;
                            }

                        }




                        break;
                }
            }


            getAllControls(this.normalParamSet, contros);

            if (contros.Count > 0)
            {
                foreach (Control item in contros)
                {
                    //获取控件的类型和名称
                    switch (item.GetType().ToString())
                    {
                        case "system.web.ui.webcontrols.button":
                            if (map.ContainsKey(item.Name))     //如果
                            {
                               // writePLC(item.Text,map[item.Name])//写入PLC
                            }
                            break;
                    }
                }
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void getAllControls(Object parentWindow, List<Control> controlList)
        {
            Control widget = parentWindow as Control;
            if (widget == null) return;                    //递归出口，没有子控件返回

            foreach (Control ctrl in widget.Controls)      //将子控件加入返回队列
            {
                controlList.Add(ctrl);                      
                getAllControls(ctrl, controlList);          //递归执行
            }
        }
        private void getALLCheckbox(Object parentWindow, List<CheckBox> checkBoxList)
        {
            Control widgrt = parentWindow as Control;

            if (widgrt == null) return;                    //递归出口，没有子控件返回

            foreach (Control ctrl in widgrt.Controls)
            {

                if (ctrl is CheckBox)
                {
                    //相关操作代码
                    checkBoxList.Add(ctrl as CheckBox);


                }
                else
                {
                    getALLCheckbox(ctrl, checkBoxList);
                }

            }
        }

        private void checkBox3_CheckStateChanged(object sender, EventArgs e)
        {
            String checkType = "forearm";
            String[] checkboxType;
            List<CheckBox> checkbox = new List<CheckBox>();
            getALLCheckbox(PLCWarningSet, checkbox);
            if (checkBox3.Checked)
            {
                foreach (CheckBox item in checkbox)
                {
                    checkboxType = Regex.Split(item.Name, "_", RegexOptions.IgnoreCase);
                 
                    if(checkboxType.Length == 3)
                    {
                        switch (checkboxType[2])
                        {
                            case "warning":
                                item.Checked = true;
                                break;
                        }
                    }
                   
                }



            }
            else
            {
                foreach (CheckBox item in checkbox)
                {
                    checkboxType = Regex.Split(item.Name, "_", RegexOptions.IgnoreCase);

                    if (checkboxType.Length == 3)
                    {
                        switch (checkboxType[2])
                        {
                            case "warning":
                                item.Checked = false;
                                break;
                        }
                    }

                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {


         

            //Invoke方法是同步的方法，所以执行过程是有先后顺序的，所以就不会出现那个异常了
            //创建线程
            Thread newThread = new Thread(new ThreadStart(readPLCThread));
            //加上这句话，否则在关闭窗体时会出现如下错误：在创建窗口句柄之前，不能在控件上调用 Invoke 或 BeginInvoke。
            newThread.IsBackground = true;
            newThread.Start();


        }

        private void readPLCThread()
        {


            while(true)
            {
                plc.ReadWord(PlcMemory.DM, 100, out D100);      //读PLC中DM存储区 D100，返回short
                this.Invoke((EventHandler)(delegate
                {
                    textBox1.Text = D100.ToString();
                }));
            }
          

        }

        private void PLCForm_Load(object sender, EventArgs e)
        {
            map.Add("textBox8", 0x8);
            map.Add("textBox9",0x9);
            map.Add("textBox10",0x10);
            map.Add("textBox11",0x12);
            map.Add("comboBox1",0x13);
            map.Add("comboBox2",0x14);
            map.Add("textBox15",0x15);
            map.Add("textBox19",0x16);
            map.Add("textBox18",0x17);
            map.Add("textBox17",0x18);
            map.Add("textBox16",0x19);

            //打开PLC
         //   bool isConnected = !Convert.ToBoolean(plc.Link("192.168.53.110", 9600, 5000)); //成功，返回0。  ip地址，端口号，超时延时

          


        }

        private void PLCForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            plc.Close();
        }

       
    }
}
