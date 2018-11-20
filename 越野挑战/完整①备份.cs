            #region 上半区
			
            if (b0 == 0)
            {
                if (FishDecision.FishDecision.judgearea5(Trace, 1) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                {//前往假定点③
                    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume3, 10, 20, 30, 300, 10, 9, 2, 5, 100, false);
                }
                //if (FishDecision.FishDecision.judgearea5(Trace, 2) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                //{//前往假定点④
                //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_3, 10, 20, 30, 3, 14, 7, 5, 5, 100, false);
                //}
                //if (FishDecision.FishDecision.judgearea5(Trace, 3) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                //{//前往假定点⑤
                //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume4, 10, 20, 30, 3, 14, 7, 14, 5, 100, false);
                //}
                //if (FishDecision.FishDecision.judgearea5(Trace, 4) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                //{//前往假定点⑥
                //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_4, 10, 20, 50, 3, 14, 7, 14, 5, 100, true);
                //}
                //if (FishDecision.FishDecision.judgearea5(Trace, 5) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                //{
                //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume5, 10, 20, 50, 50, 14, 7, 7, 5, 100, true);
                //}
                //if (FishDecision.FishDecision.judgearea5(Trace, 6) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == false)
                //{
                //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume6, 10, 20, 30, 100, 14, 5, 7, 5, 100, true);
                //}
                //if (FishDecision.FishDecision.judgearea5(Trace, 7) == true)
                //{//前往洞点
                // /*提高预测点坐标*/
                //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 7, 2, 1);
                //}
                //if (FishDecision.FishDecision.judgearea5(Ball_1, 8) == true)
                //{
                //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 20, 30, 7, 2, 1);
                //}
                ////if (FishDecision.FishDecision.judgearea(Trace, 13) == true)
                ////{
                ////    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, Test1, 10, 20, 30, 7, 2, 1);
                ////}
                //if (FishDecision.FishDecision.judgearea5(Trace, 9) == true)
                //{//前往①号得分点
                // /*缩小角度后半段冲刺*/
                //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, hole, 10, 15, 45, 1, 1, 1);
                //    //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0,0,  keyPoint, 30, 45, 60, 1, 1, 1);
                //    //测试
                //    //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], keyPoint, 30, 45, 60, 3, 100, 3, 2, 5, 100, true);
                //}
                //if (FishDecision.FishDecision.judgearea5(Ball_1, 8) == true)//&&FishDecision.FishDecision.judgearea5(Ball_1,8)==false
                //{
                //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, keyPoint, 10, 15, 45, 7, 2, 1);
                //    //FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 0, keyPoint, 10, 30, 45, 7, 2, 1);
                //}
            }
            #region 下半区
            //if (FishDecision.FishDecision.judgearea5(Ball_1, 10) == true && FishDecision.FishDecision.judgearea5(Trace, 1) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_0, 10, 20, 30, 200, 14, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace,14)==true&&FishDecision.FishDecision.judgearea5(Trace,1)==false)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_1, 10, 20, 30, 3, 14, 7, 7, 5, 100, true);
            //}
            ////if (FishDecision.FishDecision.judgearea5(Trace, 8) == true && FishDecision.FishDecision.judgearea5(Ball_1, 10) == true)
            ////{
            ////}
            ////if (FishDecision.FishDecision.judgearea5(Trace, 11) == true )
            ////{
            ////    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_1, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            ////}
            //if (FishDecision.FishDecision.judgearea5(Trace, 1) == true)
            //{
            //    //FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2_, 10, 20, 30, 7, 7, 7, 7, 5, 100, true);
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 7, 7, 7, 7, 5, 100, true);
            //}
            ////if (FishDecision.FishDecision.judgearea(Trace, 1) == true)
            ////{
            ////    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            ////}
            //if (FishDecision.FishDecision.judgearea5(Trace, 2) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_2, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 3) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_6, 10, 20, 30, 3, 7, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 4) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_5, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 5) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_7, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 6) == true)
            //{
            //    FishDecision.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[0].Fishes[0], assume_8, 10, 20, 30, 100, 7, 7, 7, 5, 100, true);
            //}
            //if (FishDecision.FishDecision.judgearea5(Trace, 16) == true)
            //{
            //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_9, 10, 15, 20, 5, 4, 2);
            //}
            //if (FishDecision.FishDecision.judgearea5(Ball_2, 17) == true)
            //{
            //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_10, 10, 15, 20, 5, 4, 2);
            //}
            //if (FishDecision.FishDecision.judgearea5(Ball_2, 18) == true)
            //{
            //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_11, 10, 15, 20, 5, 4, 2);
            //}
            //if (FishDecision.FishDecision.judgearea5(Ball_2, 19) == true)
            //{
            //    FishDecision.FishDecision.Dribble_Will(mission, ref decisions[0], 0, 0, 1, assume_12, 10, 15, 20, 5, 4, 2);
            // }
            #endregion
