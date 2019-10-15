using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AcroDD_Cart
{
    class CustomChart
    {
        public int maxHistory = 0;
        Queue<int> countHistory;
        public void initChart(Chart chart, Queue<int> _countHistory, int _maxHistory)
        {
            maxHistory = _maxHistory;
            countHistory = _countHistory;

            // チャート全体の背景色を設定
            chart.BackColor = Color.Black;
            chart.ChartAreas[0].BackColor = Color.Transparent;

            // チャート表示エリア周囲の余白をカットする
            chart.ChartAreas[0].InnerPlotPosition.Auto = false;
            chart.ChartAreas[0].InnerPlotPosition.Width = 100; // 100%
            chart.ChartAreas[0].InnerPlotPosition.Height = 90;  // 90%(横軸のメモリラベル印字分の余裕を設ける)
            chart.ChartAreas[0].InnerPlotPosition.X = 8;
            chart.ChartAreas[0].InnerPlotPosition.Y = 0;


            // X,Y軸情報のセット関数を定義
            Action<Axis> setAxis = (axisInfo) => {
                // 軸のメモリラベルのフォントサイズ上限値を制限
                axisInfo.LabelAutoFitMaxFontSize = 8;

                // 軸のメモリラベルの文字色をセット
                axisInfo.LabelStyle.ForeColor = Color.White;

                // 軸タイトルの文字色をセット(今回はTitle未使用なので関係ないが...)
                axisInfo.TitleForeColor = Color.White;

                // 軸の色をセット
                axisInfo.MajorGrid.Enabled = true;
                axisInfo.MajorGrid.LineColor = ColorTranslator.FromHtml("#008242");
                axisInfo.MinorGrid.Enabled = false;
                axisInfo.MinorGrid.LineColor = ColorTranslator.FromHtml("#008242");
            };

            // X,Y軸の表示方法を定義
            setAxis(chart.ChartAreas[0].AxisY);
            setAxis(chart.ChartAreas[0].AxisX);
            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisY.Maximum = 100;    // 縦軸の最大値を100にする

            chart.AntiAliasing = AntiAliasingStyles.None;

            // 折れ線グラフとして表示
            chart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            // 線の色を指定
            chart.Series[0].Color = ColorTranslator.FromHtml("#00FF00");

            // 凡例を非表示,各値に数値を表示しない
            chart.Series[0].IsVisibleInLegend = false;
            chart.Series[0].IsValueShownAsLabel = false;

            // チャートに表示させる値の履歴を全て0クリア
            while (countHistory.Count <= maxHistory)
            {
                countHistory.Enqueue(0);
            }
        }
        //***************************************************************************
        /// <summary> チャートを描画する
        /// </summary>
        /// <param name="chart"></param>
        //***************************************************************************
        public void showChart(Chart chart)
        {

            //-----------------------
            // チャートに値をセット
            //-----------------------
            chart.Series[0].Points.Clear();
            foreach (int value in countHistory)
            {

                // データをチャートに追加
                chart.Series[0].Points.Add(new DataPoint(0, value));
            }
        }
    }
}
