using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using URWPGSim2D.Core;
using xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;


namespace URWPGSim2D.FishDecision
{
    public static class FishDecision
    {
        #region 弧度转变为度
        public static float RedToDegree(float red)
        {
            float degree = red / (float)Math.PI * 180;
            return degree;
        }
        #endregion

        #region 获得鱼对目标的角度
        public static float GetFishToDestRad(Vector3 cur, Vector3 dest, float fishRad)
        {
            float fishtodestrad = 0;//接收目标对象和鱼的相对角度
            double tan = 0;  //接受正切值
            if (dest.X == cur.X)
            {                                //计算目标对象相当Fish[0]的相对角度bD,值为-PI~PI
                if (dest.Z > cur.Z)
                {
                    fishtodestrad = 90;
                }
                else
                {
                    fishtodestrad = -90;
                }
            }
            else if (dest.X - cur.X > 0)
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                fishtodestrad = RedToDegree((float)Math.Atan(tan));
            }
            else if (dest.Z >= cur.Z)
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                fishtodestrad = 180 + RedToDegree((float)Math.Atan(tan));
            }
            else
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                fishtodestrad = -180 + RedToDegree((float)Math.Atan(tan));
            }

            float xD = fishtodestrad - RedToDegree(fishRad);          //鱼应该转动的角度，正为顺时，负为逆时
            if (xD < -180)
            {
                xD += 360;
            }
            else if (xD > 180)
            {
                xD -= 360;
            }
            return xD;
        }
        #endregion

        #region 实现鱼转向目标的操作
        public static void FishTrun(Mission mission, ref Decision decision, int teamID, int fishID, int ballID)
        {
            float fishtodestrad = GetFishToDestRad(mission.TeamsRef[teamID].Fishes[fishID].PositionMm, mission.EnvRef.Balls[ballID].PositionMm, mission.TeamsRef[teamID].Fishes[fishID].BodyDirectionRad);



            int tc = 14;            //给予初始的一定角速度排除球和鱼成角180的情况方向初始位顺时针
            int vc = 1;
            if (fishtodestrad > -10 && fishtodestrad < 10)
            {
                vc = 6;
            }
            if (fishtodestrad <= -90)
            {
                tc = 0;
            } else if (fishtodestrad < 0)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) - 3;  // 在-90~0度间最大速度为0最小为3
            }
            else if (fishtodestrad < 90)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) + 3;   // 在0~90度间最大速度为14最小为11
            }
            else
            {
                tc = 14;
            }
            decision.TCode = tc;
            decision.VCode = vc;

        }
        #endregion

        #region dribble算法
        /// <summary>
        /// dirbble带球算法 Modified By Zhangbo 2011.10.22
        /// </summary>
        /// <param name="decision">每周期机器鱼执行策略，包含速度档位，转弯档位。</param>
        /// <param name="fish">目标仿真机器鱼参数，包含当前位置、速度信息。</param>
        /// <param name="destPtMm">临时目标点。</param>
        /// <param name="destDirRad">目标方向。</param>
        /// <param name="angleTheta1">鱼体方向与目标方向角度差的阈值一。
        ///   角度差在此阈值范围内，则赋给机器鱼一个合理的速度档位（见参数disThreshold说明）。</param>
        /// <param name="angleTheta2">鱼体方向与目标方向角度差的阈值二。角度差小于此阈值，则机器鱼直线游动；
        /// 角度差大于此阈值，则机器鱼调整游动方向。</param>
        /// <param name="disThreshold">距离阈值。距离大于此阈值，机器鱼以速度档位VCode1游动；
        /// 距离小于此阈值，机器鱼以速度档位VCode2游动。</param>
        /// /// <param name="VCode1">直游档位1（默认6档）。</param>
        /// /// <param name="VCode2">直游档位2（默认4档）。</param>
        /// <param name="cycles">速度和转弯档位之间切换所需周期数经验值。建议取值范围在5-20之间。此参数作用是防止机器鱼“转过”。</param>
        /// <param name="msPerCycle">每个仿真周期的毫秒数，传递固定参数，不能修改。</param>
        /// <param name="flag">机器鱼坐标选择标准，true为PositionMm，即鱼体绘图中心；false为PolygonVertices[0]，即鱼头点。</param>
        public static void Dribble(ref Decision decision, RoboFish fish, xna.Vector3 destPtMm, float destDirRad,
            float angleTheta1, float angleTheta2, float disThreshold, int VCode1, int VCode2, int cycles, int msPerCycle, bool flag)
        {
            // 调节所用周期数及每周期毫秒数转换得到秒数
            double seconds1 = 15 * msPerCycle / 1000.0;
            double seconds2 = cycles * msPerCycle / 1000.0;
            // 标志量为true则起始点为PositionMm即鱼体绘图中心false则起始点为PolygonVertices[0]即鱼头点（起始点）
            xna.Vector3 srcPtMm = (flag == true) ? fish.PositionMm : fish.PolygonVertices[0];
            // 起始点到目标点的距离（目标距离）
            double disSrcPtMmToDestPtMm = Math.Sqrt(Math.Pow(destPtMm.X - srcPtMm.X, 2.0)
                + Math.Pow(destPtMm.Z - srcPtMm.Z, 2.0));

            // 鱼体绘图中心指向目标点向量方向的弧度值（中间方向）
            double dirFishToDestPtRad = xna.MathHelper.ToRadians((float)GetAngleDegree(destPtMm - fish.PositionMm));
            if (disSrcPtMmToDestPtMm < 0)
            {// 起始点到目标点距离小于阈值（默认58毫米）将中间方向调为目标方向
                dirFishToDestPtRad = destDirRad;
            }

            // 中间方向与鱼体方向的差值（目标角度）
            double deltaTheta = dirFishToDestPtRad - fish.BodyDirectionRad;
            // 将目标角度规范化到(-PI,PI]
            // 规范化之后目标角度为正表示目标方向在鱼体方向右边
            // 规范化之后目标角度为负表示目标方向在鱼体方向左边
            if (deltaTheta > Math.PI)
            {// 中间方向为正鱼体方向为负才可能目标角度大于PI
                deltaTheta -= 2 * Math.PI;  // 规范化到(-PI,0)
            }
            else if (deltaTheta < -Math.PI)
            {// 中间方向为负鱼体方向为正才可能目标角度小于-PI
                deltaTheta += 2 * Math.PI;  // 规范化到(0,PI)
            }

            // 最大角速度取左转和右转最大角速度绝对值的均值
            float maxAngularV = (Math.Abs(DataBasedOnExperiment.TCodeAndAngularVelocityTable[0])
                + Math.Abs(DataBasedOnExperiment.TCodeAndAngularVelocityTable[14])) / 2;
            // 以最大角速度转过目标角度所需的预计时间（角度预计时间）
            double estimatedTimeByAngle = Math.Abs((double)(deltaTheta / maxAngularV));
            // 以角度预计时间游过目标距离所需平均速度值（目标速度）
            double targetVelocity = disSrcPtMmToDestPtMm / estimatedTimeByAngle;

            int code = 1;   // 目标（速度）档位初值置1
            while ((code < 10) && (DataBasedOnExperiment.VCodeAndVelocityTable[code] < targetVelocity))
            {// 目标（速度）档位对应的速度值尚未达到目标速度则调高目标（速度）档位
                code++;
            }
            decision.VCode = code;
            if (Math.Abs(deltaTheta) > angleTheta2 * Math.PI / 180.0)
            {// 目标角度绝对值超过某一阈值，速度档位置次低进行小半径转弯
                decision.VCode = 1;
            }
            else if (Math.Abs(deltaTheta) < angleTheta1 * Math.PI / 180.0)
            {// 目标角度绝对值小于某一阈值，若此时距离较远速度档置较高高全速前进，否则置适中档位前进。
                if (disSrcPtMmToDestPtMm > disThreshold)
                {
                    decision.VCode = VCode1;
                }
                else
                {
                    decision.VCode = VCode2;
                }
            }


            // 以最大速度游过目标距离所需的预计时间（距离预计时间）
            double estimatedTimeByDistance = disSrcPtMmToDestPtMm / DataBasedOnExperiment.VCodeAndVelocityTable[14];
            if (estimatedTimeByDistance > seconds1)
            {// 距离预计时间超过一次档位切换所需平均时间则取为该时间（默认为1秒）
                estimatedTimeByDistance = seconds1;
            }
            // 以距离预计时间游过目标角度所需平均角速度（目标角速度）
            double targetAngularV = deltaTheta / estimatedTimeByDistance;

            code = 7;
            if (deltaTheta <= 0)
            {// 目标角度为负目标方向在鱼体方向左边需要给左转档位（注意左转档位对应的角速度值为负值）
                while ((code > 0) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] > targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调低目标（转弯）档位
                    code = code - 2;
                }
                if ((fish.AngularVelocityRadPs * seconds2) < deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 12;
                }
                if (code < 0)
                    code = 0;
            }
            else
            {// 目标角度为正目标方向在鱼体方向右边需要给右转档位
                while ((code < 14) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] < targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调高目标（转弯）档位
                    code = code + 2;
                }
                if ((fish.AngularVelocityRadPs * seconds2) > deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 2;
                }
                if (code > 14)
                    code = 14;
            }
            decision.TCode = code;
        }
        #endregion

        #region dribble2算法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decision">每周期机器鱼执行策略，包含速度档位，转弯档位。</param>
        /// <param name="fish">目标仿真机器鱼参数，包含当前位置、速度信息。</param>
        /// <param name="destPtMm">临时目标点。</param>
        /// <param name="angleTheta1">鱼体方向与目标方向角度差的阈值一。
        ///   角度差在此阈值范围内，则赋给机器鱼一个合理的速度档位（见参数disThreshold说明）。</param>
        /// <param name="angleTheta2">鱼体方向与目标方向角度差的阈值二。
        ///   角度差在此阈值范围内，则赋给机器鱼一个合理的次速度档位。</param>
        /// <param name="angleTheta3">鱼体方向与目标方向角度差的阈值三。角度差小于此阈值，则机器鱼直线游动；
        /// 角度差大于此阈值，则机器鱼调整游动方向。</param>
        /// <param name="disThreshold">距离阈值。距离大于此阈值，机器鱼以速度档位VCode1游动；
        /// 距离小于此阈值，机器鱼以速度档位VCode2游动。</param>
        /// /// <param name="VCode1">直游档位1（默认8档）。</param>
        /// /// <param name="VCode2">直游档位2（默认6档）。</param>
        /// /// <param name="VCode3">直游档位3（默认4档）。</param>
        /// <param name="cycles">速度和转弯档位之间切换所需周期数经验值。建议取值范围在5-20之间。此参数作用是防止机器鱼“转过”。</param>
        /// <param name="msPerCycle">每个仿真周期的毫秒数，传递固定参数，不能修改。</param>
        /// <param name="flag">机器鱼坐标选择标准，true为PositionMm，即鱼体绘图中心；false为PolygonVertices[0]，即鱼头点。</param>
        public static void Dribble2(ref Decision decision, RoboFish fish, xna.Vector3 destPtMm,
            float angleTheta1, float angleTheta2, float angleTheta3, float disThreshold, int VCode1, int VCode2, int VCode3, int cycles, int msPerCycle, bool flag)
        {
            // 调节所用周期数及每周期毫秒数转换得到秒数
            double seconds1 = 15 * msPerCycle / 1000.0;
            double seconds2 = cycles * msPerCycle / 1000.0;
            // 标志量为true则起始点为PositionMm即鱼体绘图中心false则起始点为PolygonVertices[0]即鱼头点（起始点）
            xna.Vector3 srcPtMm = (flag == true) ? fish.PositionMm : fish.PolygonVertices[0];
            // 起始点到目标点的距离（目标距离）
            double disSrcPtMmToDestPtMm = Math.Sqrt(Math.Pow(destPtMm.X - srcPtMm.X, 2.0)
                + Math.Pow(destPtMm.Z - srcPtMm.Z, 2.0));

            // 鱼体绘图中心指向目标点向量方向的弧度值（中间方向）
            double dirFishToDestPtRad = xna.MathHelper.ToRadians((float)GetAngleDegree(destPtMm - fish.PositionMm));


            // 中间方向与鱼体方向的差值（目标角度）
            double deltaTheta = dirFishToDestPtRad - fish.BodyDirectionRad;
            // 将目标角度规范化到(-PI,PI]
            // 规范化之后目标角度为正表示目标方向在鱼体方向右边
            // 规范化之后目标角度为负表示目标方向在鱼体方向左边
            if (deltaTheta > Math.PI)
            {// 中间方向为正鱼体方向为负才可能目标角度大于PI
                deltaTheta -= 2 * Math.PI;  // 规范化到(-PI,0)
            }
            else if (deltaTheta < -Math.PI)
            {// 中间方向为负鱼体方向为正才可能目标角度小于-PI
                deltaTheta += 2 * Math.PI;  // 规范化到(0,PI)
            }

            // 最大角速度取左转和右转最大角速度绝对值的均值
            float maxAngularV = (Math.Abs(DataBasedOnExperiment.TCodeAndAngularVelocityTable[0])
                + Math.Abs(DataBasedOnExperiment.TCodeAndAngularVelocityTable[14])) / 2;
            // 以最大角速度转过目标角度所需的预计时间（角度预计时间）
            double estimatedTimeByAngle = Math.Abs((double)(deltaTheta / maxAngularV));
            // 以角度预计时间游过目标距离所需平均速度值（目标速度）
            double targetVelocity = disSrcPtMmToDestPtMm / estimatedTimeByAngle;

            int code = 1;   // 目标（速度）档位初值置1
            while ((code < 10) && (DataBasedOnExperiment.VCodeAndVelocityTable[code] < targetVelocity))
            {// 目标（速度）档位对应的速度值尚未达到目标速度则调高目标（速度）档位
                code++;
            }
            decision.VCode = code;


            if (Math.Abs(deltaTheta) < angleTheta1 * Math.PI / 180.0)
            {// 目标角度绝对值小于某一阈值，若此时距离较远速度档置较高高全速前进，否则置适中档位前进。
                if (disSrcPtMmToDestPtMm < disThreshold)
                {
                    decision.VCode = VCode1;
                }
                else
                {
                    decision.VCode = VCode1 + 3;
                }
                //decision.VCode = VCode1;
            }
            else if (Math.Abs(deltaTheta) < angleTheta2 * Math.PI / 180.0)
            {
                if (disSrcPtMmToDestPtMm < disThreshold)
                {
                    decision.VCode = VCode2;
                }
                else
                {
                    decision.VCode = VCode2 + 2;
                }
            }
            else if (Math.Abs(deltaTheta) < angleTheta3 * Math.PI / 180.0)
            {
                if (disSrcPtMmToDestPtMm < disThreshold)
                {
                    decision.VCode = VCode2;
                }
                else
                {
                    decision.VCode = VCode2 + 1;
                }
            }
            else
            {// 目标角度绝对值超过某一阈值，速度档位置次低进行小半径转弯
                decision.VCode = 1;
            }

            // 以最大速度游过目标距离所需的预计时间（距离预计时间）
            double estimatedTimeByDistance = disSrcPtMmToDestPtMm / DataBasedOnExperiment.VCodeAndVelocityTable[14];
            if (estimatedTimeByDistance > seconds1)
            {// 距离预计时间超过一次档位切换所需平均时间则取为该时间（默认为1秒）
                estimatedTimeByDistance = seconds1;
            }
            // 以距离预计时间游过目标角度所需平均角速度（目标角速度）
            double targetAngularV = deltaTheta / estimatedTimeByDistance;

            code = 7;
            if (deltaTheta <= 0)
            {// 目标角度为负目标方向在鱼体方向左边需要给左转档位（注意左转档位对应的角速度值为负值）
                while ((code > 0) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] > targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调低目标（转弯）档位
                    code = code - 1;
                }
                if ((fish.AngularVelocityRadPs * seconds2) < deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 12;
                }
                if (code < 0)
                    code = 0;
            }
            else
            {// 目标角度为正目标方向在鱼体方向右边需要给右转档位
                while ((code < 14) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] < targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调高目标（转弯）档位
                    code = code + 1;
                }
                if ((fish.AngularVelocityRadPs * seconds2) > deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 2;
                }
                if (code > 14)
                    code = 14;
            }
            decision.TCode = code;
        }
        #endregion

        #region 获得角度
        /// <summary>
        /// 返回Vector3类型的向量（Y置0，只有X和Z有意义）在场地坐标系中方向的角度值
        /// 场地坐标系定义为：X向右，Z向下，Y置0，负X轴顺时针转回负X轴角度范围为(-PI,PI)的坐标系
        /// </summary>
        /// <param name="v">待计算角度值的xna.Vector3类型向量</param>
        /// <returns>向量v在场地坐标系中方向的角度值</returns>
        public static float GetAngleDegree(xna.Vector3 v)
        {
            float x = v.X;
            float y = v.Z;
            float angle = 0;

            if (Math.Abs(x) < float.Epsilon)
            {// x = 0 直角反正切不存在
                if (Math.Abs(y) < float.Epsilon) { angle = 0.0f; }
                else if (y > 0) { angle = 90.0f; }
                else if (y < 0) { angle = -90.0f; }
            }
            else if (x < 0)
            {// x < 0 (90,180]或(-180,-90)
                if (y >= 0) { angle = (float)(180 * Math.Atan(y / x) / Math.PI) + 180.0f; }
                else { angle = (float)(180 * Math.Atan(y / x) / Math.PI) - 180.0f; }
            }
            else
            {// x > 0 (-90,90)
                angle = (float)(180 * Math.Atan(y / x) / Math.PI);
            }

            return angle;
        }
        #endregion

        #region 获得临时目标点
        /// <summary>
        /// 
        /// </summary>
        /// <param name="goal">为目标坐标</param>
        /// <param name="ball">为目标球的坐标</param>
        /// <param name="setDegree">设置偏转角度</param>
        /// <param name="setDist">为设置的临时目标点相距球的距离 一般30~45</param>
        /// <returns></returns>

        public static Vector3 SetDestPtMm(xna.Vector3 goal, xna.Vector3 ball, int setDegree, int setDist)
        {
            //求 洞指向球 的单位向量
            double xx = (ball.X - goal.X) / Math.Pow(((goal.X - ball.X) * (goal.X - ball.X) + (goal.Z - ball.Z) * (goal.Z - ball.Z)), 0.5);
            double zz = (ball.Z - goal.Z) / Math.Pow(((goal.X - ball.X) * (goal.X - ball.X) + (goal.Z - ball.Z) * (goal.Z - ball.Z)), 0.5);
            float rad = (float)(setDegree * Math.PI / 180);
            double tx = xx * Math.Cos(rad) - zz * Math.Sin(rad);
            double tz = xx * Math.Sin(rad) + zz * Math.Cos(rad);
            Vector3 p = new Vector3();
            p.X = ball.X + (float)tx * setDist;
            p.Z = ball.Z + (float)tz * setDist;
            return p;
        }
        #endregion

        #region 获得目标方向
        public static float GetDirRad(xna.Vector3 cur, xna.Vector3 dest)
        {
            float dirRad = 0;//接收目标对象和鱼的相对角度
            double tan = 0;  //接受正切值
            if (dest.X == cur.X)
            {                                //计算目标对象相当Fish[0]的相对角度bD,值为-PI~PI
                if (dest.Z > cur.Z)
                {
                    dirRad = (float)Math.PI / 2;
                }
                else
                {
                    dirRad = -(float)Math.PI / 2;
                }
            }
            else if (dest.X - cur.X > 0)
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                dirRad = (float)Math.Atan(tan);
            }
            else if (dest.Z >= cur.Z)
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                dirRad = (float)Math.PI + (float)Math.Atan(tan);
            }
            else
            {
                tan = (dest.Z - cur.Z) / (dest.X - cur.X);
                dirRad = -(float)Math.PI + (float)Math.Atan(tan);
            }

            return dirRad;

        }
        #endregion

        #region 判断是否在可带球范围
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fish">动作鱼</param>
        /// <param name="ball">目标球</param>
        /// <param name="dest">目标点</param>
        /// <param name="dis">偏置距离，一般是球心后100左右</param>
        /// <param name="radius">范围半径一般为60</param>
        /// <returns></returns>
        public static bool JudgeArea(RoboFish fish, xna.Vector3 ball, xna.Vector3 dest, int setDegree, int dis, int radius)
        {
            float fX = fish.PositionMm.X;
            float fZ = fish.PositionMm.Z;
            Vector3 area = new Vector3();
            area = SetDestPtMm(dest, ball, setDegree, dis);
            float xm = area.X;
            float zm = area.Z;
            if ((fX - xm) * (fX - xm) + (fZ - zm) * (fZ - zm) <= radius * radius)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region 计算鱼所在位置到目标球的角度于目标角度的差
        public static float GetFishAtBallAngleDegree(xna.Vector3 dest, xna.Vector3 ball, xna.Vector3 fish)
        {
            float AngleDegree = 0;
            float rad1 = GetDirRad(dest, ball);
            float rad2 = GetDirRad(fish, ball);
            double Trad = rad2 - rad1;
            if (Trad > Math.PI)
            {
                Trad -= 2 * Math.PI;
            }
            else if (Trad < -Math.PI)
            {
                Trad += 2 * Math.PI;
            }
            AngleDegree = (float)(Trad / Math.PI * 180);
            return AngleDegree;
        }
        #endregion

        #region 计算两点距离
        public static double GetDistBetwGB(xna.Vector3 v1, xna.Vector3 v2)
        {
            return Math.Pow((v1.X - v2.X) * (v1.X - v2.X) + (v1.Z - v2.Z) * (v1.Z - v2.Z), 0.5);
        }
        #endregion

        #region 判断区域
        /// <summary>
        /// 判断目标是否在指定区域内，全场氛围7个区域，整数1-7分别对应相关区域
        /// </summary>
        /// <param name="p">目标坐标</param>
        /// <param name="area">区域编号</param>
        /// <returns>bool</returns>
        public static bool JudgePIsInArea(xna.Vector3 p, int area)
        {
            switch (area)
            {
                case 1:
                    if (p.X <= -740 && p.Z >= -600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (p.X <= -70 && p.Z <= -600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (p.X >= -600 && p.X <= -70 && p.Z >= -600 && p.Z <= 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (p.X >= -600 && p.X <= 600 && p.Z > 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 5:
                    if (p.X >= 0 && p.X <= 600 && p.Z >= -600 && p.Z <= 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    if (p.X >= 0 && p.Z <= -600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 7:
                    if (p.X >= 740 && p.Z >= -600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
        #endregion

        #region 设置定角方向偏置
        /// <summary>
        /// 设置定角方向偏置,获得固定方向，实现鱼的固定方向游动
        /// </summary>
        /// <param name="fish">鱼的坐标</param>
        /// <param name="degree">偏转的角度（+为顺时针，-为逆时针）</param>
        /// <returns>Vector3</returns>
        public static Vector3 SetDegreeFishToP(xna.Vector3 fish, int degree)
        {
            xna.Vector3 p = new Vector3();
            float rad = (float)(degree * Math.PI / 180);
            double tx = 100 * Math.Cos(rad) - 0 * Math.Sin(rad);
            double tz = 100 * Math.Sin(rad) + 0 * Math.Cos(rad);
            p.X = fish.X + (float)tx;
            p.Z = fish.Z + (float)tz;
            return p;
        }
        #endregion

        #region Dribble_Will封装
        /// <summary>
        /// Dribble_Will封装
        /// </summary>
        /// <param name="mission">mission</param>
        /// <param name="decision">ref decision</param>
        /// <param name="teamId">teamId</param>
        /// <param name="fishId">fishId</param>
        /// <param name="ballId">目标球编号</param>
        /// <param name="goal">目标洞坐标</param>
        public static void Dribble_Will(Mission mission, ref Decision decision, int teamId, int fishId, int ballId, xna.Vector3 goal,
             float angleTheta1, float angleTheta2, float angleTheta3, int VCode1, int VCode2, int VCode3)
        {
            //接收球的坐标
            xna.Vector3 bM = mission.EnvRef.Balls[ballId].PositionMm;
            //接收鱼的坐标      
            xna.Vector3 fM = mission.TeamsRef[teamId].Fishes[fishId].PositionMm;
            //防止反带球绕后///////////
            #region 绕后处理
            ////////////如果在鱼驶向目标球后方时，球会被误撞，就给他给他一个偏转角，从侧面过去//////////////////////////////////
            int Tdegree = 0;  //根据情况设置不同的偏置角度
            if (GetFishAtBallAngleDegree(goal, bM, fM) < 80 && GetFishAtBallAngleDegree(goal, bM, fM) > 0)
            {
                Tdegree = -60;
            }
            else if (GetFishAtBallAngleDegree(goal, bM, fM) > -80 && GetFishAtBallAngleDegree(goal, bM, fM) < 0)
            {
                Tdegree = 60;
            }
            #endregion


            //临时目标点//////////////////////////////////处理后置偏转的问题/////////////////////////////////////////////////
            #region 临时目标点
            /////////////////////当球靠近洞口的时候，临时目标点的偏置变量后向后推，经验值51//////////////////////////////////
            xna.Vector3 P = new xna.Vector3();//////存放临时目标点
            int setD = 40;                  ///////偏执变量
                                            //    if ((bM.X > -110 && bM.X < 110) &&
                                            //        (bM.Z < -850 || bM.Z > 850) ||
                                            //        (GetDistBetwGB(bM, goal) <= 550) && goal.X == 0 ||
                                            //        ballId == 7
                                            //        )
                                            //    {
                                            //        setD = 51;
                                            //        //当目标球在左半场且距离洞口很近的时候，偏置放大，防止定过
                                            //        if (bM.X < -250 && goal.X == 0 ||
                                            //            Math.Abs(bM.Z) > 1200)
                                            //        {
                                            //            setD = 61;
                                            //        }
                                            //
                                            //        if (bM.Z > 1330 || bM.Z < -1330)
                                            //        {
                                            //            setD = 10;
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        setD = 23;
                                            //    }
            #endregion

            //生成临时目标点
            P = SetDestPtMm(goal, bM, 0, setD);


            //判断鱼是否在球后
            bool judge = JudgeArea(mission.TeamsRef[teamId].Fishes[fishId], bM, goal, Tdegree, 250, 250);

            #region Dribble调用

            if (judge)//////判断是否在可顶球的范围
            {
                //         if (goal.X == 0) //中袋
                //         {
                //             if (ballId == 0 && bM.Z < 600)
                //             {
                //
                //                 Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], P, 2, 4, 6, 180, 8, 5, 4, 10, 100, true);
                //             }
                //
                Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], P, angleTheta1, angleTheta2, angleTheta3, 200, VCode1, VCode2, VCode3, 10, 100, true);
                //         }
                //
                //         else //底袋
                //         {
                //
                //
                //             Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], P, 4, 6, 7, 180, 8, 6, 4, 10, 100, true);
                //             // fish.FishDecision.Dribble(ref decisions[0], mission.TeamsRef[teamId].Fishes[0], P, dirRad, 3, 5, 150, 9, 5, 10, 100, true);
                //         }
                //         //    if (judge2) 
                //         //    {
                //         //        Vector3 tP = fish.FishDecision.SetDestPtMm(goal, mission.EnvRef.Balls[index].PositionMm, 0, 500);
                //         //        fish.FishDecision.Dribble2(ref decisions[0], mission.TeamsRef[teamId].Fishes[0] ,tP, 4, 6, 8, 180, 7, 6, 4, 10, 100, true);
                //         //    }
                //
                //         //中袋洞口处，鱼加速 
                //         if ((bM.Z < -1330 || bM.Z > 1330) && (bM.X > -45 && bM.X < 45))
                //         {
                //             Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], P, 4, 6, 9, 300, 13, 10, 4, 10, 100, true);
                //         }
                //
                //        //边带带球加速
                //         else if ((bM.Z < -1150 || bM.Z > 1150) && (bM.X > 2140 || bM.X < -2140) ||
                //             (bM.Z < -1200 || bM.Z > 1200) && (bM.X > 1950 || bM.X < -1950)
                //             )
                //         {
                //             Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], P, 4, 6, 9, 300, 13, 10, 4, 10, 100, true);
                //         }
            }
            else  ////距离远时，全速驶向球后
            {

                int Dis = 300;
                //如果球就在洞口则直接冲向球
                if (bM.Z > 1350 || bM.Z < -1350)
                {
                    Dis = 10;
                }



                ///////开球偏置//////////////////////////////////////////////////////////////////////////////
                if (ballId == 0 && mission.TeamsRef[teamId].Fishes[fishId].PositionMm.X < -360)
                {
                    Dis = 600;
                    Tdegree = -65;
                }

                //         fish.FishDecision.Dribble2(
                //              ref decisions[0], mission.TeamsRef[teamId].Fishes[0],
                //              fish.FishDecision.SetDestPtMm(goal, bM, Tdegree, Dis),
                //              6, 11, 30, 1000, 10, 9, 8, 10, 100, true);
                Dribble(
                     ref decision, mission.TeamsRef[teamId].Fishes[fishId],
                     SetDestPtMm(goal, bM, Tdegree, Dis),
                     GetDirRad(mission.TeamsRef[teamId].Fishes[fishId].PositionMm,
                     SetDestPtMm(goal, bM, Tdegree, Dis)) + 3.14f,
                     20, 50, 800, 14, 10, 10, 100, true);

                /////////////////防止尾巴扫球////////////////////////////////////////////////////////
                //         if (bN[index] == 9)
                //         {
                //             if (mission.TeamsRef[teamId].Fishes[0].PositionMm.X > -450)
                //             {
                //                 Vector3 tb = new Vector3(-600, 0, 1000);
                //                 fish.FishDecision.Dribble(
                //                  ref decisions[0], mission.TeamsRef[teamId].Fishes[0],
                //                  fish.FishDecision.SetDestPtMm(goal, tb, Tdegree, 0),
                //                  fish.FishDecision.GetDirRad(mission.TeamsRef[teamId].Fishes[0].PositionMm,
                //                  fish.FishDecision.SetDestPtMm(goal, tb, Tdegree, 0)) + 3.14f,
                //                  10, 40, 800, 9, 4, 10, 100, true);
                //             }
                //         }
                ///===================================================================////
            }
            #endregion


        }
        #endregion

        #region 移动到定点并保持姿态
        public static void MoveToAreaS(Mission mission, ref Decision decision, int teamId, int fishId, xna.Vector3 goal, int degree)
        {
            float degreeF = (float)(mission.TeamsRef[teamId].Fishes[fishId].BodyDirectionRad / Math.PI * 180);
            xna.Vector3 fM = mission.TeamsRef[teamId].Fishes[fishId].PositionMm;

            if (GetDistBetwGB(fM, goal) < 150)
            {
                xna.Vector3 p = SetDegreeFishToP(fM, degree);
                Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], p, 60, 150, 180, 200, 1, 1, 3, 10, 100, true);
                if (Math.Abs(degree - degreeF) <= 15)
                {
                    decision.TCode = 7;
                    decision.VCode = 0;
                }
            }
            else if (Math.Abs(degree - degreeF) <= 15 && GetDistBetwGB(fM, goal) < 200)
            {
                decision.TCode = 7;
                decision.VCode = 0;
            }
            else
            {
                Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], goal, 9, 20, 60, 200, 8, 8, 8, 10, 100, true);
            }

        }
        //7 5 2
        #endregion

        #region 得到鱼体内一点坐标
        /// <summary>
        /// 得到鱼体内中轴线上一点坐标
        /// </summary>
        /// <param name="mission">mission</param>
        /// <param name="teamId">teamId</param>
        /// <param name="fishId">fishId</param>
        /// <param name="setD">偏置量（从鱼几何中心反向鱼头的距离，‘-’为向鱼头，‘+’相反）</param>
        /// <returns>Vector3</returns>
        public static Vector3 GetFishBodyPM(Mission mission, int teamId, int fishId, int setD)
        {
            Vector3 head = mission.TeamsRef[teamId].Fishes[fishId].PolygonVertices[0];
            Vector3 bodyIn = mission.TeamsRef[teamId].Fishes[fishId].PositionMm;
            return SetDestPtMm(head, bodyIn, 0, setD);
        }
        #endregion

        #region 转向
        public static void turn(Mission mission, ref Decision decision, int teamID, int fishID, Vector3 dest)
        {
            float fishtodestrad = GetFishToDestRad(mission.TeamsRef[teamID].Fishes[fishID].PositionMm, dest, mission.TeamsRef[teamID].Fishes[fishID].BodyDirectionRad);
            int tc = 14;            //给予初始的一定角速度排除球和鱼成角180的情况方向初始位顺时针

            int vc = 0;//1
            if (fishtodestrad > -10 && fishtodestrad < 10)
            {

                vc = 6;
            }
            if (fishtodestrad <= -90)
            {
                tc = 0;
            }
            else if (fishtodestrad < 0)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) - 3;  // 在-90~0度间最大速度为0最小为3
            }
            else if (fishtodestrad < 90)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) + 3;   // 在0~90度间最大速度为14最小为11
            }
            else
            {
                tc = 14;
            }
            decision.TCode = tc;
            decision.VCode = vc;


        }
        #endregion

        #region 判定区域2
        public static bool judgearea(Vector3 p, int area)
        {
            switch (area)
            {
                case 1:
                    if (p.X <= -100 && p.Z <= -800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (p.X >= 100 && p.Z <= -800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (p.X <= -100 && p.Z >= -600 && p.Z <= -100)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (p.X >= 100 && p.Z <= -100 && p.Z >= -600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 5:
                    if (p.X < -100 && p.Z >= 100 && p.Z <= 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    if (p.X >= 100 && p.Z >= 100 && p.Z <= 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 7:
                    if (p.X <= -100 && p.Z >= 800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 8:
                    if (p.X >= 100 && p.Z >= 800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 9:
                    if (p.X <= 100 && p.X > 100 && p.Z <= -800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 10:
                    if (p.X <= -100 && p.Z <= -600 && p.Z > -800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 11:
                    if (p.X > 100 && p.Z <= -600 && p.Z > -800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 12:
                    if (p.X <= 100 && p.X > -100 && p.Z <= -600 && p.Z < -100)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 13:
                    if (p.X <= -100 && p.Z <= 100 && p.Z > -100)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 14:
                    if (p.X > 100 && p.Z <= 100 && p.Z > -100)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 15:
                    if (p.X <= 100 && p.X > -100 && p.Z <= 600 && p.Z > 100)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 16:
                    if (p.X <= -100 && p.Z <= 800 && p.Z > 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 17:
                    if (p.X > 100 && p.Z <= 800 && p.Z > 600)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 18:
                    if (p.X <= 100 && p.X > -100 && p.Z > 800)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
        #endregion


        #region 判断区域3
        public static bool judgearea3(Vector3 p, int area)
        {
            switch (area)
            {
                case 1:
                    if (p.X <= -200 && p.Z <= -500)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (p.X >= 200 && p.X < 2150 && p.Z <= -900 && p.Z > -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (p.X <= -200 && p.Z >= -500 && p.Z <= -0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (p.X >= 200 && p.X < 2150 && p.Z >= -900 && p.Z <= 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 5:
                    if (p.X < -200 && p.Z >= 0 && p.Z <= 900)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    if (p.X >= 200 && p.X < 2150 && p.Z >= 0 && p.Z <= 500)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 7:
                    if (p.X <= -200 && p.X > -2150 && p.Z >= 900 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 8:
                    if (p.X >= 200 && p.X < 2150 && p.Z >= 500 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 9:
                    if (p.X >= -200 && p.X < 200 && p.Z >= -500 && p.Z < -200)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 10:
                    if (p.X >= -200 && p.X < 200 && p.Z >= 200 && p.Z < 500)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 11:
                    if (p.Z > 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 12:
                    if (p.X > 2150 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 13:
                    if (p.X < 2150 && p.Z < -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
        #endregion


        #region 转向2
        public static void Turn(Mission mission, ref Decision decision, int teamID, int fishID, Vector3 dest)
        {
            float fishtodestrad = GetFishToDestRad(mission.TeamsRef[teamID].Fishes[fishID].PositionMm, dest, mission.TeamsRef[teamID].Fishes[fishID].BodyDirectionRad);
            int tc = 14;            //给予初始的一定角速度排除球和鱼成角180的情况方向初始位顺时针

            int vc = 8;
            if (fishtodestrad > -10 && fishtodestrad < 10)
            {

                vc = 8;
            }
            if (fishtodestrad <= -90)
            {
                tc = 0;
            }
            else if (fishtodestrad < 0)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) - 3;  // 在-90~0度间最大速度为0最小为3
            }
            else if (fishtodestrad < 90)
            {
                tc = 7 + (int)(fishtodestrad / (90 / 4)) + 3;   // 在0~90度间最大速度为14最小为11
            }
            else
            {
                tc = 14;
            }
            decision.TCode = tc;
            decision.VCode = vc;


        }
        #endregion


        #region 移动2
        public static void move2(Mission mission, ref Decision decision, int teamId, int fishId, xna.Vector3 goal, int degree)
        {
            float degreeF = (float)(mission.TeamsRef[teamId].Fishes[fishId].BodyDirectionRad / Math.PI * 180);
            xna.Vector3 fM = mission.TeamsRef[teamId].Fishes[fishId].PositionMm;

            if (GetDistBetwGB(fM, goal) < 150)
            {
                xna.Vector3 p = SetDegreeFishToP(fM, degree);
                Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], p, 60, 150, 180, 200, 1, 1, 3, 10, 100, true);
                if (Math.Abs(degree - degreeF) <= 15)
                {
                    decision.TCode = 7;
                    decision.VCode = 0;
                }
            }
            else if (Math.Abs(degree - degreeF) <= 15 && GetDistBetwGB(fM, goal) < 200)
            {
                decision.TCode = 7;
                decision.VCode = 0;
            }
            else
            {
                Dribble2(ref decision, mission.TeamsRef[teamId].Fishes[fishId], goal, 9, 20, 60,200, 15, 15, 15, 10, 100, true);
            }

        }
        //7 5 2 200
        #endregion

        #region 判断三点共线
        public static bool JudgeAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            float a_x = a.X;
            float a_z = a.Z;
            float b_x = b.X;
            float b_z = b.Z;
            float c_x = c.X;
            float c_z = c.Z;

            Vector3 ab = new Vector3(b.X - a.X, 0f, b.Z - a.Z);
            Vector3 cb = new Vector3(b.X - c.X, 0f, b.Z - c.Z);

            float abdegree = GetAngleDegree(ab);
            float cddegree = GetAngleDegree(cb);
            if (abdegree == cddegree)
            {
                return true;
            }

            else
            {
                return false;
            }




        }
        #endregion

        #region 判断区域4
        public static bool judgearea4(Vector3 p, int area)
        {
            switch (area)
            {
                case 1:
                    if (p.X <= -200 && p.X > -2150 && p.Z <= -500 && p.Z > -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (p.X >= 500 && p.X < 2150 && p.Z <= -900 && p.Z > -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (p.X <= -200 && p.X > -2150 && p.Z >= -500 && p.Z <= 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (p.X >= 500 && p.X < 2150 && p.Z >= -900 && p.Z <= 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 5:
                    if (p.X < -500 && p.X > -2150 && p.Z >= 0 && p.Z <= 900)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    if (p.X >= 500 && p.X < 2150 && p.Z >= 0 && p.Z <= 500)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 7:
                    if (p.X <= -500 && p.X > -2150 && p.Z >= 900 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 8:
                    if (p.X >= 500 && p.X < 2150 && p.Z >= 500 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 9:
                    if (p.X >= -200 && p.X < 200 && p.Z >= -500 && p.Z < -200)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 10:
                    if (p.X >= -200 && p.X < 200 && p.Z >= 200 && p.Z < 500)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 11:
                    if (p.Z > 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 12:
                    if (p.X > 2150 && p.Z < 1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 13:
                    if (p.X < 2150 && p.Z < -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 14:
                    if (p.X < -2150 && p.Z < 1400 && p.Z > -1400)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
    
        

        #endregion

    }
}
