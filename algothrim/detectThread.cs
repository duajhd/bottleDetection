using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bottleDetect.ConfigInfo;
using System.IO;
//明确老版本有哪些成员函数

 //c#类目标功能：1.接受相机类作为参数2.线程可以从相机类的图像队列中弹出数据3.能检测结束信号停止线程
 //使用方法1.创建线程对象时
namespace bottleDetection.algothrim
{
   public class detectThread
    {

        
        public int ThreadNumber;                //线程号，这个号与相机号相等


        private bool m_bStopThread;


        private bool b_isShowImage;             //是否显示
        private int iErrorType;
        private int iMaxErrorType;
        private int iMaxErrorArea;
        realCameraInfo cameraInstanceNew;
        private Task threadTask;
        private s_imageData currenthCheckData;
        private bool m_bIsThreadDead = false;
        private FileStream fs = null;                                   //写入文件流
        private byte[] logText = { };
        private String stfFileName="";//写入数据
        private StreamWriter sw = null;
        //应该有一个算法类

        public detectThread( realCameraInfo cameraInstance)              //构造函数,传入相机实例
        {
            b_isShowImage = cameraInstance.b_isShowImage;

            cameraInstanceNew = cameraInstance;
            // threadTask = new Task(()=> { checkImage()});
        }
        public void  startThread()              //
        {
            Task.Run(()=> run());
        }

        public void endThread()
        {

        }

        public void checkImage( int iCheckMode)

        {
           
            switch (iCheckMode)
            {
                case (int)e_camType.normal:
                     
                    checkNormal(currenthCheckData);

                    break;

                case (int)e_camType.stress:

                    
                    checkStress(currenthCheckData);
                    break;

            }
        }

        public void run()                                               //线程启动函数
        {


            //文件名不能重复
          
            while (true)
            {

              
               

                if (cameraInstanceNew.q_imageDataQueue.Count <= 0)
                {

                    //可以添加日志类，记录缓存区为空
                    continue;
                }
                if (m_bIsThreadDead)                                                        //结束线程
                {
                    break;
                }

                lock(cameraInstanceNew.q_imageDataQueue)                                    //共享变量加锁，防止同时读写时争用条件
                {
                    currenthCheckData = cameraInstanceNew.q_imageDataQueue.Dequeue();
                    // logText = System.Text.Encoding.Default.GetBytes(stfFileName.ToString());
                    //  fs.Write(logText, 0, logText.Length);

                    
                }
                currenthCheckData.bitmap_ImageData.Dispose();

                checkImage(0);                          //检测
                
            }

            return;
            
        }

        public void  killThread()
        {
            m_bIsThreadDead = true;
            sw.Close();
            fs.Close();
        }
        private void readDetectModel()
        {

        }

        private void readModelParam()
        {

        }


        private void checkStress(s_imageData sample)
        {
            sample.bitmap_ImageData.Dispose();
            
            //没有Dispose接口
            return;

        }

        private void checkNormal(s_imageData sample)
        {
            return;
        }

        private void getCheckResult()
        {

        }

        //相机误触发，主要是谁的误触发?相机自己被触发但是采集卡并没有发出信号
        private void kickOutSmaple(int imageIndex)                //踢瓶
        {

        }

        
    }
}
