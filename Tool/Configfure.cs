using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bottleDetection.Tool
{
    class INIProcess
    {

        public INIProcess()
        {

        }
        ~INIProcess()
        {

        }



        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);


        /// 写入INI的方法
        public void writePara(string section, string key, string value, string path)
        {
            // section=配置节点名称，key=键名，value=返回键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }

        //读取INI的方法
        public string readPara(string section, string key, string path)
        {
            // 每次从ini中读取多少字节
            System.Text.StringBuilder bufferSize = new System.Text.StringBuilder(255);

            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", bufferSize, 255, path);
            return bufferSize.ToString();

        }

        //删除一个INI文件
       


    }
}
