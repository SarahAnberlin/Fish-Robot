using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using URWPGSim2D.Core;
using xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;

namespace URWPGSim2D.Strategy
{
     public static class Reangle
    {
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
            {   //埃普瑟隆=Epsilon
                // x = 0 直角反正切不存在
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

        #region dribble_666算法
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
        public static void Dribble_666(ref Decision decision, RoboFish fish, xna.Vector3 destPtMm,
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
            {// 目标角度为正目标方向在鱼体方向右边需要给右转档位
                while ((code < 14) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] < targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调高目标（转弯）档位
                    code = code + 1;
                }
                if ((fish.AngularVelocityRadPs * seconds2) > deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 1;
                }
                if (code > 14)
                    code = 14;
            }
            else
            {// 目标角度为负目标方向在鱼体方向左边需要给左转档位（注意左转档位对应的角速度值为负值）
                while ((code > 0) && (DataBasedOnExperiment.TCodeAndAngularVelocityTable[code] > targetAngularV))
                {// 目标（转弯）档位对应的角速度值尚未达到目标角速度则调低目标（转弯）档位
                    code = code - 1;
                }
                if ((fish.AngularVelocityRadPs * seconds2) < deltaTheta)
                {// 当前角速度值绝对值过大 一次档位切换所需平均时间内能游过的角度超过目标角度
                    // 则给相反方向次大转弯档位
                    code = 13;
                }
                if (code < 0)
                    code = 0;
            }
            decision.TCode = code;
        }
        #endregion

        #region 判断区域
        public static bool Judge(Vector3 p, int area)
        {
            switch (area)
            {
                case 1:
                    if (p.X>=0&&p.X<300&&p.Z<=0&&p.Z>=-350)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (p.X>=-300&&p.X<=0&&p.Z<=0&&p.Z>=-350)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (p.X >= 0 && p.X < 300 && p.Z >= 0 && p.Z <= 350)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (p.X >= -300 && p.X <= 0 && p.Z >= 0 && p.Z <= -350)
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
