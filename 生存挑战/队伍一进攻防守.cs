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
            return "南京大学金陵学院";
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

            #region 时间
            float runtime = (float)0.1 * (3000 - mission.CommonPara.RemainingCycles);
            #endregion

            #region 判断鱼是否被吃掉

            int by2 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish2Caught"]);
            int by3 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish3Caught"]);
            int by4 = Convert.ToInt32(mission.HtMissionVariables["IsYellowFish4Caught"]);
            int br2 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish2Caught"]);
            int br3 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish3Caught"]);
            int br4 = Convert.ToInt32(mission.HtMissionVariables["IsRedFish4Caught"]);
            #endregion

            #region 队伍一鱼的参数
            xna.Vector3 T1fish1_body = mission.TeamsRef[0].Fishes[0].PositionMm;
            float T1fish1_bodyX = T1fish1_body.X;
            float T1fish1_bodyZ = T1fish1_body.Z;//队伍一一号鱼的身体坐标

            xna.Vector3 T1fish1_head = mission.TeamsRef[0].Fishes[0].PolygonVertices[0];
            float T1fish1_headX = T1fish1_head.X;
            float T1fish1_headZ = T1fish1_head.Z;//队伍一一号鱼的头坐标

            xna.Vector3 T1fish2_body = mission.TeamsRef[0].Fishes[1].PositionMm;
            float T1fish2_bodyX = T1fish2_body.X;
            float T1fish2_bodyZ = T1fish2_body.Z;//队伍一二号鱼的身体坐标


            xna.Vector3 T1fish2_head = mission.TeamsRef[1].Fishes[1].PolygonVertices[0];
            float T1fish2_headX = T1fish2_head.X;
            float T1fish2_headZ = T1fish2_head.Z;//队伍一二号鱼的头坐标

            xna.Vector3 T1fish3_body = mission.TeamsRef[1].Fishes[2].PositionMm;
            float T1fish3_bodyX = T1fish3_body.X;
            float T1fish3_bodyZ = T1fish3_body.Z;//队伍一三号鱼的身体坐标

            xna.Vector3 T1fish3_head = mission.TeamsRef[1].Fishes[2].PolygonVertices[0];
            float T1fish3_headX = T1fish3_head.X;
            float T1fish3_headZ = T1fish3_head.Z;//队伍二三号鱼的头坐标

            xna.Vector3 T1fish4_body = mission.TeamsRef[1].Fishes[3].PositionMm;
            float T1fish4_bodyX = T1fish4_body.X;
            float T1fish4_bodyZ = T1fish4_body.Z;//队伍一四号鱼的身体坐标

            xna.Vector3 T1fish4_head = mission.TeamsRef[1].Fishes[3].PolygonVertices[0];
            float T1fish4_headX = T1fish4_head.X;
            float T1fish4_headZ = T1fish4_head.Z;//队伍一四号鱼的头坐标

            float T1fish1Rad = FishDecision.FishDecision.RedToDegree(mission.TeamsRef[0].Fishes[0].BodyDirectionRad);//队伍一一号鱼的鱼体方向
            #endregion

            #region 队伍二鱼的参数

            xna.Vector3 T2fish1_body = mission.TeamsRef[1].Fishes[0].PositionMm;
            float T2fish1_bodyX = T2fish1_body.X;
            float T2fish1_bodyZ = T2fish1_body.Z;//队伍二一号鱼的身体坐标

            xna.Vector3 T2fish1_head = mission.TeamsRef[1].Fishes[0].PolygonVertices[0];
            float T2fish1_headX = T2fish1_head.X;
            float T2fish1_headZ = T2fish1_head.Z;//队伍二一号鱼的头坐标

            xna.Vector3 T2fish2_body = mission.TeamsRef[1].Fishes[1].PositionMm;
            float T2fish2_bodyX = T2fish2_body.X;
            float T2fish2_bodyZ = T2fish2_body.Z;//队伍二二号鱼的身体坐标

            xna.Vector3 T2fish2_head = mission.TeamsRef[1].Fishes[1].PolygonVertices[0];
            float T2fish2_headX = T2fish2_head.X;
            float T2fish2_headZ = T2fish2_head.Z;//队伍二二号鱼的头坐标

            xna.Vector3 T2fish3_body = mission.TeamsRef[1].Fishes[2].PositionMm;
            float T2fish3_bodyX = T2fish3_body.X;
            float T2fish3_bodyZ = T2fish3_body.Z;//队伍二三号鱼的身体坐标

            xna.Vector3 T2fish3_head = mission.TeamsRef[1].Fishes[2].PolygonVertices[0];
            float T2fish3_headX = T2fish3_head.X;
            float T2fish3_headZ = T2fish3_head.Z;//队伍二三号鱼的头坐标

            xna.Vector3 T2fish4_body = mission.TeamsRef[1].Fishes[3].PositionMm;
            float T2fish4_bodyX = T2fish4_body.X;
            float T2fish4_bodyZ = T2fish4_body.Z;//队伍二四号鱼的身体坐标

            xna.Vector3 T2fish4_head = mission.TeamsRef[1].Fishes[3].PolygonVertices[0];
            float T2fish4_headX = T2fish4_head.X;
            float T2fish4_headZ = T2fish4_head.Z;//队伍二四号鱼的头坐标

            float T2fish1Rad = mission.TeamsRef[1].Fishes[0].BodyDirectionRad;//队伍二一号鱼的鱼体方向
            float T2fish2Rad = mission.TeamsRef[1].Fishes[1].BodyDirectionRad;//队伍二二号鱼的鱼体方向
            float T2fish3Rad = mission.TeamsRef[1].Fishes[2].BodyDirectionRad;//队伍二三号鱼的鱼体方向
            float T2fish4Rad = mission.TeamsRef[1].Fishes[3].BodyDirectionRad;//队伍二四号鱼的鱼体方向

            #endregion

            #region 搏命战术所需坐标

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
            Vector3 T2Fish_1 = FishDecision.FishDecision.SetDestPtMm(battle3, T2fish2_body, -135, 100);
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

            #endregion

            #region 搏命战术所需参数

            #region 假想点
            Vector3 goal_4 = new Vector3(0, 0, -1572);
            #endregion

            #region 预设顶点
            Vector3 destPtMm1 = FishDecision.FishDecision.SetDestPtMm(goal_4, T2fish1_body, 30, 30);
            Vector3 destPtMm2 = FishDecision.FishDecision.SetDestPtMm(goal_4, T2fish2_body, 30, 30);
            Vector3 destPtMm3 = FishDecision.FishDecision.SetDestPtMm(goal_4, T2fish3_body, 30, 30);
            #endregion

            #endregion

            #region 点坐标
            Vector3 goal1 = new Vector3(-2200f, 0f, -1500f);  //左上角的点
            Vector3 goal2 = new Vector3(-2200f, 0f, 0f);     //左中的点
            Vector3 goal3 = new Vector3(-2200f, 0f, 1500f); //左下的点
            Vector3 goal4 = new Vector3(2200f, 0f, -1500f); //右上的点
            Vector3 goal5 = new Vector3(2200f, 0f, 0f);     //右中的点
            Vector3 goal6 = new Vector3(2200f, 0f, 1500f);  //右下的点

            Vector3 goal7 = new Vector3(200f, 0f, -900f);
            Vector3 goal8 = new Vector3(-200f, 0f, -500f);
            Vector3 goal9 = new Vector3(200f, 0f, 200f);
            Vector3 goal10 = new Vector3(-200f, 0f, -200f);
            Vector3 goal11 = new Vector3(-200f, 0f, 500f);
            Vector3 goal12 = new Vector3(200f, 0f, 900f);
            Vector3 goal13 = new Vector3(-200f, 0f, -900f);
            Vector3 goal14 = new Vector3(200f, 0f, -500f);
            Vector3 goal15 = new Vector3(200f, 0f, -200f);
            Vector3 goal16 = new Vector3(-200f, 0f, 200f);
            Vector3 goal17 = new Vector3(200f, 0f, 500f);
            Vector3 goal18 = new Vector3(-200f, 0f, 900f);
            Vector3 goal19 = new Vector3(0f, 0f, -900f);
            Vector3 goal20 = new Vector3(-200f, 0f, -700f);
            Vector3 goal21 = new Vector3(200f, 0f, -700f);
            Vector3 goal22 = new Vector3(0f, 0f, -500f);
            Vector3 goal23 = new Vector3(0f, 0f, -200f);
            Vector3 goal24 = new Vector3(-200f, 0f, 0f);
            Vector3 goal25 = new Vector3(200f, 0f, 0f);
            Vector3 goal26 = new Vector3(0f, 0f, 200f);
            Vector3 goal27 = new Vector3(0f, 0f, 500f);
            Vector3 goal28 = new Vector3(-200f, 0f, 700f);
            Vector3 goal29 = new Vector3(200f, 0f, 700f);
            Vector3 goal30 = new Vector3(0f, 0f, 900f);
            Vector3 goal31 = new Vector3(-1200f, 0f, 0f);
            Vector3 goal32 = new Vector3(1500f, 0f, -1100f);
            Vector3 goal33 = new Vector3(1500f, 0f, 1000f);
            Vector3 goal34 = new Vector3(-1700f, 0f, 1200f);
            Vector3 goal35 = new Vector3(-1685f, 0f, 1185f);
            Vector3 goal36 = new Vector3(-1715f, 0f, 1215f);

            Vector3 goal37 = new Vector3(-100, 0f, -200);
            Vector3 goal38 = new Vector3(100, 0f, -200);
            Vector3 goal39 = new Vector3(-200, 0f, -100);
            Vector3 goal40 = new Vector3(200, 0f, -100);
            Vector3 goal41 = new Vector3(-200, 0f, 100);
            Vector3 goal42 = new Vector3(200, 0f, 100);
            Vector3 goal43 = new Vector3(-100, 0f, 200);
            Vector3 goal44 = new Vector3(100, 0f, 200);
            Vector3 goal45 = new Vector3(200, 0f, -200);
            Vector3 goal46 = new Vector3(-200, 0f, 200);

            Vector3 goal47 = new Vector3(1300, 0f, 900);
            Vector3 goal48 = new Vector3(1300, 0f, -900);


            #endregion

            #region 距离
            double L1 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body);
            double L2 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body);
            double L3 = FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body);
            #endregion

            Vector3 T2Fish2 = FishDecision.FishDecision.SetDestPtMm(T2fish1_head, T2fish2_body, -135, 200);

            #region 
            //if(FishDecision.FishDecision.judgearea(T1fish1_head,1)==true|| FishDecision.FishDecision.judgearea(T1fish1_head, 3) == true|| FishDecision.FishDecision.judgearea(T1fish1_head, 5) == true)
            //    FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));

            //if (FishDecision.FishDecision.judgearea(T1fish1_head, 7) == true)
            //    FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal17, (int)(Math.PI / 2));

            //if (FishDecision.FishDecision.judgearea(T1fish1_head, 8) == true)
            //    FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal11, (int)(Math.PI / 2));



            //if (FishDecision.FishDecision.judgearea(T1fish1_head, 2) == true|| FishDecision.FishDecision.judgearea(T1fish1_head, 4) == true|| FishDecision.FishDecision.judgearea(T1fish1_head, 6) == true)
            //    FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
            #endregion

            #region 
            /*
            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)Math.PI / 2);
            //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 60, 150, 180, 200, 15, 15, 15, 10, 100, true);
            //FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));
              if ((by3==1||by4==1)&&by2==0)
                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)Math.PI / 2);
           // FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 90, 60, 30, 200, 15, 15, 15, 10, 100, true);
            //FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI/2));
            if (by2==1&&by3==0)
                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)Math.PI / 2);
            //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 90, 60, 30, 200, 15, 15, 15, 10, 100, true);
            //FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));
              if(by2==1&&by3==1)
                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)Math.PI / 2);
            //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 90, 60, 30, 200, 15, 15, 15, 10, 100, true);
           // FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));
             */
            #endregion

            #region 
            /*
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));
                            if ((br3 == 1 || br4 == 1) && br2 == 0)
                                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));
                            if (br2 == 1 && br3 == 0)
                                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));
                            if (br2 == 1 && br3 == 1)
                                FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));
                           */

            #endregion
            int remainTime = mission.CommonPara.RemainingCycles;

            if (remainTime <= 3000)
            {
                #region 防守方防御手

                if (br2 == 0 && br3 == 0 && br4 == 0 && L2 < L1 && L2 < L3)
                {
                    FishDecision.FishDecision.move2(mission, ref decisions[0], 1, 0, goal35, 180);
                }

                //if (L1 < 250 || L2 < 250 || L3 < 250) {
                //    if (T1fish1Rad > 0 && T1fish1Rad < 90)

                //        FishDecision.FishDecision.Dribble3(ref decisions[0], mission.TeamsRef[1].Fishes[0], new Vector3(T1fish1_headX + 25, 0, T1fish1_headZ + 25), 120, 60, 30, 200, 14, 12, 10, 15, 100, false);

                //    if (T1fish1Rad > 90 && T1fish1Rad <= 180)
                //        FishDecision.FishDecision.Dribble3(ref decisions[0], mission.TeamsRef[1].Fishes[0], new Vector3(T1fish1_headX + 25, 0, T1fish1_headZ - 25), 120, 60, 30, 200, 14, 12, 10, 15, 100, false);

                //    if (T1fish1Rad > -180 && T1fish1Rad <= -90)
                //        FishDecision.FishDecision.Dribble3(ref decisions[0], mission.TeamsRef[1].Fishes[0], new Vector3(T1fish1_headX - 25, 0, T1fish1_headZ - 25), 120, 60, 30, 200, 14, 12, 10, 15, 100, false);

                //    if (T1fish1Rad > -90 && T1fish1Rad <= 0)
                //        FishDecision.FishDecision.Dribble3(ref decisions[0], mission.TeamsRef[1].Fishes[0], new Vector3(T1fish1_headX - 25, 0, T1fish1_headZ + 25), 120, 60, 30, 200, 14, 12, 10, 15, 100, false);
                //}
                //else
               else  FishDecision.FishDecision.move2(mission, ref decisions[0], 1, 0, T1fish1_body, 0);

                #endregion


                #region 防守方躲避手



                #region 二号鱼
                FishDecision.FishDecision.Move(mission, ref decisions[1], 1, 1, goal48, 0);
                //if (FishDecision.FishDecision.judgearea3(T1fish1_head, 6) == true || FishDecision.FishDecision.judgearea3(T1fish1_head, 8) == true)
                if (FishDecision.FishDecision.judgearea4(T1fish1_head, 2) == true || FishDecision.FishDecision.judgearea4(T1fish1_head, 4) == true)
                    FishDecision.FishDecision.Turn(mission, ref decisions[1], 1, 1, mission.TeamsRef[0].Fishes[0].PositionMm);
                #endregion

                
                //  FishDecision.FishDecision.Move(mission, ref decisions[3], 1, 3, goal34, 180);
                //if (FishDecision.FishDecision.judgearea3(T1fish1_head, 2) == true || FishDecision.FishDecision.judgearea3(T1fish1_head, 4) == true)

                #region  三号鱼
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 1) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal9, 135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 2) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal44, 135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 3) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal43, 135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 4) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal46, -135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 5) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal42, 45);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 6) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal41, -135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 7) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal40,45);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 8) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal39, -135);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 9) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal45, 45);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 10) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal38, -45);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 11) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal37,-45);

                }
                if (FishDecision.FishDecision.Judgearea(T1fish1_head, 12) == true)
                {
                    FishDecision.FishDecision.Move(mission, ref decisions[2], 1, 2, goal10, -45);

                }

                //if (FishDecision.FishDecision.judgearea4(T1fish1_head, 2) == true || FishDecision.FishDecision.judgearea4(T1fish1_head, 4) == true)
                //    FishDecision.FishDecision.Turn(mission, ref decisions[2], 1, 2, mission.TeamsRef[0].Fishes[0].PositionMm);
                #endregion



                #region
                /*
                                   if (FishDecision.FishDecision.judgearea(T1fish1_head, 1) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal14, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 2) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal8, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal16, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }
                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 3) == true)

                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }


                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 4) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal16, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 5) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 6) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 7) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal17, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 8) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal11, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 9) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal14, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal16, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 10) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 11) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal16, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 12) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 13) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 14) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 15) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 16) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal17, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 17) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                                    }

                                    if (FishDecision.FishDecision.judgearea(T1fish1_head, 18) == true)
                                    {
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                                        FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                                    }
                                  */

                //---------------------------------------------------------------------------
                /* if (FishDecision.FishDecision.judgearea3(T1fish1_head, 1) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal14, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                 }

                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 2) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal8, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                 }
                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 3) == true||FishDecision.FishDecision.judgearea3(T1fish1_head,9)==true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal9, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                 }
                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 4) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal16, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                 }

                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 5) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal12, (int)(Math.PI / 2));
                 }
                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 6) == true || FishDecision.FishDecision.judgearea3(T1fish1_head, 10) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal18, (int)(Math.PI / 2));
                 }
                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 7) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal7, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal15, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal17, (int)(Math.PI / 2));
                 }

                 if (FishDecision.FishDecision.judgearea3(T1fish1_head, 8) == true)
                 {
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[1], 1, 1, goal13, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[2], 1, 2, goal10, (int)(Math.PI / 2));
                     FishDecision.FishDecision.MoveToAreaS(mission, ref decisions[3], 1, 3, goal11, (int)(Math.PI / 2));
                 }
                 */
                #endregion

                #region 四号鱼

                FishDecision.FishDecision.Move(mission, ref decisions[3], 1, 3, goal47, 0);

                if ((FishDecision.FishDecision.judgearea4(T1fish1_head, 6) == true || FishDecision.FishDecision.judgearea4(T1fish1_head, 8) == true) )
                    FishDecision.FishDecision.Turn(mission, ref decisions[3], 1, 3, mission.TeamsRef[0].Fishes[0].PositionMm);

                #endregion

                #endregion

            }

            if (remainTime >= 3000)
            {
                #region 0-3分钟
                if (runtime >= 0 && runtime <= 180)
                {
                    #region 进攻方追捕手
                    //if (L1 < 600 || L2 < 600 || L3 < 600)
                    //{
                    //    if (fishtodestrad > -60 && fishtodestrad < 60)
                    //    {
                    //        decisions[0].VCode = 14;
                    //    }
                    //    else {
                    //        decisions[0].VCode = 0;
                    //        decisions[0].TCode = 7;
                    //    }

                    //}
                    ///*{ FishDecision.FishDecision.Turn2(mission, ref decisions[0], 0, 0, T2fish2_body); }*/
                    //else FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));

                    // FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2Fish2, (int)(Math.PI / 2));
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish2, 50, 80, 200, 200, 15, 10, 15, 15, 100, true);





                    if (by2 == 0 && by3 == 0 && by4 == 0)
                    {
                        if (L1 < L2 && L1 < L3)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2Fish2, (int)(Math.PI / 2));
                        //FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));
                        //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 90, 60, 30, 0, 15, 15, 15, 15, 100, true);
                        if (L2 < L1 && L2 < L3)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));
                        //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 90, 60, 30, 0, 15, 15, 15, 15, 100, true);
                        if (L3 < L1 && L3 < L2)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));

                    }

                    if (by2 == 1 && by3 == 0 && by4 == 0)
                    {

                        if (L2 < L3)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));
                        if (L3 < L2)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));
                    }


                    if (by2 == 0 & by3 == 1 && by4 == 0)
                    {
                        if (L1 < L3)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2Fish2, (int)(Math.PI / 2));
                        //FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));
                        if (L3 < L1)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));

                    }

                    if (by2 == 0 && by3 == 0 && by4 == 1)
                    {
                        if (L1 < L2)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2Fish2, (int)(Math.PI / 2));
                        // FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));

                        if (L2 < L1)
                            FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));

                    }

                    if (by2 == 1 && by3 == 1 & by4 == 0)
                        FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish4_body, (int)(Math.PI / 2));
                    // FishDecision.FishDecision.Turn2(mission, ref decisions[0], 0, 0, T2fish4_body);
                    if (by2 == 1 && by3 == 0 && by4 == 1)
                        FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish3_body, (int)(Math.PI / 2));
                    if (by2 == 0 && by3 == 1 && by4 == 1)
                        FishDecision.FishDecision.move2(mission, ref decisions[0], 0, 0, T2fish2_body, (int)(Math.PI / 2));


                    #region 


                    #endregion




                    #endregion

                }

                #endregion

                #region 3-4分钟
                if (runtime > 180 && runtime <= 240)
                {
                    #region 搏命战术
                    if (by2 == 0 && by3 == 0 && by4 == 0)
                    {//000
                        if (DisTC1 < DisTC2 && DisTC1 < DisTC3)
                        {//距离一号最近
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish2_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                        if (DisTC2 < DisTC1 && DisTC2 < DisTC3)
                        {//距离二号最近
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                        if (DisTC3 < DisTC1 && DisTC3 < DisTC2)
                        {//距离三号最近
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 100, 15, 10, 9, 5, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                    }
                    //Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 90, 200, 15, 10, 9, 5, 100, true);
                    if (by2 == 1 && by3 == 0 && by4 == 0)
                    {//100
                        if (DisTC2 < DisTC3)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish3_body, 50, 80, 90, 100, 15, 15, 15, 5, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                        if (DisTC3 < DisTC2)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 90, 100, 15, 15, 15, 5, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                    }
                    //Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
                    if (by2 == 1 && by3 == 1 && by4 == 0)
                    {//110
                        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2fish4_body, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
                        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
                        {
                            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                        }
                    }
                    if (by2 == 0 && by3 == 1 && by4 == 0)
                    {//010
                        if (DisTC3 < DisTC1)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 9, 15, 200, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_3_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                        if (DisTC1 < DisTC3)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 200, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }

                    }
                    if (by2 == 0 && by3 == 0 && by4 == 1)
                    {//001
                        if (DisTC1 < DisTC2)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish2_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }
                        if (DisTC2 > DisTC1)
                        {
                            Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
                            if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
                            {
                                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                            }
                        }

                    }
                    if (by2 == 0 && by3 == 1 && by3 == 1)
                    {//011
                        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1, 50, 80, 200, 200, 15, 10, 9, 15, 100, true);
                        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish3_body) < 300)
                        {
                            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_1_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                        }
                    }
                    if (by2 == 1 && by3 == 0 && by4 == 1)
                    {//111
                        Reangle.Dribble_666(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2, 50, 80, 200, 200, 15, 10, 10, 9, 100, true);
                        if (FishDecision.FishDecision.GetDistBetwGB(T1fish1_body, T2fish4_body) < 300)
                        {
                            FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], T2Fish_2_1, 50, 80, 200, 100, 15, 10, 9, 15, 100, true);
                        }
                    }
                    #endregion

                }
                #endregion

                #region 4-5分钟
                if (runtime > 240 && runtime <= 300)
                {
                    if (by2 == 0 && by3 == 0 && by4 == 0)
                    {
                        if (L1 < L2 && L1 < L3)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm1, 120, 60, 0, 0, 15, 15, 15, 15, 100, true);
                        }
                        if (L2 < L1 && L2 < L3)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm2, 120, 60, 0, 0, 15, 15, 5, 15, 100, true);
                        }
                        if (L3 < L1 && L3 < L2)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm3, 120, 0, 180, 0, 15, 10, 8, 15, 100, true);
                        }
                    }
                    if (by2 == 1 && by3 == 0 && by4 == 0)
                    {
                        if (L2 < L3)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm2, 120, 60, 0, 0, 15, 15, 5, 15, 100, true);
                        }
                        if (L3 < L2)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm3, 120, 0, 180, 0, 15, 10, 8, 15, 100, true);
                        }
                    }
                    if (by2 == 0 && by3 == 1 && by4 == 0)
                    {
                        if (L1 < L3)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm1, 120, 60, 0, 0, 15, 15, 15, 15, 15, true);
                        }
                        if (L3 > L1)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm3, 120, 0, 180, 0, 15, 10, 8, 15, 100, true);
                        }
                    }
                    if (by2 == 0 && by3 == 0 && by4 == 1)
                    {
                        if (L1 < L2)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm1, 120, 60, 0, 0, 15, 15, 15, 15, 100, true);
                        }
                        if (L2 < L1)
                        {
                            Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm2, 120, 60, 0, 0, 15, 15, 5, 15, 100, true);
                        }
                    }
                    if (by2 == 1 && by3 == 1 && by4 == 0)
                    {
                        Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm3, 120, 0, 180, 0, 15, 10, 8, 15, 100, true);
                    }
                    if (by2 == 1 && by3 == 0 && by4 == 1)
                    {
                        Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm2, 120, 60, 0, 0, 15, 15, 5, 15, 100, true);
                    }
                    if (by2 == 0 && by3 == 1 && by4 == 1)
                    {
                        Reangle.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], destPtMm1, 120, 60, 0, 0, 15, 15, 15, 15, 100, true);
                    }
                }
                #endregion
            }

            #endregion

            return decisions;
            }
        }
    }
