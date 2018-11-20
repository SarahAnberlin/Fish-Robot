using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using xna = Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;

namespace URWPGSim2D.Strategy
{
    public class Strategy : MarshalByRefObject, IStrategy
    {
        #region reserved code never be changed or removed
        /// <summary>
        /// override the InitializeLifetimeService to return null instead of a valid ILease implementation
        /// to ensure this type of remote object never dies
        /// </summary>
        /// <returns>null</returns>
        public override object InitializeLifetimeService()
        {
            //return base.InitializeLifetimeService();
            return null; // makes the object live indefinitely
        }
        #endregion

        /// <summary>
        /// 决策类当前对象对应的仿真使命参与队伍的决策数组引用 第一次调用GetDecision时分配空间
        /// </summary>
        private Decision[] decisions = null;

        /// <summary>
        /// 获取队伍名称 在此处设置参赛队伍的名称
        /// </summary>
        /// <returns>队伍名称字符串</returns>
        public string GetTeamName()
        {
            return "庄鸿杰";
        }

        /// <summary>
        /// 获取当前仿真使命（比赛项目）当前队伍所有仿真机器鱼的决策数据构成的数组
        /// </summary>
        /// <param name="mission">服务端当前运行着的仿真使命Mission对象</param>
        /// <param name="teamId">当前队伍在服务端运行着的仿真使命中所处的编号 
        /// 用于作为索引访问Mission对象的TeamsRef队伍列表中代表当前队伍的元素</param>
        /// <returns>当前队伍所有仿真机器鱼的决策数据构成的Decision数组对象</returns>
        public Decision[] GetDecision(Mission mission, int teamId)
        {
            // 决策类当前对象第一次调用GetDecision时Decision数组引用为null
            if (decisions == null)
            {// 根据决策类当前对象对应的仿真使命参与队伍仿真机器鱼的数量分配决策数组空间
                decisions = new Decision[mission.CommonPara.FishCntPerTeam];
            }

            #region 决策计算过程 需要各参赛队伍实现的部分
            #region 策略编写帮助信息
            //====================我是华丽的分割线====================//
            //======================策略编写指南======================//
            //1.策略编写工作直接目标是给当前队伍决策数组decisions各元素填充决策值
            //2.决策数据类型包括两个int成员，VCode为速度档位值，TCode为转弯档位值
            //3.VCode取值范围0-14共15个整数值，每个整数对应一个速度值，速度值整体但非严格递增
            //有个别档位值对应的速度值低于比它小的档位值对应的速度值，速度值数据来源于实验
            //4.TCode取值范围0-14共15个整数值，每个整数对应一个角速度值
            //整数7对应直游，角速度值为0，整数6-0，8-14分别对应左转和右转，偏离7越远，角度速度值越大
            //5.任意两个速度/转弯档位之间切换，都需要若干个仿真周期，才能达到稳态速度/角速度值
            //目前运动学计算过程决定稳态速度/角速度值接近但小于目标档位对应的速度/角速度值
            //6.决策类Strategy的实例在加载完毕后一直存在于内存中，可以自定义私有成员变量保存必要信息
            //但需要注意的是，保存的信息在中途更换策略时将会丢失
            //====================我是华丽的分割线====================//
            //=======策略中可以使用的比赛环境信息和过程信息说明=======//
            //场地坐标系: 以毫米为单位，矩形场地中心为原点，向右为正X，向下为正Z
            //            负X轴顺时针转回负X轴角度范围为(-PI,PI)的坐标系，也称为世界坐标系
            //mission.CommonPara: 当前仿真使命公共参数
            //mission.CommonPara.FishCntPerTeam: 每支队伍仿真机器鱼数量
            //mission.CommonPara.MsPerCycle: 仿真周期毫秒数
            //mission.CommonPara.RemainingCycles: 当前剩余仿真周期数
            //mission.CommonPara.TeamCount: 当前仿真使命参与队伍数量
            //mission.CommonPara.TotalSeconds: 当前仿真使命运行时间秒数
            //mission.EnvRef.Balls: 
            //当前仿真使命涉及到的仿真水球列表，列表元素的成员意义参见URWPGSim2D.Common.Ball类定义中的注释
            //mission.EnvRef.FieldInfo: 
            //当前仿真使命涉及到的仿真场地，各成员意义参见URWPGSim2D.Common.Field类定义中的注释
            //mission.EnvRef.ObstaclesRect: 
            //当前仿真使命涉及到的方形障碍物列表，列表元素的成员意义参见URWPGSim2D.Common.RectangularObstacle类定义中的注释
            //mission.EnvRef.ObstaclesRound:
            //当前仿真使命涉及到的圆形障碍物列表，列表元素的成员意义参见URWPGSim2D.Common.RoundedObstacle类定义中的注释
            //mission.TeamsRef[teamId]:
            //决策类当前对象对应的仿真使命参与队伍（当前队伍）
            //mission.TeamsRef[teamId].Para:
            //当前队伍公共参数，各成员意义参见URWPGSim2D.Common.TeamCommonPara类定义中的注释
            //mission.TeamsRef[teamId].Fishes:
            //当前队伍仿真机器鱼列表，列表元素的成员意义参见URWPGSim2D.Common.RoboFish类定义中的注释
            //mission.TeamsRef[teamId].Fishes[i].PositionMm和PolygonVertices[0],BodyDirectionRad,VelocityMmPs,
            //                                   AngularVelocityRadPs,Tactic:
            //当前队伍第i条仿真机器鱼鱼体矩形中心和鱼头顶点在场地坐标系中的位置（用到X坐标和Z坐标），鱼体方向，速度值，
            //                                   角速度值，决策值
            //====================我是华丽的分割线====================//
            //========================典型循环========================//
            //for (int i = 0; i < mission.CommonPara.FishCntPerTeam; i++)
            //{
            //  decisions[i].VCode = 0; // 静止
            //  decisions[i].TCode = 7; // 直游
            //}
            //====================我是华丽的分割线====================//
            #endregion
            //请从这里开始编写代码

            #endregion
            #region 鱼身坐标
            Vector3 Trace = mission.TeamsRef[0].Fishes[0].PositionMm;
            float Trace_X = Trace.X;
            float Trace_Z = Trace.Z;

            #endregion
            #region 得分球坐标
            Vector3 Ball_1 = mission.EnvRef.Balls[0].PositionMm;//得分球①球心坐标
            Vector3 Ball_2 = mission.EnvRef.Balls[1].PositionMm;//得分球②球心坐标
            #endregion
            #region 障碍球坐标点
            Vector3 battle1 = new Vector3(1000f, 0f, 0f);//一号障碍物
            Vector3 battle2 = new Vector3(0f, 0f, 0f);//二号障碍物
            Vector3 battle3 = new Vector3(-1000f, 0f, 0f);//三号障碍物
            #endregion
            #region 上半区假定点
            Vector3 assume1 = new Vector3(1000f, 0f, 300f);     //假想点①
            Vector3 assume2 = new Vector3(0f, 0f, -317f);       //假想点②
            Vector3 assume3 = new Vector3(1000f, 0f, 250f);      //假想点③-障碍①下
            Vector3 assume_3 = new Vector3(600f, 0f, 250f);     //假想点_③-障碍①下
            Vector3 assume4 = new Vector3(300f, 0f, -250f);       //假想点④-障碍②上
            Vector3 assume_4 = new Vector3(-400f, 0f, 40f);   //假想点_④-障碍②下
            Vector3 assume5 = new Vector3(-1200f, 0f, 400f);    //假想点⑤-障碍③下
            Vector3 assume6 = new Vector3(-2050f, 0f, -1000f);   //假想点⑥-障碍③上
            #endregion
            #region 下半区假定点
            Vector3 assume_0 = new Vector3(1488f, 0f, -18f);    //假定重起点
            Vector3 assume_1 = new Vector3(1400f, 0f, -300f);    //假定点①-障碍①上
            Vector3 assume_2 = new Vector3(0f, 0f, 370f);       //假定点②-障碍②下
            Vector3 assume_2_ = new Vector3(0f, 0f, 500f);
            Vector3 assume__3 = new Vector3(-1180f, 0f, -400f);  //假定点③-障碍③上
            Vector3 assume__4 = new Vector3(-2050f, 0f, 990f);   //假定点④-障碍③下
            Vector3 assume_5 = new Vector3(-400f, 0f, -100f);
            Vector3 assume_6 = new Vector3(300f, 0f, 300f);
            Vector3 assume_7 = new Vector3(-1200f, 0f, -500f);
            Vector3 assume_8 = new Vector3(-2000f, 0f, 900f);
            Vector3 assume_9 = new Vector3(-500f, 0f, 1200f);
            Vector3 assume_10 = new Vector3(500f, 0f, 1200f);
            Vector3 assume_11 = new Vector3(1000f, 0f, 1200f);
            Vector3 assume_12 = new Vector3(1900f, 0f, 1200f);
            #endregion
            #region 洞点
            Vector3 hole = new Vector3(500, 0, -1224);
            Vector3 hole_1 = new Vector3(30, 0, 1200);
            #endregion
            #region 预设顶点
            Vector3 fakepoint = FishDecision.FishDecision.SetDestPtMm(hole, Ball_1, -45, 30);
            #endregion
            #region 得分点
            Vector3 keyPoint = new Vector3(1900f, 0f, -1200f);//得分点
            #endregion
            #region handpaper
            //if (FishDecision.FishDecision.judgearea4(Trace, 1) == false)
            //{

            //}
            //bool flag = false;
            //if (flag == false)
            //{
            //    FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, assume1, 135);
            //    if (FishDecision.FishDecision.judgearea4(Trace, 1) == true)
            //    {
            //        flag = true;
            //        if (flag == true)
            //        {
            //            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, assume2, -45);
            //        }
            //    }

            //}
            #endregion
            #region 判断是否得分
            int b0 = Convert.ToInt32(mission.HtMissionVariables["Ball0InHole"]);
            int b1 = Convert.ToInt32(mission.HtMissionVariables["Ball1InHole"]);
            #endregion
            #region 测试用点
            Vector3 Test1 = new Vector3(-1150f, 0f, -1200f);
            Vector3 Test2 = new Vector3(-800f, 0f, -1200f);
            Vector3 Test3 = new Vector3(-2050f, 0f, -1300f);
            #endregion
            #region test
            //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 30, 90, 7, 3, 4);

            #endregion
            #region 上半区
            if (b0 == 0)
            {
                if (FishDecision.FishDecision.judgearea5(Trace, 1) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {//前往假定点③
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume3, 10, 20, 30, 300, 10, 9, 2, 5, 100, false);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 2) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {//前往假定点④
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_3, 10, 20, 30, 3, 14, 7, 5, 5, 100, false);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 3) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {//前往假定点⑤
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume4, 10, 20, 30, 3, 14, 7, 14, 5, 100, false);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 4) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {//前往假定点⑥
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_4, 10, 20, 50, 3, 14, 7, 14, 5, 100, true);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 5) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume5, 10, 20, 50, 50, 14, 7, 7, 5, 100, true);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 6) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume6, 10, 20, 30, 100, 14, 5, 7, 5, 100, true);
                }
                if (FishDecision.FishDecision.judgearea5(Trace, 7) == true)
                {//前往洞点
                 /*提高预测点坐标*/
                    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 7, 2, 1);
                }
                if (FishDecision.FishDecision.judgearea5(Ball_1, 8) == true)
                {
                    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 7, 2, 1);
                }
                //if (FishDecision.FishDecision.judgearea(Trace, 13) == true)
                //{
                //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, Test1, 10, 20, 30, 7, 2, 1);
                //}
                if (FishDecision.FishDecision.judgearea5(Trace, 9) == true)
                {//前往①号得分点
                 /*缩小角度后半段冲刺*/
                    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 15, 45, 1, 1, 1);
                    //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0,0,  keyPoint, 30, 45, 60, 1, 1, 1);
                    //测试
                    //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], keyPoint, 30, 45, 60, 3, 100, 3, 2, 5, 100, true);
                }
                if (FishDecision.FishDecision.judgearea5(Ball_1, 8) == true)//&&FishDecision.FishDecision.judgearea5(Ball_1,8)==false
                {
                    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, keyPoint, 10, 15, 45, 7, 2, 1);
                    //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, keyPoint, 10, 30, 45, 7, 2, 1);
                }
            }
            #endregion
            #region 下半区
            if (FishDecision.FishDecision.judgearea5(Ball_1, 10) == true && FishDecision.FishDecision.judgearea5(Trace, 1) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_0, 10, 20, 30, 200, 14, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 14) == true && FishDecision.FishDecision.judgearea5(Trace, 1) == false)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_1, 10, 20, 30, 3, 14, 7, 7, 5, 100, true);
            }
            //if (FishDecision.FishDecision.judgearea5(Trace, 8) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == true)
            //{
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 11) == true )
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_1, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            //}
            if (FishDecision.FishDecision.judgearea5(Trace, 1) == true)
            {
                //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2_, 10, 20, 30, 7, 7, 7, 7, 5, 100, true);
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 7, 7, 7, 7, 5, 100, true);
            }
            //if (FishDecision.FishDecision.judgearea(Trace, 1) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            //}
            if (FishDecision.FishDecision.judgearea5(Trace, 2) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 3) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_6, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 4) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_5, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 5) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_7, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 6) == true)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_8, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            }
            if (FishDecision.FishDecision.judgearea5(Trace, 16) == true)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_9, 10, 15, 20, 5, 4, 2);
            }
            if (FishDecision.FishDecision.judgearea5(Ball_2, 17) == true)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_10, 10, 15, 20, 5, 4, 2);
            }
            if (FishDecision.FishDecision.judgearea5(Ball_2, 18) == true)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_11, 10, 15, 20, 5, 4, 2);
            }
            if (FishDecision.FishDecision.judgearea5(Ball_2, 19) == true)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_12, 10, 15, 20, 5, 4, 2);
            }
            #endregion
            /**/

            //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 3, 2, 1);


            return decisions;
        }
    }
}
