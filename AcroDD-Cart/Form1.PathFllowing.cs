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
            float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);

            if (!auto_driving_start)
            {
                targetVelocity_vec = Vector3.Zero;
            }
            else
            {            

                targetPosition = selectedPathData[nowIndex];
                diffPosition_vec.X = (float)(targetPosition[0] - nowPosition[0]);
                diffPosition_vec.Y = (float)(targetPosition[1] - nowPosition[1]);
                diffPosition_vec.Z = (float)(targetPosition[2] - nowAngle) * turningRadius;

                if (diffPosition_vec.Length() <= errorRadius)
                {
                    if (nowIndex < selectedPathData.Count - 1)
                        nowIndex++;
                    else
                        isEndPoint = true;


                    System.Console.WriteLine(nowIndex + " "+ diffPosition_vec.Length());
                    return;
                }

                targetUnitVector_vec = Vector3.Normalize(diffPosition_vec);

                if (isEndPoint)
                {
                    auto_driving_start = false;
                    isEndPoint = false;
                    nowIndex = 0;
                }
                else
                {
                    targetVelocity_vec = Vector3.Multiply((float)Constants.MaxVelocity, targetUnitVector_vec);
                }
            }
            targetVelocityFilter_vec = Vector3.Multiply((float)(1.0 - filterConst), targetVelocityFilter_vec)
                + Vector3.Multiply((float)(filterConst), targetVelocity_vec);//ローパスフィルタ

            tagVelo[0] = targetVelocityFilter_vec.X;
            tagVelo[1] = targetVelocityFilter_vec.Y;
            tagAngVelo = targetVelocityFilter_vec.Z / turningRadius;
        }
        private void checkBox_reverse_CheckedChanged(object sender, EventArgs e)
        {
            selectedPathData.Reverse();
            chart_position.Series["Target"].Points.Clear();
            foreach (var item in selectedPathData)
            {
                chart_position.Series["Target"].Points.AddXY(item[1], item[0]);
            }
        }
        bool auto_driving_start = false;

        private void updateButtonStatus_startDriving()
        {
            if (auto_driving_start)
            {
                button_startDriving.Text = "Stop Driving";
            }
            else
            {
                button_startDriving.Text = "Start Driving";
            }
        }
        private void button_startDriving_Click(object sender, EventArgs e)
        {
            if (auto_driving_start)
            {
                auto_driving_start = false;
            }
            else
            {
                auto_driving_start = true;

            }
            updateButtonStatus_startDriving();
        }
    }
}
