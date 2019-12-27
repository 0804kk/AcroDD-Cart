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

        enum AutoRunModeEnum
        {
            Stop,
            RunWayPoint,
            RunEndPoint,
        }
        AutoRunModeEnum AutoRunMode = AutoRunModeEnum.Stop;


        Vector3 diffPosition_vec = new Vector3();
        Vector3 targetUnitVector_vec = new Vector3();
        Vector3 targetVelocity_vec = new Vector3();
        Vector3 targetVelocityFilter_vec = new Vector3();

        public double[] targetPosition = new double[3];//todo



        double errorRadius = 10.0;//[mm]
        double LastErrorRadius = 2.0;//[mm]

        int nowIndex = 0;
        double filterConst  = 0.5;
        bool isEndPoint = false;

        public void CalcTargetVelocity(double[] tagVelo, ref double tagAngVelo, double[] nowPosition, double nowAngle)
        {
            float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);

            if (AutoRunMode == AutoRunModeEnum.Stop)
            {
                targetVelocity_vec = Vector3.Zero;
            }
            else
            {
                targetPosition = selectedPathData[nowIndex];
                diffPosition_vec.X = (float)(targetPosition[0] - nowPosition[0]);
                diffPosition_vec.Y = (float)(targetPosition[1] - nowPosition[1]);
                diffPosition_vec.Z = (float)(targetPosition[2] - nowAngle) * turningRadius;
            
                targetUnitVector_vec = Vector3.Normalize(diffPosition_vec);
                

                if (AutoRunMode == AutoRunModeEnum.RunWayPoint)
                {
                    if (diffPosition_vec.Length() <= errorRadius)
                    {
                        if (nowIndex < selectedPathData.Count - 2)
                            nowIndex++;
                        else
                            AutoRunMode = AutoRunModeEnum.RunEndPoint;//目標が最後の点になったら

                        System.Console.WriteLine(nowIndex + " " + diffPosition_vec.Length());
                        return;
                    }

                    targetVelocity_vec = Vector3.Multiply((float)Constants.MaxVelocity, targetUnitVector_vec);
                    
                }
                else if (AutoRunMode == AutoRunModeEnum.RunEndPoint)
                {
                    targetVelocity_vec = Vector3.Multiply((float)Constants.MaxVelocity * (float)diffPosition_vec.Length() / (float)interval, targetUnitVector_vec);
                    //targetVelocity_vec = Vector3.Multiply((float)Constants.MaxVelocity, targetUnitVector_vec);

                    if (diffPosition_vec.Length() <= LastErrorRadius)
                    {
                        AutoRunMode = AutoRunModeEnum.Stop;
                        isEndPoint = false;
                        nowIndex = 0;
                    }

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

        private void updateButtonStatus_startDriving()
        {
            if (AutoRunMode != AutoRunModeEnum.Stop)
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
            if (AutoRunMode == AutoRunModeEnum.Stop)
            {
                AutoRunMode  = AutoRunModeEnum.RunWayPoint;
            }
            else
            {
                AutoRunMode  = AutoRunModeEnum.Stop;
            }
            updateButtonStatus_startDriving();
        }
    }
}
