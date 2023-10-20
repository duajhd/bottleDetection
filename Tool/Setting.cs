using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace bottleDetection.Tool
{
  public  class Setting
    {
        //这个类具有的功能1.读取ini某个section下的key2.能写入某个字段下的key3.判断某个字段有无4.判断某个字段下有无某个key
        
    

        private String s_sectionName = "";
        private String s_filePath;

        private List<String> Sections;
        private List<String> Keys;
 
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(String section, String key, String val, String filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(String section, String key, String def, System.Text.StringBuilder retVal, int size, String filePath);

        //LPCTSTR对应String;   DWORD对应int ;LPTSTR对应System.Text.StringBuilder
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long GetPrivateProfileSectionNames(System.Text.StringBuilder retVal, int nSize,  String filePath);


        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern uint GetPrivateProfileStringA(string section, string key,
           string def, Byte[] retVal, int size, string filePath);
        public Setting(String filePath)//这个函数里必须判断文件是否存在并且是否是ini文件
        {
            s_filePath = filePath;
            Sections = new List<String>();
            Keys = new List<String>();
            try
            {
                if (File.Exists(s_filePath))
                {
                    ReadSections(s_filePath);
                }
               
            }
            catch
            {
                //使用日志函数输出
                MessageBox.Show("读配置文件错误，请检查配置是否存在以及格式是否正确");
                return;
            }
            

          

            
        }

        ~Setting()
        {

        }

        /// 写入INI的方法
        public void INIWrite(String section, String key, String value, String path)
        {
            // section=配置节点名称，key=键名，value=返回键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }

        //读取INI的方法
        public String INIRead(String section, String key, String path)
        {
            // 每次从ini中读取多少字节
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp.ToString();

        }
        //必须判断有无字段
        public void setValue(String key , String value)
        {
            WritePrivateProfileString(s_sectionName, key, value, s_filePath);
            updateSection();


        }
        private void updateSection()
        {
            if (s_filePath != "")
            {
                ReadSections(s_filePath);
            }
            
        }
        public void beginGroup(String sectionName)
        {
            if (s_sectionName == "")
            {
                s_sectionName = sectionName;
            }
            else
            {
               // throw;         //可以在这里抛出异常
            }
        }
        public void endGroup()
        {
            s_sectionName = "";
        }
        //只保留字符串value就够了
        public void value(String key, String defaultValue, out String param)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(s_sectionName, key, "", temp, 255, s_filePath);
            param = temp.ToString();

        }

        public void value(String key, int defaultValue, out int param)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(s_sectionName, key, "", temp, 255, s_filePath);
            if(temp.ToString() != "")
            {
                param = Convert.ToInt32(temp.ToString());           //要先转换成String
            }
            else
            {
                param = defaultValue;
            }
          

        }
        //只能获取第一个字段，why?
        public String getALLSection()
        {

            System.Text.StringBuilder temp = new System.Text.StringBuilder(256);
            GetPrivateProfileSectionNames(temp,256, s_filePath);




             return   temp.ToString();
        }

        public  void ReadSections(string iniFilename)
        {
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(null, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    Sections.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
        }

        public bool isSectionExist(String section)   //在段集合中查询某个section是否存在，存在返回true；不存在返回false
        {
            int i;
            for (i = 0; i < Sections.Count; i++)
            {
                if(Sections[i] == section)
                {
                    break;
                }
            }

            return i == Sections.Count ? false : true;
        }
        //一次性读出字段下所有的的key
        public void ReadKeys(string SectionName, string iniFilename)
        {
           
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(SectionName, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    Keys.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
           
        }










        public void value(String key, bool defaultValue, out bool param)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(s_sectionName, key, "", temp, 255, s_filePath);

           // param = (int)(temp.ToString());
            param = Convert.ToBoolean(temp.ToString());

        }

       


        public void value(String key, double defaultValue, out double param)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(s_sectionName, key, "", temp, 255, s_filePath);
            //param = (int)(temp.ToString());
            param = Convert.ToDouble(temp.ToString());







            

        }

    }
}
