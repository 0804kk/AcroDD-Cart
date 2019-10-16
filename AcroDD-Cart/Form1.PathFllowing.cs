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
        public List<double[]> pathData2 = new List<double[]>();

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
        public List<double[]> ConvertEqualIntervalPath(List<double[]> original)
        {
            Vector3 nowPoint;
            nowPoint.X = (float)pathData[0][0];
            nowPoint.Y = (float)pathData[0][1];
            nowPoint.Z = (float)pathData[0][2];

            int count = 0;
            List<double[]> output = new List<double[]>();
            while (true)
            {
                if (count == pathData.Count - 1)
                {
                    output.Add(original[count]);
                    break;
                }
                float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);
                Vector3 intersection = Vector3.Zero;
                Vector3 point1;
                Vector3 point2;
                point1.X = (float)pathData[count][0];
                point1.Y = (float)pathData[count][1];
                point1.Z = (float)pathData[count][2] * turningRadius;

                point2.X = (float)pathData[count + 1][0];
                point2.Y = (float)pathData[count + 1][1];
                point2.Z = (float)pathData[count + 1][2] * turningRadius;
                var ret = calcIntersectionOfSphereAndLine(ref intersection, point1, point2, nowPoint, (float)20);
                if (ret)
                {
                    double[] data = new double[3] { intersection.X, intersection.Y, intersection.Z / turningRadius };
                    output.Add(data);
                    nowPoint = intersection;
                }
                else
                {
                    count++;
                }
            }
            return output;
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
            //x,yが等間隔ではない円軌道を生成
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
                    theta = maxAngle * Math.Sin((double)i / max * 4 * Math.PI);

                    double[] data = new double[3] { x, y, theta };

                    pathData.Add(data);
                    maxCount = r.Next(1, 1);
                    pass = true;
                }
            }

            pathData2 = ConvertEqualIntervalPath(pathData);

            System.Console.WriteLine("pathData1");

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

            System.Console.WriteLine("pathData2");
            for (int i = 0; i < pathData2.Count; i++)
            {
                var a = pathData2[i];
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
        //線分と球の交点を返す（2点あるのが返すのはpoint2に近い方）交わらない場合はfalse
        private bool calcIntersectionOfSphereAndLine(ref Vector3 intersection, Vector3 point1, Vector3 point2, Vector3 center, float radius)
        {
            Vector3 e;
            e = point2 - point1;
            e = Vector3.Normalize(e);
            float A = (point1.X - center.X);
            float B = (point1.Y - center.Y);
            float C = (point1.Z - center.Z);
            //at^2+bt+c=0
            float a = e.X * e.X + e.Y * e.Y + e.Z * e.Z;
            float b = 2 * (A * e.X + B * e.Y + C * e.Z);
            float c = A * A + B * B + C * C - radius * radius;
            
            var D = b * b - 4 * a * c;
            float t1, t2;
            /* 判別式に数値による条件分岐 */
            if (D >= 0)
            {
                // 解の計算
                t1 = (-b + (float)Math.Sqrt(D)) / (2 * a);
                t2 = (-b - (float)Math.Sqrt(D)) / (2 * a);
            }
            else
            {
                return false;
            }
            Vector3 inter1 = point1 + Vector3.Multiply(t1, e);//point2側
            Vector3 inter2 = point1 + Vector3.Multiply(t2, e);//point1側

            Vector3 up;
            Vector3 down;
            if (point1.X > point2.X)
            {
                up.X = point1.X;
                down.X = point2.X;
            }
            else
            {
                up.X = point2.X;
                down.X = point1.X;
            }
            if (point1.Y > point2.Y)
            {
                up.Y = point1.Y;
                down.Y = point2.Y;
            }
            else
            {
                up.Y = point2.Y;
                down.Y = point1.Y;
            }
            if (point1.Z > point2.Z)
            {
                up.Z = point1.Z;
                down.Z = point2.Z;
            }
            else
            {
                up.Z = point2.Z;
                down.Z = point1.Z;
            }

            if ((down.X <= inter1.X) && (inter1.X <= up.X) && (down.Y <= inter1.Y) && (inter1.Y <= up.Y) && (down.Z <= inter1.Z) && (inter1.Z <= up.Z))
                //交点が線分の範囲内に入っているか
            {
                intersection = inter1;
                return true;
            }
            else return false;
        }
        //線分と円の交点を返す（2点あるのが返すのはpoint2に近い方）交わらない場合はfalse
        private bool calcIntersectionOfCurcleAndLine(ref Vector3 intersection, Vector3 point1, Vector3 point2,Vector3 center,float radius)
        {
            //http://godfoot.world.coocan.jp/circle-line.htm
            //ax+by+c=0
            float a = (point2.Y - point1.Y);
            float b = (point1.X - point2.X);
            float c = -(a * point1.X + b * point1.Y);
            float L = (float)Math.Sqrt(Math.Pow(b, 2) + Math.Pow(a, 2));
            Vector3 e;
            e.X = -b / L;
            e.Y = a / L;
            Vector3 v;
            v.X = -e.Y;
            v.Y = e.X;
            float k = -(a * center.X + b * center.Y + c) / (a * v.X + b * v.Y);
            Vector3 H;
            H.X = center.X + k * v.X;
            H.Y = center.Y + k * v.Y;
            if (radius < k) return false;//直線と円は交わらない
            float S = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(k, 2));
            Vector3 inter1 = Vector3.Zero;//point2側
            Vector3 inter2 = Vector3.Zero;//point1側
            inter1.X = H.X + S * e.X;
            inter1.Y = H.Y + S * e.Y;
            inter2.X = H.X - S * e.X;
            inter2.Y = H.Y - S * e.Y;
            //System.Console.WriteLine(inter1.X + "," + inter1.Y);
            //System.Console.WriteLine(inter2.X + "," + inter2.Y);

            float upX, upY, downX, downY;
            if (point1.X > point2.X)
            {
                upX = point1.X;
                downX = point2.X;
            }
            else
            {
                upX = point2.X; 
                downX = point1.X;
            }
            if (point1.Y > point2.Y)
            {
                upY = point1.Y;
                downY = point2.Y;
            }
            else
            {
                upY = point2.Y;
                downY = point1.Y;
            }

            if ((downX <= inter1.X) && (inter1.X <= upX) && (downY <= inter1.Y) && (inter1.Y <= upY)) //交点が線分の範囲内に入っているか
            {
                intersection = inter1;
                return true;
            }
            else return false;
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
