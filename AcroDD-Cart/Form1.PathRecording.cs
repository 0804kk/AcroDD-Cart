using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Numerics;
using System.ComponentModel;


namespace AcroDD_Cart
{
    public partial class Form1
    {
        public List<List<double[]>> pathDataList = new List<List<double[]>>();
        public BindingList<string> pathNameList = new BindingList<string>();
        //public List<double[]> pathData = new List<double[]>();
        //public List<double[]> pathData2 = new List<double[]>();

        public void initPathRecoding()
        {
            listBox_path.DataSource = pathNameList;
        }
        double interval = 10;
        double radius = 500;
        double maxAngle = Math.PI / 6.0;
        public void CreateCircle()
        {
            List<double[]> pathData = new List<double[]>();

            double x = 0;
            double y = 0;
            double theta = 0;
            double max = 2.0 * Math.PI * radius / interval;
            for (int i = 0; i < max + 1; i++)
            {
                x = radius - radius * Math.Cos((double)i / max * 2 * Math.PI);
                y = -radius * Math.Sin((double)i / max * 2 * Math.PI);
                theta = maxAngle * Math.Sin((double)i / max * 2 * Math.PI);
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }
            AddToPathList(pathData,"Circle");
        }
        public void CreateLine()
        {
            List<double[]> pathData = new List<double[]>();
            List<double[]> pathData2 = new List<double[]>();

            double x = 0;
            double y = 0;
            double theta = 0;
            for (int i = 0; i < 2; i++)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
                x = -500;
            }

            pathData2 = ConvertEqualIntervalPath(pathData);
            AddToPathList(pathData2, "Line_x");

        }

        public void CreateEqualIntervalCircle()
        {
            List<double[]> pathData = new List<double[]>();
            List<double[]> pathData2 = new List<double[]>();
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
                    theta = maxAngle * Math.Sin((double)i / max * 2 * Math.PI);
                    //theta = 0.0;

                    double[] data = new double[3] { x, y, theta };

                    pathData.Add(data);
                    maxCount = r.Next(1, 1);
                    pass = true;
                }
            }

            pathData2 = ConvertEqualIntervalPath(pathData);
            AddToPathList(pathData2, "Equal Interval Circle");

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
                System.Console.WriteLine(diff.Length());
                //System.Console.WriteLine(vec.X + "," + vec.Y + "," + vec.Z);
                preVec = vec;
            }
        }
        public List<double[]> ConvertEqualIntervalPath(List<double[]> original)
        {
            Vector3 nowPoint;
            nowPoint.X = (float)original[0][0];
            nowPoint.Y = (float)original[0][1];
            nowPoint.Z = (float)original[0][2];
            int count = 0;
            List<double[]> output = new List<double[]>();
            output.Add(original[0]);
            while (true)
            {
                if (count == original.Count - 1)
                {
                    output.Add(original[count]);
                    break;
                }
                float turningRadius = (float)Math.Sqrt(axisCenterFromRear * axisCenterFromRear + Constants.Wc * Constants.Wc);
                Vector3 intersection = Vector3.Zero;
                Vector3 point1;
                Vector3 point2;
                point1.X = (float)original[count][0];
                point1.Y = (float)original[count][1];
                point1.Z = (float)original[count][2] * turningRadius;

                point2.X = (float)original[count + 1][0];
                point2.Y = (float)original[count + 1][1];
                point2.Z = (float)original[count + 1][2] * turningRadius;
                var ret = calcIntersectionOfSphereAndLine(ref intersection, point1, point2, nowPoint, (float)interval);
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
        private bool calcIntersectionOfCircleAndLine(ref Vector3 intersection, Vector3 point1, Vector3 point2, Vector3 center, float radius)
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
            List<double[]> pathData = new List<double[]>();
            double x = 0;
            double y = 0;
            double theta = 0;
            double max = Math.PI / 2.0 * curveRadius / interval;
            for (x = 0; x < length - curveRadius; x += interval)
            {
                double[] data = new double[3] { x, y, theta };
                pathData.Add(data);
            }
            for (int i = 0; i < max + 1; i++)
            {
                var x_r = x + curveRadius * Math.Sin((double)i / max * Math.PI / 2.0);
                var y_r = y + curveRadius - curveRadius * Math.Cos((double)i / max * Math.PI / 2.0);
                double[] data = new double[3] { x_r, y_r, theta };
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

            AddToPathList(pathData,"Square");

        }

        private void AddToPathList(List<double[]> pathData,string name)
        {
            pathDataList.Add(pathData);
            pathNameList.Add(name);

        }


        List<double[]> recodingPathData = new List<double[]>();
        private void RecordPath()
        {
            if (recoding)
            {
                double[] data = new double[3] { cartPosition[0], cartPosition[1], cartAngle};
                recodingPathData.Add(data);
            }
        }
        bool recoding = false;
        int recodeNum = 1;
        private void button_record_Click(object sender, EventArgs e)
        {
            if (!recoding)
            {
                recoding = true;
                button_record.Text = "Stop Recoding";

            }
            else
            {
                recoding = false;
                button_record.Text = "Start Recoding";
                List<double[]> pathData = new List<double[]>(recodingPathData);
                List<double[]> pathData2 = new List<double[]>();
                pathData2 = ConvertEqualIntervalPath(pathData);
                AddToPathList(pathData2, "RecodedPathData" + recodeNum);
                recodingPathData.Clear();
                recodeNum++;
            }
        }
        List<double[]> selectedPathData = new List<double[]>();

        private void listBox_path_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPathData = pathDataList[listBox_path.SelectedIndex];
            textBox_selectedPath.Text = listBox_path.SelectedItem.ToString();
            chart_position.Series["Target"].Points.Clear();
            foreach (var item in selectedPathData)
            {
                chart_position.Series["Target"].Points.AddXY(item[1], item[0]);

            }

        }
    }
}
