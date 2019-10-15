using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcroDD_Cart
{
    static class Constants //0:left 1:right
    {
        //-----------------------------
        // Device Constants
        //-----------------------------
        public static readonly string[] AioDeviceName = { "AIO001", "AIO002" };//Left, Right
        public static readonly string[] DioDeviceName = { "DIO001", "DIO002" };
        public const int DioInputIndex = 0;//absolute encoder (steer)
        public const int DioOutputIndex = 1;//brake and clutch
        public static readonly byte[] CounterCH = { EPX18QC.EPX18QC_CNT_CH2, EPX18QC.EPX18QC_CNT_CH1 };//Left, Right  incremental encoder (wheel)

        public const short BrakePortNo = 0;
        public const short ClutchPortNo = 2;

        public const uint padIndex = 0;
        //-----------------------------
        // Mechanical Constants
        //-----------------------------
        public static readonly int[] SteerOffset = { 1378, 279 };
        public static readonly int[] PulsePerRotation = { 8192, 4096 };
        public static readonly double[,] VoltageOffset = new double[2, 2]{ { -0.0098, -0.02245},
                                                                           {  0.0134, 0.02142 } };
        public static readonly double[,] MotorDirection = new double[2, 2]{ { -1.0, 1.0},
                                                                            { -1.0, 1.0} };//モータの回転方向とエンコーダの回転方向を合わせるために使う

        public static readonly double[,] GearCorrectionMatrix = { { 1.0/3.0 , 1.0/3.0   }, //エンコーダのギア比とキャスタが回転した時タイヤも一緒に回転するのを考慮するための行列
                                                                  { 0       , 1         }};
        public static readonly double[,] GearCorrectionInverseMatrix = {{ 3.0 , -1.0 },
                                                                        { 0   , 1    }};
        public const double WheelRadius = 62.5;
        public const double CasterOffset = 40.0;
        public const double Lc = 315.0;//後輪中心と台車中心の間の距離
        public const double Wc= 470.0;//後輪中心と後輪の間の距離


        public const double MaxVelocity = 100.0;
        public const double MaxAngularVelocity = 20.0 * (2.0 * Math.PI) / 360.0;

        public static readonly double[] MotorToEncoderGearRatio = { 1.0, 2.0 };//wheel,steer
        public const double VoltagePerRps = 1.0 / (550.0 * 0.2 / 60.0);

        public static readonly double[] EncoderRpspsLimit = { 120.0, 1000 };

        public const int ControllInterval = 50;
    }
}
