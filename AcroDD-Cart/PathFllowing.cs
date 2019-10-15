using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AcroDD_Cart
{
    class PathFllowing
    {
        Vector diffPosition_vec = new Vector();
        Vector targetUnitVector_vec = new Vector();
        Vector targetVelocity_vec = new Vector();
        Vector targetVelocityFilter_vec = new Vector();
        double diffAngle;
        double preDiffAngle;
        public double[] targetPosition = new double[3];//todo

        double P = 0.0;
        double I = 0.0;
        double D = 0.0;

        //double[] tagVelo = new double[2];
        //double[] tagTagVelo = new double[2];
        //double[] diffPosition = new double[2];
        //double[] preDiffPosition = new double[2];
        //double[] P = new double[2];
        //double[] I = new double[2];
        //double[] D = new double[2];
        double pGain = 3.0;
        double iGain = 1.0;
        double dGain = 0.0;
        //double acc  = 100.0;
        //double[] targetVelocity = new double[2];

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
                theta = maxAngle* Math.Sin((double)i / max * 2 * Math.PI);
                double[] data = new double[3]{ x, y, theta };
                pathData.Add(data);

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
        public void CalcTargetVelocity(double[] tagVelo, ref double tagAngVelo, double[] nowPosition, double nowAngle, double dt)
        {
            targetPosition = pathData[nowIndex];
            diffPosition_vec.X = targetPosition[0] - nowPosition[0];
            diffPosition_vec.Y = targetPosition[1] - nowPosition[1];


            if (diffPosition_vec.Length <= errorRadius)
            {
                if (nowIndex < pathData.Count - 1)
                    nowIndex++;
                else
                    isEndPoint = true;


                System.Console.WriteLine(nowIndex + " "+ diffPosition_vec.Length);
                return;
            }

            diffAngle = targetPosition[2] - nowAngle;
            P = diffAngle * pGain;
            I += diffAngle * dt * iGain;
            if(dt>0.001)
                D = (diffAngle - preDiffAngle) / dt * dGain;
            tagAngVelo = P + I + D;
            preDiffAngle = diffAngle;

            targetUnitVector_vec = diffPosition_vec;
            targetUnitVector_vec.Normalize();
            if (isEndPoint)
            {
                targetVelocity_vec.X = 0.0;
                targetVelocity_vec.Y = 0.0;
            }
            else
            {
                targetVelocity_vec = targetUnitVector_vec * Constants.MaxVelocity;
            }

            targetVelocityFilter_vec = targetVelocityFilter_vec * (1.0 - filterConst) + targetVelocity_vec * (filterConst);//ローパスフィルタ

            tagVelo[0] = targetVelocityFilter_vec.X;
            tagVelo[1] = targetVelocityFilter_vec.Y;
        }

    }
}
