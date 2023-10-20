using System;
using System.Collections.Generic;

using bottleDetect.ConfigInfo;
using bottleDetection.Tool;
using bottleDetection.widget;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using bottleDetect.ConfigInfo;


namespace bottleDetection
{

    //这个结构体也可以做初始化值

    //这个类只讲编辑框内容写到ini
    //串口参数结构体
    struct COMPORT_ATTRIBUTE
    {
        public int bandrate;
        public int dataBit;
        public Parity parity_check_bit;
        public StopBits stop_bit;
        public string comport_number;
    };

    public partial class systemSet : Form
    {

        private Setting parseSet;
        // 串口参数
        private COMPORT_ATTRIBUTE uartPort;
        //实例化串口类
        public System.IO.Ports.SerialPort _serialPort = new System.IO.Ports.SerialPort();
        //新建一个30字节长度的
        private byte[] strCommnand;
        private string[] serialPortCommand;

        //串口是否打开，false时串口为关闭状态
        private bool isserialPortOpen = false;
        public systemSet()
        {
            InitializeComponent();
            string INIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "test.ini";
            parseSet = new Setting(INIPath);
            serialPortCommand = new string[30] { "AA" , "AA" , "AA" , "AA" , "AA" , "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA", "AA" };
            strCommnand = new byte[30];
            strCommnand[0] = 0xAA;
            strCommnand[2] = 0x80;
            //读取ini文件配置光源和相机结构体
        }
     
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.propertySet.SelectedIndex)
            {
                case 0:
                    loadForm(new tab1());
                    break;
                case 1:
                    break;
            }
        }


        //获取界面上所有的输入控件
        private void getALLInputControl(Object parentWindow, List<Control> controls)
        {
            Control widgrt = parentWindow as Control;

            if (widgrt == null) return;                    //递归出口，没有子控件返回

            foreach (Control ctrl in widgrt.Controls)
            {

                if (ctrl is TextBox || ctrl is ComboBox)
                {
                    //相关操作代码
                    controls.Add(ctrl );


                }
                else
                {
                    getALLInputControl(ctrl, controls);
                }

            }
        }


        private void changeTabPage(String pageName)
        {

        }

        private void loadForm(Object form)
        {
            Form widget = form as Form;

            widget.TopLevel = false;
            widget.Dock = DockStyle.Fill;
            this.propertySet.SelectedTab.Tag = widget;
           
            widget.Show();
        }


        //
        private void button1_Click(object sender, EventArgs e)
        {

            //   string INIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "test.ini";



            List<Control> contrlsList = new List<Control>();

            s_camerConfig cam = new s_camerConfig();
            //


            getALLInputControl(this.propertySet.TabPages[0], contrlsList);
            String imageHeight;
            String imageWidth;
            bool b_isSectionExist;
           

            
            //相机设置是什么?
            //相机有哪些设置?答：依据数据类型的不同，大恒水星相机属性可以分为七类1.整型2.浮点3.枚举4.布尔5.字符串6.命令型7.读写控制
            //如何设置相机,将数据保存到一个结构体然后遍历结构体分别设置每个成员。结构体数据成员可以来自ini或者winform控件
            //要设置的结构体成员名来自哪里?可以看大恒相机手册看大恒如何命名的
            //

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
        //两个问题：1.桌面上的值如何获取2.获取后如何设置到相机
        //读取ini到结构体2.结构体写入到相机
        private void systemSet_Load(object sender, EventArgs e)
        {


        }

        //index:相机列表下标
        private void setCamerInfo(int index)
        {
            s_camerConfig cam = new s_camerConfig();

        }

        private void camList_SelectedIndexChanged(object sender, EventArgs e)
        {

           
      
            
        }

        private void  getINIContent(String section)
        {
            //对于相机设置需完成1.从结构体设置到相机2.从配置文件读取到界面3.从界面写到结构体
            s_camerConfig cam = new s_camerConfig();
            int imageHeight;
            int imageWidth;
            string INIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "test.ini";
            Setting parseSet = new Setting(INIPath);
            parseSet.beginGroup(section);
            parseSet.value("imageHeight", 34, out imageHeight);
            parseSet.value("imageWidth", 34, out imageWidth);
          
           
            parseSet.endGroup();
        }
        //控制偶数行
      

        private void ScannSerialPort_Click(object sender, EventArgs e)
        {
            string[] portList = SerialPort.GetPortNames();
            string lastName = "";

            serialPort.Items.Clear();//清除数据
            if (portList.Length == 0)
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                //获取有多少个COM口就添加进COMBOX项目列表  
                serialPort.Items.Add(s);
                lastName = s;//保存最新的一个

            }
            serialPort.Text = lastName;//显示最新的一个串口
            uartPort.comport_number = lastName;//赋值变量

        }

        private void Order_40H_CheckedChanged(object sender, EventArgs e)
        {
            //strCommnand[2] = "40";
          
            strCommnand[2] = 0x40;
        }
        private void Order_C0H_CheckedChanged(object sender, EventArgs e)
        {
           
            strCommnand[2] = 0xC0;
        }

        private void Order_20H_CheckedChanged(object sender, EventArgs e)
        {
           
            strCommnand[2] = 0x20;
        }

        private void Order_60H_CheckedChanged(object sender, EventArgs e)
        {
            
            strCommnand[2] = 0x60;
        }

        private void Order_04H_CheckedChanged(object sender, EventArgs e)
        {
           
            strCommnand[2] = 0x04;
        }

        private void Order_80H_CheckedChanged(object sender, EventArgs e)
        {
            
            strCommnand[2] = 0x80;
        }
        //奇数改变
        private void OddNumberTrackbar_ValueChanged(object sender, EventArgs e)
        {
            oddNumbertextBox.Text = oddNumberTrackbar.Value.ToString(); 

            trackBar_1.Value = oddNumberTrackbar.Value;
            textBox_1.Text = oddNumberTrackbar.Value.ToString();
            trackBar_3.Value = oddNumberTrackbar.Value;
            textBox_3.Text = oddNumberTrackbar.Value.ToString();
            trackBar_5.Value = oddNumberTrackbar.Value;
            textBox_5.Text = oddNumberTrackbar.Value.ToString();
            trackBar_7.Value = oddNumberTrackbar.Value;
            textBox_7.Text = oddNumberTrackbar.Value.ToString();
            trackBar_9.Value = oddNumberTrackbar.Value;
            textBox_9.Text = oddNumberTrackbar.Value.ToString();
            trackBar_11.Value = oddNumberTrackbar.Value;
            textBox_11.Text = oddNumberTrackbar.Value.ToString();
            trackBar_13.Value = oddNumberTrackbar.Value;
            textBox_13.Text = oddNumberTrackbar.Value.ToString();
            trackBar_15.Value = oddNumberTrackbar.Value;
            textBox_15.Text = oddNumberTrackbar.Value.ToString();
            trackBar_17.Value = oddNumberTrackbar.Value;
            textBox_17.Text = oddNumberTrackbar.Value.ToString();
            trackBar_19.Value = oddNumberTrackbar.Value;
            textBox_19.Text = oddNumberTrackbar.Value.ToString();
        }
        
        private void EvenNumbertrackBar_ValueChanged(object sender, EventArgs e)
        {
            evenNumbertextBox.Text = evenNumbertrackBar.Value.ToString();

            trackBar_2.Value = evenNumbertrackBar.Value;
            textBox_2.Text = evenNumbertrackBar.Value.ToString();
            trackBar_4.Value = evenNumbertrackBar.Value;
            textBox_4.Text = evenNumbertrackBar.Value.ToString();
            trackBar_6.Value = evenNumbertrackBar.Value;
            textBox_6.Text = evenNumbertrackBar.Value.ToString();
            trackBar_8.Value = evenNumbertrackBar.Value;
            textBox_8.Text = evenNumbertrackBar.Value.ToString();
            trackBar_10.Value = evenNumbertrackBar.Value;
            textBox_10.Text = evenNumbertrackBar.Value.ToString();
            trackBar_12.Value = evenNumbertrackBar.Value;
            textBox_12.Text = evenNumbertrackBar.Value.ToString();
            trackBar_14.Value = evenNumbertrackBar.Value;
            textBox_14.Text = evenNumbertrackBar.Value.ToString();
            trackBar_16.Value = evenNumbertrackBar.Value;
            textBox_16.Text = evenNumbertrackBar.Value.ToString();
            trackBar_18.Value = evenNumbertrackBar.Value;
            textBox_18.Text = evenNumbertrackBar.Value.ToString();
            trackBar_20.Value = evenNumbertrackBar.Value;
            textBox_20.Text = evenNumbertrackBar.Value.ToString();

        }

        private void FilltrackBar_ValueChanged(object sender, EventArgs e)
        {
            FilltextBox.Text = FilltrackBar.Value.ToString();
        }

        private void OpenSerialPort_Click(object sender, EventArgs e)
        {
            //如果串口已经打开则应该关闭串口
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                openSerialPort.Text = "打开串口";
            }
            try
            {
                _serialPort.PortName = serialPort.Text;     //端口号
                _serialPort.BaudRate = 9600;                //波特率
                _serialPort.DataBits = 8;                   //数据位
                _serialPort.Parity = Parity.None;           //校验位
                _serialPort.StopBits = StopBits.One;       //停止位
              //  _serialPort.Handshake = Handshake.None;
                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    MessageBox.Show("打开成功");
                }
                openSerialPort.Text = "关闭串口";

               

            }
            catch
            {
                MessageBox.Show("打开串口失败");
            }
       
        }
        //核心：1.十进制整型转为16进制字节2.16进制字符转为16进制字节
        private void SendCommand_Click(object sender, EventArgs e)
        {
            string resultStr = "";
            if (externalTrrigger.Checked)
            {
                
                strCommnand[1] = 0x01;
            }
            if (internalTrriger.Checked)
            {
              
                strCommnand[1] = 0x02;
            }
            if (readDeviceInfo.Checked)
            {
               
                strCommnand[1] = 0x03;
            }
            if (timeOfExposure.Text != "")
            {
                int timeofExposure = Convert.ToInt32(timeOfExposure.Text);
                string tempStr = "";
                
                tempStr = timeofExposure.ToString("X4");  //亮闪时间最大是1000，因此需要四位16进制数
               
                if (tempStr.Length != 4)
                {
                    tempStr = 0 + tempStr;
                }
                if (tempStr.Length == 4)
                {
                    
                    //这里将16进制字符转为整数再写到字节
                    strCommnand[3] =0xAA;
                    
                    strCommnand[4] = 0XAA;
                    strCommnand[3] = Convert.ToByte(Convert.ToInt32(tempStr[0].ToString()+ tempStr[1].ToString(),16));
                    strCommnand[4] = Convert.ToByte(Convert.ToInt32(tempStr[2].ToString() + tempStr[3].ToString(), 16));
                }

               
            }
          
            //


           //写串口时可以直接写10进制


            strCommnand[5] = Convert.ToByte(trackBar_1.Value);
            strCommnand[6] = Convert.ToByte(trackBar_2.Value);
            strCommnand[7] = Convert.ToByte(trackBar_3.Value);
            strCommnand[8] = Convert.ToByte(trackBar_4.Value);
            strCommnand[9] = Convert.ToByte(trackBar_5.Value);
            strCommnand[10] = Convert.ToByte(trackBar_6.Value);
            strCommnand[11] = Convert.ToByte(trackBar_7.Value);
            strCommnand[12] = Convert.ToByte(trackBar_8.Value);
            strCommnand[13] = Convert.ToByte(trackBar_9.Value);
            strCommnand[14] = Convert.ToByte(trackBar_10.Value);
            strCommnand[15] = Convert.ToByte(trackBar_11.Value);
            strCommnand[16] = Convert.ToByte(trackBar_12.Value);
            strCommnand[17] = Convert.ToByte(trackBar_13.Value);
            strCommnand[18] = Convert.ToByte(trackBar_14.Value);
            strCommnand[19] = Convert.ToByte(trackBar_15.Value);
            strCommnand[20] = Convert.ToByte(trackBar_16.Value);
            strCommnand[21] = Convert.ToByte(trackBar_17.Value);
            strCommnand[22] = Convert.ToByte(trackBar_18.Value);
            strCommnand[23] = Convert.ToByte(trackBar_19.Value);
            strCommnand[24] = Convert.ToByte(trackBar_20.Value);



            strCommnand[25] = 0x00;
            strCommnand[26] = 0x00;
            strCommnand[27] = 0x00;
            strCommnand[28] = Convert.ToByte(FilltrackBar.Value);
            strCommnand[29] = 0x55;

          

           

            //10进制整数转化为16进制字节

           
           
            foreach (byte single in strCommnand)
            {
                Console.WriteLine(single.ToString());
            }
            //如何把十进制数转化为16进制字节
            //手工输入的都是正确的
           _serialPort.Write(strCommnand, 0, 30);
            //  byte[] b = new byte[30];
            //  _serialPort.Read(b,0,30);



        }
     
        private void Button1_Click_1(object sender, EventArgs e)
        {
            s_CameraParam camerparam = new s_CameraParam();
            List<Control> allControls = new List<Control>();
            getALLInputControl(groupBox9, allControls);

            foreach (Control item in allControls)
            {
                switch (item.Name)
                {
                    case "Width":
                        if (item.Text != "")
                        {
                            camerparam.Width = Convert.ToInt32(item.Text);
                        }
                        
                        break;
                }
            }


           
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //如何把配置文件信息读到一个结构体中?
        }
    }
}
