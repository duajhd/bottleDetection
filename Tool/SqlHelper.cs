using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//新加命名空间
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
namespace bottleDetection.Tool
{

   


    //1.明确数据库操作模块怎么封装2.明确数据库模板有哪些功能3.要明白登录的流程是怎样的4.明确需要创建哪些表，哪些表登录的时候用，哪些表存储检测结果用等等

    public class SQLServerDatabase
    {
        private static SqlConnection m_Connection = null;

        public SQLServerDatabase(string strConnection)
        {
            m_Connection = new SqlConnection(strConnection);
            OpenConnection();
        }
        // 链接数据库操作
        public SQLServerDatabase(string strServerName, string strDatabaseName, string strUserName, string strPassword, bool bIntegratedSecurity)
        {
            string strConnection = "data source = " + strServerName + ";initial catalog = " + strDatabaseName;
            if (bIntegratedSecurity)
            {
                strConnection += ";Integrated Security = SSPI";
            }
            else
            {
                strConnection += ";user id = ";
                strConnection += strUserName;
                strConnection += ";password = ";
                strConnection += strPassword;
            }
            m_Connection = new SqlConnection(strConnection);
            OpenConnection();
        }
        // 根据sql语句获得datatable
        public DataTable GetTableBySQL(string strSQL, bool bAddWithKey = false)
        {
            SqlCommand cmd = new SqlCommand(null, m_Connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSQL;

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            DataTable dTable = new DataTable();
            da.Fill(dTable);

            return dTable;
        }
        //获得datarow根据sql语句
        public DataRow GetRowBySQL(string strSQL)
        {
            DataTable dTable = GetTableBySQL(strSQL);

            if (dTable.Rows.Count == 0)
                return null;
            else
                return dTable.Rows[0];
        }
        // 执行sql语句
        public void ExecuteSQL(string strSQL)
        {
            SqlCommand cmd = new SqlCommand(null, m_Connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();
        }
        // 执行事务，传入sql列表
        public void ExecuteNonQueryTransSql(List<String> lstSql)
        {
            SqlConnection conn = m_Connection;
            SqlTransaction sqlTran = null;
            SqlCommand cmd = new SqlCommand(); ;
            try
            {
                sqlTran = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 1800;
                cmd.Transaction = sqlTran;

                foreach (String sql in lstSql)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                sqlTran.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    if (sqlTran != null)
                    {
                        sqlTran.Rollback();
                    }
                }
                catch
                {
                }
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }
        //获得最后的ID
        public int GetLastID()
        {
            SqlCommand cmd = new SqlCommand(null, m_Connection);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select @@identity";
            return System.Convert.ToInt32(cmd.ExecuteScalar());
        }

        //判断数据库库中是否有相应的表
        public bool TableExists(string strTableName)
        {
            string sql = "select * from sysobjects where type='U' and name='" + strTableName + "'";
            SqlDataAdapter sqlda = new SqlDataAdapter(sql, m_Connection);
            DataSet ds = new DataSet();
            sqlda.Fill(ds);
            return (ds.Tables[0].Rows.Count != 0);
        }
        //得到表名称
        public List<string> GetTableName()
        {
            List<string> listTableName = new List<string>();

            DataTable dt = GetTableBySQL("select name from sysobjects where type='U'");
            foreach (DataRow row in dt.Rows)
            {
                listTableName.Add(row["name"].ToString());
            }

            return listTableName;
        }


        //判断字段是否存在
        public bool FieldExists(string strTableName, string strFieldName)
        {
            string sql = "select * from syscolumns where id=object_id('" + strTableName + "') and name='" + strFieldName + "'";
            SqlDataAdapter sqlda = new SqlDataAdapter(sql, m_Connection);
            //DataSet或许都是自定义的吧
            DataSet ds = new DataSet();
            sqlda.Fill(ds);
            return (ds.Tables[0].Rows.Count != 0);
        }
        // 关闭链接
        public void CloseConnection()
        {
            m_Connection.Close();
        }
        // 打开链接
        public void OpenConnection()
        {
            m_Connection.Open();
        }

        public System.Data.Common.DbCommand NewCommand(string strCommandText)
        {
            return new SqlCommand(strCommandText, m_Connection);
        }
        //获得表列
        public DataTable GetColumnTable(string strTableName)
        {
            string sql = "select * from syscolumns where id=object_id('" + strTableName + "') ";
            SqlDataAdapter sqlda = new SqlDataAdapter(sql, m_Connection);
            DataSet ds = new DataSet();
            sqlda.Fill(ds);
            return (ds.Tables[0]);
        }
    }
}
