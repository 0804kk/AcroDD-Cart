using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Numerics;

namespace AcroDD_Cart
{

    public partial class Form1
    {
        Vector3 diffPosition_vec = new Vector3();
        Vector3 targetUnitVector_vec = new Vector3();
        Vector3 targetVelocity_vec = new Vector3();
        Vector3 targetVelocityFilter_vec = new Vector3();

        public double[] targetPosition = new double[3];//todo

        double errorRadius = 10.0;//[mm]

        int nowIndex = 0;
        double filterConst  = 0.5;
        bool isEndPoint = false;
        public void CalcTargetVelocity(double[] tagVelo, ref double tagAngVelo, double[] nowPosition, double nowAngle)
        {
            List<double[]> pathData = pathDataList[listBox_path.SelectedIndex];

            targetPosition = pathData[nowIndex];
            diffPosition_vec.X = (float)(targetPosition[0] - nowPosition[0]);
            diffPosition_vec.Y = (float)(targetPosition[1] - nowPosition[1]);
            float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);
            diffPosition_vec.Z = (float)(targetPosition[2] - nowAngle) * turningRadius;

            if (diffPosition_vec.Length() <= errorRadius)
            {
                if (nowIndex < pathData.Count - 1)
                    nowIndex++;
                else
                    isEndPoint = true;


                System.Console.WriteLine(nowIndex + " "+ diffPosition_vec.Length());
                return;
            }

            targetUnitVector_vec = Vector3.Normalize(diffPosition_vec);

            if (isEndPoint)
            {
                targetVelocity_vec = Vector3.Zero;
            }
            else
            {
                targetVelocity_vec = Vector3.Multiply((float)Constants.MaxVelocity, targetUnitVector_vec);
            }

            targetVelocityFilter_vec = Vector3.Multiply((float)(1.0 - filterConst), targetVelocityFilter_vec)
                + Vector3.Multiply((float)(filterConst), targetVelocity_vec);//ローパスフィルタ

            tagVelo[0] = targetVelocityFilter_vec.X;
            tagVelo[1] = targetVelocityFilter_vec.Y;
            tagAngVelo = targetVelocityFilter_vec.Z / turningRadius;
        }
        //public void CalcTargetVelocity(double[] tagVelo, ref double tagAngVelo, double[] nowPosition, double nowAngle, double dt)
        //{
        //    targetPosition = pathData[nowIndex];
        //    diffPosition_vec.X = targetPosition[0] - nowPosition[0];
        //    diffPosition_vec.Y = targetPosition[1] - nowPosition[1];


        //    if (diffPosition_vec.Length <= errorRadius)
        //    {
        //        if (nowIndex < pathData.Count - 1)
        //            nowIndex++;
        //        else
        //            isEndPoint = true;


        //        System.Console.WriteLine(nowIndex + " " + diffPosition_vec.Length);
        //        return;
        //    }

        //    diffAngle = targetPosition[2] - nowAngle;
        //    P = diffAngle * pGain;
        //    I += diffAngle * dt * iGain;
        //    if (dt > 0.001)
        //        D = (diffAngle - preDiffAngle) / dt * dGain;
        //    tagAngVelo = P + I + D;
        //    preDiffAngle = diffAngle;

        //    targetUnitVector_vec = diffPosition_vec;
        //    targetUnitVector_vec.Normalize();
        //    if (isEndPoint)
        //    {
        //        targetVelocity_vec.X = 0.0;
        //        targetVelocity_vec.Y = 0.0;
        //    }
        //    else
        //    {
        //        targetVelocity_vec = targetUnitVector_vec * Constants.MaxVelocity;
        //    }

        //    targetVelocityFilter_vec = targetVelocityFilter_vec * (1.0 - filterConst) + targetVelocity_vec * (filterConst);//ローパスフィルタ

        //    tagVelo[0] = targetVelocityFilter_vec.X;
        //    tagVelo[1] = targetVelocityFilter_vec.Y;
        //}
    }
}
