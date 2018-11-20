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
            Vector3 assume7 = new Vector3(200, 0, -1200);
            #endregion
            #region 上半区假定点2
            Vector3 p1 = new Vector3(1000, 0, 500);
            Vector3 p2 = new Vector3(600, 0, 0);
            Vector3 p3 = new Vector3(300, 0, -200);
            Vector3 p4 = new Vector3(-400, 0, -210);
            Vector3 p5 = new Vector3(-600, 0, 0);
            Vector3 p6 = new Vector3(-1200, 0, 400);
            Vector3 p7 = new Vector3(-1600, 0, 0);
            Vector3 p8 = new Vector3(-2000, 0, -900);
            Vector3 p9 = new Vector3(-2000, 0, -1000);
            #endregion
            #region 下半区假定点
            Vector3 assume_0 = new Vector3(1488f, 0f, -18f);    //假定重起点
            Vector3 assume_1 = new Vector3(1400f, 0f, -300f);    //假
            Vector3 o6 = new Vector3(-1600, 0, 0);
            Vector3 o7 = new Vector3(-2000, 0, 1000);
            #endregion
            #region 洞点
            Vector3 b00 = new Vector3(-1300, 0, -1100);
            Vector3 b2 = new Vector3(-850, 0, -1100);
            Vector3 b3 = new Vector3(-150, 0, -1200);
            Vector3 b4 = new Vector3(-100, 0, -1200);
            Vector3 hole = new Vector3(500, 0, -1140);
            Vector3 hole_1 = new Vector3(500, 0, 1200);
            #endregion
            #region 预设顶点
            Vector3 fakepoint = FishDecision.FishDecision.SetDestPtMm(hole, Ball_1, 0, 35);
            #endregion
            #region 得分点
            Vector3 keyPoint = new Vector3(1800f, 0f, -1200f);//得分点
            Vector3 keyPoint1 = new Vector3(1800f, 0f, 1200f);//得分点定点①-障碍①上
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
            #region 下半区假定点
            Vector3 o1 = new Vector3(1500, 0, -300);             //假定重起点
            Vector3 o_1 = new Vector3(1300, 0, -400);
            Vector3 o2 = new Vector3(1000, 0, -700);
            Vector3 o3 = new Vector3(700, 0, 100);
            Vector3 o4 = new Vector3(300, 0, 200);
            Vector3 o_4 = new Vector3(-400, 0, 100);
            Vector3 o5 = new Vector3(-1200, 0, -400);

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
            int b_2 = Convert.ToInt32(mission.HtMissionVariables["Ball1InHole"]);
            #endregion
            Vector3[] holel = new Vector3[2];
            #region 测试用点
            Vector3 Test1 = new Vector3(-1150f, 0f, -1200f);
            Vector3 Test2 = new Vector3(-800f, 0f, -1200f);
            Vector3 Test3 = new Vector3(-2050f, 0f, -1300f);
            #endregion
            #region test
            //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 30, 90, 7, 3, 4);

            #endregion
            #region 标志位
            int flag = 0;
            int flag1 = 0;
            int flag2 = 0;
            #endregion
            #region 上半区            
            if (FishDecision.FishDecision.judgearea5(Ball_1, 10) == true)
            {
                flag1 = 1;

            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 1) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p1, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 1;
            if (FishDecision.FishDecision.Judgearea6(Trace, 2) == true && b0 == 0 && flag == 1 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p2, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 0;

            if (FishDecision.FishDecision.Judgearea6(Trace, 3) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p3, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 1;

            if (FishDecision.FishDecision.Judgearea6(Trace, 4) == true && b0 == 0 && flag == 1 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p4, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 0;

            if (FishDecision.FishDecision.judgearea5(Trace, 4) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p5, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 1;

            if (FishDecision.FishDecision.judgearea5(Trace, 5) == true && b0 == 0 && flag == 1 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p6, 10, 20, 30, 100, 8, 6, 4, 10, 100, true);
            }
            flag = 0;

            if (FishDecision.FishDecision.Judgearea6(Trace, 7) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p7, 10, 30, 40, 100, 8, 6, 5, 10, 100, true);
            }
            flag = 1;

            if (FishDecision.FishDecision.Judgearea6(Trace, 9) == true && b0 == 0 && flag == 1 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p8, 10, 20, 40, 100, 14, 8, 6, 10, 100, true);
                //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 7, 5, 4);

            }
            flag = 0;
            if (FishDecision.FishDecision.Judgearea6(Trace, 11) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p9, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            flag = 1;
            if (FishDecision.FishDecision.Judgearea6(Trace, 15) == true && b0 == 0 && flag == 1 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 1, 5, 8, 6, 5, 4);//小区间内减速 
            }
            flag = 0;
            //if (FishDecision.FishDecision.judgearea5(Trace, 9) == true && b0 == 0 && flag == 0 && flag1 == 0)
            //{
            //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, assume7, 1, 3, 5, 3, 2, 1);

            //}
            //flag = 1;
            if (FishDecision.FishDecision.Judgearea6(Trace, 16) == true && b0 == 0 && flag == 0 && flag1 == 0)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, keyPoint, 1, 5, 8, 6, 5, 4);
            }




            #endregion
            #region 下半区

            if (FishDecision.FishDecision.Judgearea6(Trace, 16) && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o1, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 18) && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o_1, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 17) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o2, 10, 20, 30, 100, 8, 5, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 2) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], p2, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 4) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o4, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 5) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o_4, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 6) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o5, 100, 10, 20, 30, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 9) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o6, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 22) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], o7, 10, 20, 30, 100, 8, 6, 4, 5, 100, true);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 20) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, hole_1, 1, 5, 8, 6, 5, 4);
            }
            if (FishDecision.FishDecision.Judgearea6(Trace, 21) == true && flag1 == 1)
            {
                FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, keyPoint1, 1, 5, 8, 8, 5, 4);
            }


            #endregion