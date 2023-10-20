using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
//新加命名空间
using System.Data.SqlClient;
namespace bottleDetection.widget
{
    public partial class StatisticForm : Form
    {
        public StatisticForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login();
        }

        private bool Login()
        {
            //1.打开数据库2.连接数据库3.查询数据库4.验证是否用户名和密码匹配5.显示登录结果6.设置功能可用
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Administrator\Documents\mydatabase.mdf;Integrated Security=True;Connect Timeout=30");
            int n = 0;


            String strUserName = username.Text;
            String strPassWord = password.Text;
            Regex regxUsername = new Regex("^[A-Za-z0-9]{4,15}$");              //用户名只能是字母和数字的组合，长度4-15位
            Regex regxPassword = new Regex("^(?![0-9]+$)(?![a-z]+$)(?![A-Z]+$)(?!([^(0-9a-zA-Z)])+$).{6,20}$");     //密码至少包含：数字,英文,字符中的两种以上，长度6-20

            //核心就是验证用户输入的文本是否符合正则表达式
            Match UserNamematch = regxUsername.Match(strUserName);
            Match passwordmatch = regxPassword.Match(strPassWord);
            if (strUserName == "" || strPassWord == "")
            {
                MessageBox.Show("用户名或密码不能为空");
                return false;
            }
            if (!regxUsername.IsMatch(strUserName))
            {
                MessageBox.Show("用户名只能是字母和数字的组合，长度4-15位");
                return false;
            }
            if (!regxPassword.IsMatch(strPassWord))
            {
                MessageBox.Show("密码至少包含：数字,英文,字符中的两种以上，长度6-20");
                return false;
            }

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
                try
                {
                    command.CommandText = "insert into [dbo].[user] (Id,username,password) values(2,'haha','haha')";
                    //下一步就是验证输入数据正确性后插入表中
                    n = command.ExecuteNonQuery();

                  
                }
                catch
                {

                }
            }
            catch
            {

            }


            return false;
        }

      

        private bool Register()
        {
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Administrator\Documents\mydatabase.mdf;Integrated Security=True;Connect Timeout=30");
            int n = 0;


            String strUserName = username.Text;
            String strPassWord = password.Text;
            Regex regxUsername = new Regex("^[A-Za-z0-9]{4,15}$");              //用户名只能是字母和数字的组合，长度4-15位
            Regex regxPassword = new Regex("^(?![0-9]+$)(?![a-z]+$)(?![A-Z]+$)(?!([^(0-9a-zA-Z)])+$).{6,20}$");     //密码至少包含：数字,英文,字符中的两种以上，长度6-20

            //核心就是验证用户输入的文本是否符合正则表达式
            Match UserNamematch = regxUsername.Match(strUserName);
            Match passwordmatch = regxPassword.Match(strPassWord);
            if (strUserName == "" || strPassWord == "")
            {
                MessageBox.Show("用户名或密码不能为空");
                return false;
            }
            if (!regxUsername.IsMatch(strUserName))
            {
                MessageBox.Show("用户名只能是字母和数字的组合，长度4-15位");
                return false;
            }
            if (!regxPassword.IsMatch(strPassWord))
            {
                MessageBox.Show("密码至少包含：数字,英文,字符中的两种以上，长度6-20");
                return false;
            }

            //执行到这里说明验证通过，可以向数据库中写入数据

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
               
                try
                {

                    // command.CommandText = "insert into test (id, name) values(1,'haha');" +

                    //     "insert into test (id, name) values (2,'hehe')";
                    //必须正确使用sql语句
                    //必须向表中插入数据
                    //加上[dbo].[user]居然插入成功了
                    //保证ID是自增的
                    //设置自增
                    //先验证数据库中没有这个用户
                    command.CommandText = "select * from login where username=" +"'"+ strUserName+"'";
                    //从数据库中读取数据流存入reader中  
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //reader.GetOrdinal("id")是得到ID所在列的index,  
                        //reader.GetInt32(int n)这是将第n列的数据以Int32的格式返回  
                        //reader.GetString(int n)这是将第n列的数据以string 格式返回  
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string name = reader.GetString(reader.GetOrdinal("name"));
                        string pwd = reader.GetString(reader.GetOrdinal("password"));
                        int age = reader.GetInt32(reader.GetOrdinal("age"));
                        string sex = reader.GetString(reader.GetOrdinal("sex"));
                        string phone = reader.GetString(reader.GetOrdinal("phone"));
                        string address = reader.GetString(reader.GetOrdinal("Address"));

                    }
                    if (false)//表中没有相应同用户名的
                    {
                        command.CommandText = "insert into [dbo].[login] (username,password) values(" + "'" + strUserName + "'" + "," + "'" + strPassWord + "'" + ")";
                        //下一步就是验证输入数据正确性后插入表中
                        n = command.ExecuteNonQuery();
                    }
                   
                   
                 

                   
                }
                catch
                {
                    MessageBox.Show("插入数据异常");
                }
                finally

                {

                    //销毁连接对象，释放资源

                    command.Dispose(); 

                }
            }
            catch
            {
                MessageBox.Show("发生异常");
            }
            return false;
        }

        private void register2_Click_1(object sender, EventArgs e)
        {
            Register();
        }

        private void createExcel()
        {

        }

        private void writeExcel()
        {

        }
    }
}
