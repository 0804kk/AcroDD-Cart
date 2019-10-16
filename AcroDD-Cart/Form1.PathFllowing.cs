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


        public List<double[]> pathData = new List<double[]>();

        double errorRadius = 10.0;//[mm]
        double interval = 10;

        double radius = 500;
        double maxAngle =  Math.PI / 6.0;
        public void CreateCircle()
        {
            double x = 0;
            double y = 0;
            double theta = 0;
            double max = 2.0 * Math.PI * radius / interval;
            for (int i = 0; i < max+1; i++)
            {
                x = radius - radius * Math.Cos((double)i / max * 2 * Math.PI);
                y = -radius * Math.Sin((double)i / max * 2 * Math.PI);
                theta = maxAngle* Math.Sin((double)i / max * 4 * Math.PI);
                double[] data = new double[3]{ x, y, theta };
                pathData.Add(data);

            }

            Vector3 preVec = Vector3.Zero;
            for (int i = 0; i < pathData.Count; i++)
            {
                var a = pathData[i];
                Vector3 vec;
                float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);

                vec.X = (float)a[0];
                vec.Y = (float)a[1];
                vec.Z = (float)a[2]*turningRadius;
                var diff = vec - preVec;
                //System.Console.WriteLine(diff.Length());
                System.Console.WriteLine(vec.X+","+vec.Y+","+vec.Z);
                preVec = vec;

            }
        }
        public void CreateCircleRandom()
        {
            double x = 0;
            double y = 0;
            double theta = 0;
            double max = 2.0 * Math.PI * radius / interval;
            System.Random r = new System.Random(1000);
            bool pass = false;
            int passCount = 0;
            int maxCount = 0;
            for (int i = 0; i < max + 1; i++)
            {
                if (pass)
                {
                    passCount++;
                    if (passCount == maxCount)
                    {
                        pass = false;
                        passCount = 0;
                    }
                }
                else
                {
                    x = radius - radius * Math.Cos((double)i / max * 2 * Math.PI);
                    y = -radius * Math.Sin((double)i / max * 2 * Math.PI);
                    double[] data = new double[3] { x, y, theta };
                    pathData.Add(data);
                    maxCount = r.Next(1, 10);
                    pass = true;
                }


            }

            Vector3 preVec = Vector3.Zero;
            for (int i = 0; i < pathData.Count; i++)
            {
                var a = pathData[i];
                Vector3 vec;
                float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);

                vec.X = (float)a[0];
                vec.Y = (float)a[1];
                vec.Z = (float)a[2] * turningRadius;
                var diff = vec - preVec;
                //System.Console.WriteLine(diff.Length());
                System.Console.WriteLine(vec.X + "," + vec.Y + "," + vec.Z);
                preVec = vec;

            }
        }
        double length = 500;
        double curveRadius = 100;
        public void CreateSquare()
        {
            double x = 0;
            double y = 0;
            double theta = 0;
            double max = Math.PI / 2.0 * curveRadius / interval;
            for (x = 0; x < length - curveRadius; x+=interval)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }
            for (int i = 0; i < max + 1; i++)
            {
                var x_r = x + curveRadius * Math.Sin((double)i / max * Math.PI / 2.0);
                var y_r = y + curveRadius - curveRadius * Math.Cos((double)i / max * Math.PI / 2.0);
                double[] data = new double[3] { x_r ,y_r, theta };
                pathData.Add(data);
            }

            x = length;
            for (y = curveRadius; y < length - curveRadius; y += interval)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }
            for (int i = 0; i < max + 1; i++)
            {
                var x_r = x - curveRadius + curveRadius * Math.Cos((double)i / max * Math.PI / 2.0);
                var y_r = y + curveRadius * Math.Sin((double)i / max * Math.PI / 2.0);
                double[] data = new double[3] { x_r, y_r, theta };
                pathData.Add(data);
            }

            y = length;
            for (x = length - curveRadius; x > curveRadius; x -= interval)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }
            for (int i = 0; i < max + 1; i++)
            {
                var x_r = x - curveRadius * Math.Sin((double)i / max * Math.PI / 2.0);
                var y_r = y - curveRadius + curveRadius * Math.Cos((double)i / max * Math.PI / 2.0);
                double[] data = new double[3] { x_r, y_r, theta };
                pathData.Add(data);
            }

            x = 0;
            for (y = length - curveRadius; y > 0; y -= interval)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }

        }


        int nowIndex = 0;
        double filterConst  = 0.5;
        bool isEndPoint = false;
        public void CalcTargetVelocity(double[] tagVelo, ref double tagAngVelo, double[] nowPosition, double nowAngle)
        {
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
