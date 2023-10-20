using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bottleDetection.algothrim
{
  public enum e_DefectType
    {
        ERROR_LOCATEFAIL1111 = 1,               //定位失败
        ERROR_INVALID_ROI = 2,                  //预处理区域错误 
        ERROR_HORI_SIZE = 3,                    //横向尺寸错误
        ERROR_VERTSIZE = 4,                     //纵向尺寸错误
        ERROR_BENTNECK = 5,                     //歪脖错误
        ERROR_VERTANG = 6,                      //垂直度错误
        ERROR_SPOT = 7,                         //污点
        ERROR_CRACK = 8,                        //裂纹
        ERROR_BUBBLE = 9,                       //气泡
        ERROR_STONE = 10,                       //结石
        ERROR_DARKDOT = 11,                     //小黑点
        ERROR_SSIDEFIN = 12,                    //口部缺陷

        ERROR_OVERPRESS = 13,                   //双口 
        ERROR_SSCREWF = 14,                     //螺纹口裂纹
        ERROR_FINISHIN = 15,                     //口内沿缺陷
        ERROR_FINISHMID = 16,                     //口平面缺陷
        ERROR_LIGHT_SPOT = 17,                   //亮点
        ERROR_LOF = 18,                           //剪刀印
        ERROR_FINISHOUT = 19,                    //口外沿缺陷
        ERROR_BREACH = 20,                       //外环缺口
        ERROR_BOTTOM_DL = 21,                    //亮暗底
        ERROR_INNER_STRESS = 22,                //内应力错误
        ERROR_STONE_STRESS = 23,                //结石(应力)
        ERROR_FIN_CONT = 24,                    //瓶口轮廓缺陷
        ERROR_NECK_CONT = 25,                   //瓶脖轮廓缺陷
        ERROR_BODY_CONT = 26,                   //瓶身轮廓缺陷
        ERROR_MOUTHDEFORM = 27,                 //瓶口变形
        ERROR_BROKENRING = 28,                  //外环断环
        ERROR_BOTTOM_STRIPE = 29,               //瓶底条纹
        ERROR_BRI_SPOT = 30,                    //亮斑

        ERROR_CIRCLE_DIA = 31,                  //圆环外径错误
        ERROR_CIRCLE_OVALITY = 32,              //椭圆度错误
        ERROR_BASECONVEX_CONT = 33,             //瓶底轮廓缺陷
        ERROR_SMALL_STONE = 34,                 //小结石
        ERROR_TINY_CRACK = 35,                  //微裂纹
        ERROR_LOF_BL = 36,                      //背光剪刀印
        ERROR_SHALLOW_BUBBLE = 37,              //薄皮气泡
        ERROR_MODEID_IDENTIFY_FAIL = 38,        //模号识别失败
        ERROR_MODEID_REJECT = 39,               //模号设置剔除
        ERROR_MOULD_NUMBER = 40,                //模数识别失败
        ERROR_FINISH_IN_BUBBLE = 41,            //瓶口内壁气泡
        ERROR_SMALL_SPOT = 42,                  //小污点
        ERROR_LIGHT_STRIPE = 43,                // 亮条纹
        ERROR_CLOSING_BUBBLE = 44,              //闭合气泡
        ERROR_BAFFLE_MARK = 45,                 //闷头印错位
        ERROR_AiDetect = 46,                    //AI检测错误
        ERROR_BLACK = 47,                       //暗物质
        ERROR_FIN_BLACKELINE = 48,              //黑口线
        ERROR_FIN_WHITELINE = 49,               //白口线
        ERROR_FIN_BLACKDFECT = 50,              //暗缺陷
        ERROR_FIN_WHITEDFECT = 51               //亮缺陷
    }


    public struct s_detectBox
    {

    }
    
}
