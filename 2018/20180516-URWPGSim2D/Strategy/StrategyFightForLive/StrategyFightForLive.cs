using Microsoft.Xna.Framework;
using System;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;
using xna = Microsoft.Xna.Framework;

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
            return "生存挑战 Test Team";
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
            float runtime = (float)0.1 * (3000 - mission.CommonPara.RemainingCycles);

            #region 判断鱼是否被吃掉

            int by2 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish2Caught"]);
            int by3 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish3Caught"]);
            int by4 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish4Caught"]);
            int br2 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish2Caught"]);
            int br3 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish3Caught"]);
            int br4 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish4Caught"]);
            #endregion

            #region team1
            //Vector3 T1fish1_body = mission.TeamsRef[0].Fishes[0].PositionMm;
            xna.Vector3 T1fish1_body = mission.TeamsRef[0].Fishes[0].PositionMm;
            #endregion

            #region team2鱼身
            //Vector3 T2fish2_body = mission.TeamsRef[1].Fishes[1].PositionMm;
            xna.Vector3 T2fish2_body = mission.TeamsRef[1].Fishes[1].PositionMm;
            //Vector3 T2fish3_body = mission.TeamsRef[1].Fishes[2].PositionMm;
            xna.Vector3 T2fish3_body = mission.TeamsRef[1].Fishes[2].PositionMm;
            //Vector3 T2fish4_body = mission.TeamsRef[1].Fishes[3].PositionMm;
            xna.Vector3 T2fish4_body = mission.TeamsRef[1].Fishes[3].PositionMm;
            #endregion

            #region team2
            Vector3 T2Fish1_1 = mission.TeamsRef[1].Fishes[1].PrePolygonVertices[0];
            Vector3 T2Fish2_1 = mission.TeamsRef[1].Fishes[2].PrePolygonVertices[0];
            Vector3 T2Fish3_1 = mission.TeamsRef[1].Fishes[3].PrePolygonVertices[0];
            #endregion

            #region battle
            Vector3 battle3 = new Vector3(0, 0, -700);
            Vector3 battle2 = new Vector3(0, 0, 0);
            Vector3 battle1 = new Vector3(0, 0, -700);
            #endregion

            #region assume
            Vector3 T2Fish_1 = FishDecision.FishDecision.SetDestPtMm(battle3,T2fish2_body,-135,100);
            Vector3 T2Fish_2 = FishDecision.FishDecision.SetDestPtMm(battle2, T2fish3_body, 150, 100);
            Vector3 T2Fish_3 = FishDecision.FishDecision.SetDestPtMm(battle1, T2fish4_body, 90, 100);
            #endregion

            #region assume1
            Vector3 T2Fish_1_1 = FishDecision.FishDecision.SetDestPtMm(T2Fish1_1, T2fish2_body, -135, 100);
            Vector3 T2Fish_2_1 = FishDecision.FishDecision.SetDestPtMm(T2Fish2_1, T2fish3_body, 150, 100);
            Vector3 T2Fish_3_1 = FishDecision.FishDecision.SetDestPtMm(T2Fish3_1, T2Fish_3, 90, 100);
            #endregion

            #region distance
            double DisTC1 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body);
            double DisTC2 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body);
            double DisTC3 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body);
            #endregion

            #region Distance
            double dis = FishDecision.FishDecision.GetDistBetwGB(new Vector3(0, 0, 0), T2fish3_body);
            #endregion

            #region 暂时
            ////FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 30, 40, 50, 100, 8, 6, 4, 5, 100, true);
            ////FishDecision.FishDecision.Dribble2(ref decisions[1], mission.TeamsRef[1].Fishes[1], T1fish1_body, 30, 40, 50, 100, 8, 6, 4, 5, 100, true);
            ////Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 50, 80, 90, 100, 15, 15, 15, 5, 100, true);
            ////Reangle.Dribble_666(ref decisions[1], mission.TeamsRef[1].Fishes[1], T1fish1_body, 30, 40, 50, 100, 8, 6, 4, 5, 100, true);
            //if (by2 == 0 && by3 == 0 && by4 == 0)
            //{//000
            //    if (DisTC1 < DisTC2 && DisTC1 < DisTC3)
            //    {//距离一号最近
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //    if (DisTC2 < DisTC1 && DisTC2 < DisTC3)
            //    {//距离二号最近
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //    if (DisTC3 < DisTC1 && DisTC3 < DisTC2)
            //    {//距离三号最近
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //}
            ////Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 90, 200, 15, 10, 9, 5, 100, true);
            //if (by2 == 1 && by3 == 0 && by4 == 0)
            //{//100
            //    if (DisTC2 < DisTC3)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 90, 100, 15, 15, 15, 5, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //    if (DisTC3 < DisTC2)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 90, 100, 15, 15, 15, 5, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //}
            ////Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
            //if (by2 == 1 && by3 == 1 && by4 == 0)
            //{//110
            //    Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
            //    if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
            //    {
            //        FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //    }
            //}
            //if (by2 == 0 && by3 == 1 && by4 == 0)
            //{//010
            //    if (DisTC3 < DisTC1)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 9, 15, 200, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //    if (DisTC1 < DisTC3)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 200, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }

            //}
            //if (by2 == 0 && by3 == 0 && by4 == 1)
            //{//001
            //    if (DisTC1 < DisTC2)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }
            //    if (DisTC2 > DisTC1)
            //    {
            //        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
            //        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
            //        {
            //            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //        }
            //    }

            //}
            //if (by2 == 0 && by3 == 1 && by3 == 1)
            //{//011
            //    Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
            //    if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
            //    {
            //        FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //    }
            //}
            //if (by2 == 1 && by3 == 0 && by4 == 1)
            //{
            //    Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 10, 9, 100, true);
            //    if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
            //    {
            //        FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
            //    }
            //}
            #endregion
            Vector3 goal_4 = new Vector3(0, 0, -1572);
            Vector3 destPtMm2 = FishDecision.FishDecision.SetDestPtMm(goal_4, T2fish3_body, 30, 30);

            #region 测试破柱
            int flag = 0;
            float record=0;
            float record_1 = 0;
            if (dis < 300)
            {
                /*
                 *记录时间
                 */
                if (by3==0&&dis<300&&flag==0)
                {
                    record = runtime;
                    record_1 = record;
                    record = record + 5;
                    flag = 1;
                }
                if (runtime>=record&&runtime<=record_1&&Reangle.Judge(T2fish3_body,1))
                {
                    FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[0], 0, 0, new Vector3(0, 0, -200), -30);
                }
                Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm2, 120, 60, 0, 0, 15, 15, 5, 15, 100, true);
            }

            #endregion

            #endregion

            return decisions;
        }
    }
}
