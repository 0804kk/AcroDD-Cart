using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AcroDD_Cart
{
    public partial class Form1
    {
        Chart[] voltageCharts = new Chart[4];
        private void InitGraph()
        {
            voltageCharts[0] = chart5;
            voltageCharts[1] = chart6;
            voltageCharts[2] = chart7;
            voltageCharts[3] = chart8;

            InitTimeChart(voltageCharts[0], "Now", "Target", "Left Wheel Voltage");
            InitTimeChart(voltageCharts[1], "Now", "Target", "Left Steer Voltage");
            InitTimeChart(voltageCharts[2], "Now", "Target", "Right Wheel Voltage");
            InitTimeChart(voltageCharts[3], "Now", "Target", "Right Steer Voltage");

            InitTimeChart(chart1, "Now", "Target", "Angle");
            chart1.ChartAreas[0].AxisY.Title = "θ [rad]";
            //InitTimeChart(chart2, "Wheel", "Steer");

            //InitTimeChart(chart3,"Left","Right");

            InitTimeChart(chart_dt, "Delta Time", "", "Delta Time");
            chart_dt.Legends.Clear();

            InitPositionChart(chart_position);
        }

        private void InitTimeChart(Chart chart, string series1, string series2 = "", string title = "")
        {
            chart.Titles.Add(new Title(title));
            //chart.Legends.Clear();
            chart.Series.Clear();
            chart.Series.Add(series1);
            if (series2 != "")
                chart.Series.Add(series2);
            for (int i = 0; i < chart.Series.Count; i++)
            {
                chart.Series[i].ChartType = SeriesChartType.Point;
                chart.Series[i].MarkerStyle = MarkerStyle.Circle;
            }
            chart.Series[0].Color = Color.DodgerBlue;
            if (series2 != "")
                chart.Series[1].Color = Color.IndianRed;
        }
        private void InitPositionChart(Chart chart)
        {
            chart.Titles.Add(new Title("Position"));
            chart.Legends.Clear();
            chart.Series.Clear();
            chart.Series.Add("Estimated");
            chart.Series.Add("Target");
            chart.ChartAreas[0].AxisX.IsReversed = true;
            chart.ChartAreas[0].AxisX.Title = "Y [mm]";
            chart.ChartAreas[0].AxisY.Title = "X [mm]";

            //chart.Legends[0].MaximumAutoSize = 28;
            //chart.Legends[0].IsDockedInsideChartArea = true;
            //chart.Legends[0].Position = new ElementPosition(70,10,30,20);
            //= ChartPositionType.Bottom;

            for (int i = 0; i < chart.Series.Count; i++)
            {
                chart.Series[i].MarkerStyle = MarkerStyle.Circle;
                chart.Series[i].ChartType = SeriesChartType.Point;
                chart.Series[i].MarkerSize = 4;
            }
            chart.Series[0].Color = Color.DodgerBlue;
            chart.Series[1].Color = Color.IndianRed;
        }


        double[] min = new double[2] { -30, -30 };
        double[] max = new double[2] { 30, 30 };
        private void PlotGraph()
        {
            if (cnt % 1 == 0)
            {
                for (int LorR = 0; LorR < 2; LorR++)
                {
                    for (int WorS = 0; WorS < 2; WorS++)
                    {
                        //voltageCharts[LorR * 2 + WorS].Series["Target"].Points.AddXY(time, casterVelocity[0, 1]);
                        //voltageCharts[LorR * 2 + WorS].Series["Now"].Points.AddXY(time, casterVelocity[1, 1]);

                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    voltageCharts[i].Series["Target"].Points.AddXY(time, targetCartVelocity[i]);
                    voltageCharts[i].Series["Now"].Points.AddXY(time, cartVelocityCenter[i]);
                }
                voltageCharts[2].Series["Target"].Points.AddXY(time, targetCartAngularVelocity);
                voltageCharts[2].Series["Now"].Points.AddXY(time, cartAngularVelocity);

                voltageCharts[3].Series["Target"].Points.AddXY(time, cartAngularVelocity);
                voltageCharts[3].Series["Now"].Points.AddXY(time, cartAngularVelocityByResitrantCondition2);

                chart1.Series["Target"].Points.AddXY(time, targetPosition[2]);
                chart1.Series["Now"].Points.AddXY(time, cartAngle);

                //chart2.Series["Wheel"].Points.AddXY(time, targetCasterOmega[0, 0]);
                //chart2.Series["Steer"].Points.AddXY(time, targetCasterOmega[0, 1]);

                //chart3.Series["Left"].Points.AddXY(time, targetMotorVoltage[0, 0]);
                //chart3.Series["Right"].Points.AddXY(time, targetMotorVoltage[1, 0]);

                chart_dt.Series["Delta Time"].Points.AddXY(time, dt);
            }
            if (cnt % 1 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (min[i] > cartPosition[i]) min[i] = cartPosition[i];
                    if (max[i] < cartPosition[i]) max[i] = cartPosition[i];
                }
                //System.Console.WriteLine("{0} {1} {2} ", max[0], min[0], cartPosition[0]);
                chart_position.Series[0].Points.AddXY(cartPosition[1], cartPosition[0]);
                //chart_position.Series[0].Points.AddXY(IdealCartPosition[1], IdealCartPosition[0]);
                chart_position.Series[1].Points.AddXY(targetPosition[1], targetPosition[0]);
                //if (cnt > 5)
                {
                    chart_position.ChartAreas[0].AxisX.Maximum = Math.Floor(max[1]);
                    chart_position.ChartAreas[0].AxisX.Minimum = Math.Floor(min[1]);
                    chart_position.ChartAreas[0].AxisY.Maximum = Math.Floor(max[0]);
                    chart_position.ChartAreas[0].AxisY.Minimum = Math.Floor(min[0]);

                }
            }

            var pointCount = chart1.Series[0].Points.Count;
            int maxCount = 30;
            if (pointCount >= maxCount)
            {
                var timeAxisMinimum = Math.Floor(chart1.Series[0].Points[pointCount - maxCount].XValue * 100) / 100.0;
                for (int i = 0; i < 4; i++)
                {
                    voltageCharts[i].ChartAreas[0].AxisX.Minimum = timeAxisMinimum;
                }
                chart1.ChartAreas[0].AxisX.Minimum = timeAxisMinimum;
                //chart2.ChartAreas[0].AxisX.Minimum = timeAxisMinimum;
                //chart3.ChartAreas[0].AxisX.Minimum = timeAxisMinimum;
                chart_dt.ChartAreas[0].AxisX.Minimum = timeAxisMinimum;
                double max_dt = 0.0;
                for (int i = pointCount - maxCount; i < chart_dt.Series[0].Points.Count; i++)
                {
                    var tmp = chart_dt.Series[0].Points[i].YValues[0];
                    if (max_dt < tmp)
                    {
                        max_dt = tmp;
                    }
                }
                chart_dt.ChartAreas[0].AxisY.Maximum = max_dt + 0.01;
            }
            if (pointCount == 1)
            {
                var timeAxisStartPoint = chart1.Series[0].Points[0].XValue;
                for (int i = 0; i < 4; i++)
                {
                    voltageCharts[i].ChartAreas[0].AxisX.Minimum = timeAxisStartPoint;
                }
                chart1.ChartAreas[0].AxisX.Minimum = timeAxisStartPoint;
                //chart2.ChartAreas[0].AxisX.Minimum = timeAxisStartPoint;
                //chart3.ChartAreas[0].AxisX.Minimum = timeAxisStartPoint;
                chart_dt.ChartAreas[0].AxisX.Minimum = timeAxisStartPoint;
            }

            if (chart_position.Series[0].Points.Count >= 700)
            {
                chart_position.Series[0].Points.RemoveAt(0);
                chart_position.Series[1].Points.RemoveAt(0);
                //chart_position.Series[0].Points.Clear();
            }

        }
        private void Button_reset_Click(object sender, EventArgs e)
        {
            //chart1.Series["left"].Points.Clear();
            //chart1.Series["right"].Points.Clear();

            //chart2.Series["left"].Points.Clear();
            //chart2.Series["right"].Points.Clear();

            chart_position.Series[0].Points.Clear();
            chart_position.Series[1].Points.Clear();

            //chart_dt.Series[0].Points.Clear();
        }
    }
}
