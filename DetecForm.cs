using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

//新加命名空间
using System.Data.SqlClient;
using System.Reflection;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bottleDetection.algothrim;
using System.IO;
using bottleDetection.Tool;
//善用右键选择控件
namespace bottleDetection
{
    public  partial  class DetecForm : Form
    {
        public camControl cameraInstance;

        public List<PictureBox> pictureList;

        public List<detectThread> threadList = new List<detectThread>(3);

        public DetecForm()
        {
            InitializeComponent();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void leftControl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void m_startCapture_Click(object sender, EventArgs e)
        {

            //在这里新建线程
        
            cameraInstance = new camControl();
            bool openres = cameraInstance.openAllCamera();
            if (openres == true)
            {
                MessageBox.Show("打开成功");

                cameraInstance.openAllCamera();
                cameraInstance.initCamParam();
                cameraInstance.bindCaptrueCallbak();

                for (int i = 0; i < cameraInstance.l_camList.Count; i++)
                {
                    detectThread detectObject = new detectThread(cameraInstance.l_camList[i]);

                    threadList.Add(detectObject);
                    detectObject.startThread();
                }


            }
            else
            {
                MessageBox.Show("打开失败");
            }

        }

        private void Button7_Click(object sender, EventArgs e)
        {
            cameraInstance.closeAllCamera();
        }

        private void DetecForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cameraInstance.closeAllCamera();
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < threadList.Count; i++)
            {
                threadList[i].killThread();
            }

        }

     private void getALLPictureBoxs(Object   parentWindow, List<PictureBox> pictureBoxList)
        {
            Control widgrt = parentWindow as Control;

            if (widgrt == null) return;                    //递归出口，没有子控件返回

            foreach (Control ctrl in widgrt.Controls)
            {
                
                if (ctrl is PictureBox)
                {
                    //相关操作代码
                    pictureBoxList.Add(ctrl as PictureBox);
                   

                }
                else
                {
                    getALLPictureBoxs(ctrl, pictureBoxList);
                }
              
            }
        }


        


        private void WriteINI_Click(object sender, EventArgs e)
        {
            

            //要使用代码创建存储过程
            //定义数据库连接语句:服务器=.(本地) 数据库名=PhoneMS(手机管理系统)


            //下一步就是下载开源项目，学习登录以及数据表的设计
            //1.设计数据表2.学习登录逻辑3.设置正则表达式，确保用户名只能是数字、大小写字符以及@；密码只能是数字特殊字符大小写字母4.必须要有机制对控件可操作性限制/解除
            //登录流程1.获取用户输入数据2.验证数据是否合格(不为空、且符合正则表达式)3.查询数据库(可以利用存储过程也可以直接执行sql语句)5.检验查询结果，验证结果是否与数据库存储字段相匹配如果匹配允许登录，如果不匹配拒绝登录
            //命名1.保存用户名和密码strUserName strPassWord2.验证用户名和密码的正则regxUsername、regxPassWord3.
            //
            string consqlserver = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Administrator\Documents\mydatabase.mdf;Integrated Security=True;Connect Timeout=30";
            //定义SQL Server连接对象
            SqlConnection con = new SqlConnection(consqlserver);
            //打开连接
            con.Open();


            //创建一个SQL命令对象

            SqlCommand command = new SqlCommand();

            //设置命令所使用的数据库连接

            command.Connection = con;

            //设置命令类型


            //正确创建存储过程方法是使用代码创建
            command.CommandType = CommandType.Text;
            command.CommandText = "CREATE PROCEDURE selecname @Pname varchar(20) as  select id as 手机品牌,name as 手机型号 from test where name=@Pname return 0";
        //    int n = command.ExecuteNonQuery();

            //定义数据库执行一个SQL语句或存储过程                   
            SqlCommand com = new SqlCommand("selecname", con);
            //指定命令类型为存储过程  
            com.CommandType = CommandType.StoredProcedure;
            //存储过程添加变量并赋值给textBox1    
            com.Parameters.Add("@Pname", SqlDbType.NVarChar, 20).Value = "haha";
            //定义获取数据
            SqlDataAdapter da = new SqlDataAdapter(com);
            DataSet ds = new DataSet();

            try
            {
                da.Fill(ds);                                  //填充数据
                dataGridView1.DataSource = ds.Tables[0];      //显示在dataGridView中
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);                  //异常处理
            }
            finally
            {
                con.Close();                                   //关闭连接
                con.Dispose();                                 //释放连接
                da.Dispose();                                  //释放资源
            }



        }

        private void m_showNormalImage_Click(object sender, EventArgs e)
        {//现在应该能正常使用sql Server数据库了
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Administrator\Documents\mydatabase.mdf;Integrated Security=True;Connect Timeout=30");

            try

            {

                //打开数据库连接

                connection.Open();

                //输出数据库名称

                Console.WriteLine("数据库名称：{0}", connection.Database);

                //创建一个SQL命令对象

                SqlCommand command = new SqlCommand();

                //设置命令所使用的数据库连接

                command.Connection = connection;

                //设置命令类型

                command.CommandType = CommandType.Text;



                //创建表

              //  command.CommandText = "create table test (id int, name nvarchar(10))";

                try

                {

                    //执行命令 n为影响行数

                    

                  

                  
                    //插入数据

                    command.CommandText = "insert into test (id, name) values(1,'haha');" +

                        "insert into test (id, name) values (2,'hehe')";

                   int n = command.ExecuteNonQuery();

                    Console.WriteLine("插入数据成功，影响行数{0}", n);

                    

                    //查询数据

                    command.CommandText = "select id from test";

                    // command.ExecuteScalar()执行SQL命令，并返回结果集中第一行第一列的值

                    object o = command.ExecuteScalar();

                    Console.WriteLine("查询执行成功，结果是{0}", o);



                    //命令文本中可以包含一条以上的SQL语句

                    //command.CommandText = "delete from test; droptable test";

                    //command.ExecuteNonQuery();

                    //Console.WriteLine("删除数据，删除表成功");

                }
                catch
                {
                    MessageBox.Show("插入数据失败");
                }

                finally

                {

                    //销毁连接对象，释放资源

                    command.Dispose();

                }

            }
            catch
            {

            }
        }

        private void initializePLC()            //
        {

        }

        private void initializeDatabase()
        {

        }
    }
}
