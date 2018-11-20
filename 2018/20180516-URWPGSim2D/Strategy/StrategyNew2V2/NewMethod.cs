using System;
using Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;
using URWPGSim2D;


namespace URWPGSim2D.Strategy
{
    class NewMethod
    {
        static Mission mission;
        static int teamId;
        void getElement(Mission a, int b) {
            mission = a;
            teamId = b;
        }

        static int MyTeam, YourTeam;

        static HalfCourt MyHalfCourt = mission.TeamsRef[teamId].Para.MyHalfCourt;

        static int RemainingTime = mission.CommonPara.RemainingCycles;
        #region 队伍识别

        void Teamid() {
            if (teamId % 2 == 0)
            {
                MyTeam = 0;
                YourTeam = 1;
            }
            else
            {
                MyTeam = 1;
                YourTeam = 0;
            }
        }
        #endregion

        #region 主要标识符

        //获取鱼角度
        //下标 0-零号鱼  1-一号鱼  2-对方一号鱼  3-对方二号鱼
        static float[] getFishRad() {
            float Fish00Rad = mission.TeamsRef[MyTeam].Fishes[0].BodyDirectionRad;
            float Fish01Rad = mission.TeamsRef[MyTeam].Fishes[1].BodyDirectionRad;
            float Fish10Rad = mission.TeamsRef[YourTeam].Fishes[0].BodyDirectionRad;
            float Fish11Rad = mission.TeamsRef[YourTeam].Fishes[1].BodyDirectionRad;
            float[] fishRad = {Fish00Rad,Fish01Rad,Fish10Rad,Fish11Rad };
            return fishRad;
        }


        //获取鱼的坐标
        //下标 0-零号鱼中心坐标  1-零号鱼鱼头坐标  2-一号鱼中心坐标  3-一号鱼鱼头坐标
        //     4-对方零号鱼中心坐标  5-对方零号鱼鱼头坐标  6-对方一号鱼中心坐标  7-对方一号鱼鱼头坐标
        static Vector3[] getFishPosition() {
            Vector3 MyFish0 = mission.TeamsRef[MyTeam].Fishes[0].PositionMm;
            Vector3 MyFish1 = mission.TeamsRef[MyTeam].Fishes[0].PositionMm;
            Vector3 MyFish0Head = mission.TeamsRef[MyTeam].Fishes[0].PolygonVertices[0];
            Vector3 MyFish1Head = mission.TeamsRef[MyTeam].Fishes[1].PolygonVertices[0];

            Vector3 YourFish0 = mission.TeamsRef[YourTeam].Fishes[0].PositionMm;
            Vector3 YourFish1 = mission.TeamsRef[YourTeam].Fishes[1].PositionMm;
            Vector3 YourFish0Head = mission.TeamsRef[YourTeam].Fishes[0].PolygonVertices[0];
            Vector3 YourFish1Head = mission.TeamsRef[YourTeam].Fishes[1].PolygonVertices[0];

            Vector3[] fishPosition = {MyFish0,MyFish0Head,MyFish1,MyFish1Head,YourFish0,YourFish0Head,YourFish1,YourFish1Head};
            return fishPosition;
        }

        //获取球坐标
        //下标 0-零号球 1-一号球 2-二号球 ......
        static Vector3[] getBallPosition() {
            Vector3[] ball = new Vector3[9];
            ball[0] = new Vector3(mission.EnvRef.Balls[0].PositionMm.X, 0, mission.EnvRef.Balls[0].PositionMm.Z);
            ball[1] = new Vector3(mission.EnvRef.Balls[1].PositionMm.X, 0, mission.EnvRef.Balls[1].PositionMm.Z);
            ball[2] = new Vector3(mission.EnvRef.Balls[2].PositionMm.X, 0, mission.EnvRef.Balls[2].PositionMm.Z);
            ball[3] = new Vector3(mission.EnvRef.Balls[3].PositionMm.X, 0, mission.EnvRef.Balls[3].PositionMm.Z);
            ball[4] = new Vector3(mission.EnvRef.Balls[4].PositionMm.X, 0, mission.EnvRef.Balls[4].PositionMm.Z);
            ball[5] = new Vector3(mission.EnvRef.Balls[5].PositionMm.X, 0, mission.EnvRef.Balls[5].PositionMm.Z);
            ball[6] = new Vector3(mission.EnvRef.Balls[6].PositionMm.X, 0, mission.EnvRef.Balls[6].PositionMm.Z);
            ball[7] = new Vector3(mission.EnvRef.Balls[7].PositionMm.X, 0, mission.EnvRef.Balls[7].PositionMm.Z);
            ball[8] = new Vector3(mission.EnvRef.Balls[8].PositionMm.X, 0, mission.EnvRef.Balls[7].PositionMm.Z);
            Vector3[] ballPosition = {ball[0],ball[1],ball[2],ball[3],ball[4],ball[5],ball[6],ball[7],ball[8] };
            return ballPosition;
        }

