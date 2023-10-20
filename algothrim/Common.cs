using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bottleDetect.ConfigInfo;
namespace bottleDetection.algothrim
{
	public struct s_pCreatModel
	{

		public int iNumLevel;      //****金字塔级数（该值越大匹配耗时越短）
								   //模板角度，一般根据对称性来进行设置，若模板图像不对称需设幅度为360度，若为正方形，可设幅度为90度，长方形为180度
		public float fPhiStart;    //起始角度（弧度，范围-3.14到3.14）
		public float fPhiExtent;   //角度幅度（弧度，范围0到6.28）
		public float fPhiStep;     //角度步长
								   //**优化算法参数,第一个参数，依次更优化，点数多时可使用.'none'1, 'point_reduction_low'1/2, 'point_reduction_medium'1/3, 'point_reduction_high'1/4）
								   //*优化算法参数,第二个参数，'pregeneration', 'no_pregeneration' ，预分配内存对速度影响不大，反而耗费内存，不推荐使用
		public String strOptim;   //**优化算法（降低边缘点数可提速,依次更优化.)
								  //极性，'use_polarity'，使用该参数即可，一般模板与目标极性相同
		public int iEdge;          //对比度，用于提取模板，有三种参数模式[min,max,size],目前只用一个参数
		public int iMinEdge;       //最小对比度，排除无效边缘干扰
	}

	public struct s_pFindModel
	{
		public double fPhiStart;    //**起始角度（弧度，范围-3.14到3.14）
		public double fPhiExtent;   //**角度幅度（弧度，范围0到6.28）
		public double fScore;       //***匹配度，该值越小匹配越耗时，范围[0，1]，推荐值（0.3-0.7）
		public int nNumMatchs; //匹配个数，0为所有匹配，指定个数，则选择匹配度最高的
		public double fMaxOverlap;  //允许的最大重叠面积，范围[0，1]，无格挡可设为0
									//**亚像素精度影响检测速度和匹配结果（*角度*），若精度要求不高，可使用'interpolation'，综合精度和速度可使用'least_squares'
		public String strSubPixel;//**亚像素精度。
		public double fGreediness;  //****用于定位加速，该值越大速度越快，但可能导致无法找到匹配目标，推荐（0.7-0.9）

		public s_pFindModel(double fphiStart = -3.14, double fphiExtent = 6.29, double fscore = 0.3, int nnumMatchs = 0, double fmaxOverlap = 0, String strsubPixel = "least_squares", double fgreediness = 0.7)
		{
			fPhiStart = fphiStart;
			fPhiExtent = fphiExtent;
			fScore = fscore;
			nNumMatchs = nnumMatchs;
			fMaxOverlap = fmaxOverlap;
			strSubPixel = strsubPixel;
			fGreediness = fgreediness;

		}
	}


	public struct s_dectectBox
    {

    }
	//在这里实现1.匹配2.线定位
	public class Common
    {

		
    }
}