        //判断球是否进入左边球门 1与0表示
        //下标 0-零号球 1- 一号球 2-二号球 ......
        static int[] ballStatusInLeft() {
            int b0_l = Convert.ToInt32(mission.HtMissionVariables["Ball_0_Left_Status"]);
            int b1_l = Convert.ToInt32(mission.HtMissionVariables["Ball_1_Left_Status"]);
            int b2_l = Convert.ToInt32(mission.HtMissionVariables["Ball_2_Left_Status"]);
            int b3_l = Convert.ToInt32(mission.HtMissionVariables["Ball_3_Left_Status"]);
            int b4_l = Convert.ToInt32(mission.HtMissionVariables["Ball_4_Left_Status"]);
            int b5_l = Convert.ToInt32(mission.HtMissionVariables["Ball_5_Left_Status"]);
            int b6_l = Convert.ToInt32(mission.HtMissionVariables["Ball_6_Left_Status"]);
            int b7_l = Convert.ToInt32(mission.HtMissionVariables["Ball_7_Left_Status"]);
            int b8_l = Convert.ToInt32(mission.HtMissionVariables["Ball_8_Left_Status"]);

            int[] ballLeft = {b0_l,b1_l,b2_l,b3_l,b4_l,b5_l,b6_l,b7_l,b8_l };
            return ballLeft;

        }

        //判断球是否进入右边球门 1与0表示
        //下标 0-零号球 1- 一号球 2-二号球 ......
        static int[] ballStatusInRight(){
            int b0_r = Convert.ToInt32(mission.HtMissionVariables["Ball_0_Right_Status"]);
            int b1_r = Convert.ToInt32(mission.HtMissionVariables["Ball_1_Right_Status"]);
            int b2_r = Convert.ToInt32(mission.HtMissionVariables["Ball_2_Right_Status"]);
            int b3_r = Convert.ToInt32(mission.HtMissionVariables["Ball_3_Right_Status"]);
            int b4_r = Convert.ToInt32(mission.HtMissionVariables["Ball_4_Right_Status"]);
            int b5_r = Convert.ToInt32(mission.HtMissionVariables["Ball_5_Right_Status"]);
            int b6_r = Convert.ToInt32(mission.HtMissionVariables["Ball_6_Right_Status"]);
            int b7_r = Convert.ToInt32(mission.HtMissionVariables["Ball_7_Right_Status"]);
            int b8_r = Convert.ToInt32(mission.HtMissionVariables["Ball_8_Right_Status"]);

            int[] ballRight = {b0_r,b1_r,b2_r,b3_r,b4_r,b5_r,b6_r,b7_r,b8_r };
            return ballRight;
        }

        #endregion

        #region 关键量所在区域

        //判断区域（10个）

        int PointArea(Vector3 vector)
        {
            int area = 0;

            return area;
        }


        //下标 0-零号鱼 1-一号鱼 2-对方零号鱼 3-对方一号鱼 4-球0 5-球1 6-球2 7-球3 8-球4 9-球5 10-球6 11-球7 12-球8
        int[] getarea() {
            int fish0area = PointArea(getFishPosition()[1]);     //零号鱼
            int fish1area = PointArea(getFishPosition()[3]);     //一号鱼
            int fish2area = PointArea(getFishPosition()[5]);   //对方零号鱼
            int fish3area = PointArea(getFishPosition()[7]);   //对方一号鱼
            int ball0 = PointArea(getBallPosition()[0]);
            int ball1 = PointArea(getBallPosition()[1]);
            int ball2 = PointArea(getBallPosition()[2]);
            int ball3 = PointArea(getBallPosition()[3]);
            int ball4 = PointArea(getBallPosition()[4]);
            int ball5 = PointArea(getBallPosition()[5]);
            int ball6 = PointArea(getBallPosition()[6]);
            int ball7 = PointArea(getBallPosition()[7]);
            int ball8 = PointArea(getBallPosition()[8]);
            int[] area = { fish0area, fish1area, fish2area, fish3area, ball0, ball1, ball2, ball3, ball4, ball5, ball6, ball7, ball8 };
            return area;
        }
        #endregion

        //获取半场信息
        //1-左半场上半场
        //2-左半场下半场
        //3-右半场上半场
        //4-右半场下半场
        int getHalfCourt() {
            int set=0;
            #region 左半场
            if (MyHalfCourt == HalfCourt.LEFT)
            {
                if (RemainingTime >= 3000)
                {
                    set = 1;
                }
                else
                {
                    set = 2;
                }
            }
            #endregion

            #region 右半场
            if (MyHalfCourt == HalfCourt.RIGHT)
            {
                if (RemainingTime >= 3000)
                {
                    set = 3;
                }
                else
                {
                    set = 4;
                }
            }
            #endregion
            return set;
        }


    }
}
