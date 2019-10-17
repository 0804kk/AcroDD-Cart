using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcroDD_Cart
{
    public partial class Form1
    {

        private void GetEncoderRawValue()
        {
            //wheel encoder raw value 
            int[] counterValue = new int[2];
            for (int i = 0; i < 2; i++)
            {
                string returnCode;//todo
                var ret = device.GetCounterValue(Constants.CounterCH[i], ref counterValue[i], out returnCode);
                if (ret) {; }//todo
                encoderRawValue[i, wheel] = counterValue[i];
            }

            {//steer encoder raw value
                string returnCode;
                byte[] dioInputData = new byte[8];
                var ret = device.GetDioInputValue(dioInputData, out returnCode);
                if (ret) {; }//todo
                int a = dioInputData[0];
                int b = dioInputData[1];
                int c = dioInputData[2] & 0XF;//left
                int d = (dioInputData[2] & 0XF0) / 16;//right

                encoderRawValue[left, steer] = a + c * 256;
                encoderRawValue[right, steer] = b + d * 256;
            }
        }

        private void CalcRotationFromRawValue(out double rotation, int raw, int pulsePerRotation, int WorS, int steerOffset)
        {
            if (WorS == steer)
            {
                var fixedRaw = raw - steerOffset + (raw >= steerOffset ? 0 : pulsePerRotation);
                rotation = (double)fixedRaw / (double)pulsePerRotation;
            }
            else
            {
                rotation = (double)raw / (double)pulsePerRotation;
            }
        }

        private void CalcEncoderRpsFromEncoderRotation(out double rps, double dt, double rotation, double preRotation, int WorS)
        {
            double diff = rotation - preRotation;
            if (WorS == steer)
            {
                if (diff > 0.7)
                    diff -= 1.0;
                else if (diff < -0.7)
                    diff += 1.0;
            }
            if (dt > 0.0010)
                rps = diff / dt;
            else rps = 0.0;
            //System.Console.WriteLine(rps);
            //System.Console.WriteLine(dt);
        }
        private void CalcCasterOmegaFromEncoderRps(double[,] casterOmega, double[,] encoderRps)//文法上仕方なくLR両方計算　ジャグ配列使えばいける?
        {
            for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
            {
                for (int WorS = 0; WorS < 2; WorS++)
                {
                    casterOmega[LorR, WorS] = (2.0 * Math.PI) * encoderRps[LorR, 0] * Constants.GearCorrectionMatrix[WorS, 0]
                                           + (2.0 * Math.PI) * encoderRps[LorR, 1] * Constants.GearCorrectionMatrix[WorS, 1];
                }
            }
        }

        private void CalcCasterVelocity(double[,] velo, double[,] omega, double[] steerAngle)//文法上仕方なくLR両方計算　ジャグ配列使えばいける?
        {
            for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
            {
                //補正あり
                velo[LorR, wheel] = omega[LorR, wheel] * Constants.WheelRadius * Math.Cos(steerAngle[LorR])
                                  - (omega[LorR, steer] + cartAngularVelocity) * Constants.CasterOffset * Math.Sin(steerAngle[LorR]); //車輪前方向移動速度[mm/s]
                velo[LorR, steer] = omega[LorR, wheel] * Constants.WheelRadius * Math.Sin(steerAngle[LorR])
                                  + (omega[LorR, steer] + cartAngularVelocity) * Constants.CasterOffset * Math.Cos(steerAngle[LorR]); //車輪横方向移動速度[mm/s]

                //補正なし
                //velo[LorR, wheel] = omega[LorR, wheel] * Constants.WheelRadius * Math.Cos(steerAngle[LorR])
                //                  - (omega[LorR, steer] ) * Constants.CasterOffset * Math.Sin(steerAngle[LorR]); //車輪前方向移動速度[mm/s]
                //velo[LorR, steer] = omega[LorR, wheel] * Constants.WheelRadius * Math.Sin(steerAngle[LorR])
                //                  + (omega[LorR, steer] ) * Constants.CasterOffset * Math.Cos(steerAngle[LorR]); //車輪横方向移動速度[mm/s]
            }
        }
        private void CalcCartVelocity(double[] cartVeloRear, out double angularVelo, ref double angle, double dt, double[,] casterVelo)//逆運動学
        {
            double[] cartVeloMachine = new double[2];//台車座標系

            cartVeloMachine[0] = (casterVelo[left, wheel] + casterVelo[right, wheel]) / 2.0;
            cartVeloMachine[1] = (casterVelo[right, wheel] - casterVelo[left, wheel]) * 0.0 / Constants.Wc
                               + (casterVelo[right, steer] + casterVelo[left, steer]) / 2.0;
            angularVelo = (casterVelo[right, wheel] - casterVelo[left, wheel]) / Constants.Wc;

            angle += angularVelo * dt;
            if (angle > Math.PI) angle -= 2 * Math.PI;
            else if (angle <= -Math.PI) angle += 2 * Math.PI;

            //System.Console.WriteLine("{0} {1}", angularVelo, angle);

            cartVeloRear[0] = cartVeloMachine[0] * Math.Cos(angle) - cartVeloMachine[1] * Math.Sin(angle);//車輪中点X方向移動距離(床座標系)
            cartVeloRear[1] = cartVeloMachine[0] * Math.Sin(angle) + cartVeloMachine[1] * Math.Cos(angle);//車輪中点Y方向移動距離(床座標系)   
        }
        private void CalcCartPosition(double[] cartPosRear, double[] cartPosCenter, double[] cartPosFront, double dt, double[] cartVeloRear, double angle)
        {
            for (int i = 0; i < 2; i++)
            {
                cartPosRear[i] += cartVeloRear[i] * dt;
            }

            cartPosCenter[0] = cartPosRear[0] + Constants.Lc * Math.Cos(angle) - Constants.Lc;
            cartPosCenter[1] = cartPosRear[1] + Constants.Lc * Math.Sin(angle);

            cartPosFront[0] = cartPosRear[0] + 2.0 * Constants.Lc * Math.Cos(angle) - 2.0 * Constants.Lc;
            cartPosFront[1] = cartPosRear[1] + 2.0 * Constants.Lc * Math.Sin(angle);


            for (int i = 0; i < 2; i++)
            {
                switch (origin)
                {
                    case OriginEnum.Front:
                        cartPosition[i] = cartPositionFront[i];
                        break;
                    case OriginEnum.Center:
                        cartPosition[i] = cartPositionCenter[i];
                        break;
                    case OriginEnum.Rear:
                        cartPosition[i] = cartPositionRear[i];
                        break;
                    default:
                        break;
                }
            }
        }


        private void GetTargetCartVelocityByJoypad()
        {
            targetCartVelocity[0] = joyValueFilter.X * Constants.MaxVelocity;
            targetCartVelocity[1] = joyValueFilter.Y * Constants.MaxVelocity;
            targetCartAngularVelocity = joyValueFilterZ * Constants.MaxAngularVelocity;
            //targetCasterOmega[0, 0] = joyValueFilter.X * Math.PI / 6.0;
            //targetCasterOmega[0, 1] = joyValueFilter.Y * Math.PI / 6.0;
            //targetEncoderRps[0, 0] = joyValueFilter.X * Math.PI / 6.0;
            //targetEncoderRps[0, 1] = joyValueFilter.Y * Math.PI / 6.0;
        }
        private void CalcCasterVelocityFromCartVelocity(double[,] caster, double[] cart, double angularVelo, double angle)//順運動学
        {
            double[] cartVeloMachine = new double[2];//台車座標系
            cartVeloMachine[0] = cart[0] * Math.Cos(angle) + cart[1] * Math.Sin(angle);
            cartVeloMachine[1] = -cart[0] * Math.Sin(angle) + cart[1] * Math.Cos(angle);

            caster[0, 0] = cartVeloMachine[0] - Constants.Wc / 2.0 * angularVelo;
            caster[1, 0] = cartVeloMachine[0] + Constants.Wc / 2.0 * angularVelo;
            caster[0, 1] = cartVeloMachine[1] - axisCenterFromRear * angularVelo;
            caster[1, 1] = cartVeloMachine[1] - axisCenterFromRear * angularVelo;
        }
        private void CalcCasterOmegaFromCasterVelocity(double[,] omega, double[,] velo, double[] steerAngle)//文法上仕方なくLR両方計算　ジャグ配列使えばいける?
        {
            //System.Console.WriteLine(steerAngle[0] *360.0 / (2.0 * Math.PI) + " " + steerAngle[1] * 360.0 / (2.0 * Math.PI));
            for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
            {
                omega[LorR, wheel] = velo[LorR, wheel] / Constants.WheelRadius * Math.Cos(steerAngle[LorR])
                                  + velo[LorR, steer] / Constants.WheelRadius * Math.Sin(steerAngle[LorR]); //車輪前方向移動速度[mm/s] 
                omega[LorR, steer] = -(velo[LorR, wheel] / Constants.CasterOffset) * Math.Sin(steerAngle[LorR])
                                  + (velo[LorR, steer] / Constants.CasterOffset) * Math.Cos(steerAngle[LorR])
                                  - targetCartAngularVelocity;//補正項
            }
        }

        private void CalcEncoderRpsFromCasterOmega(double[,] encoderRps, double[,] casterOmega)
        {
            for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
            {
                for (int WorS = 0; WorS < 2; WorS++)
                {
                    encoderRps[LorR, WorS] = casterOmega[LorR, wheel] / (2.0 * Math.PI) * Constants.GearCorrectionInverseMatrix[WorS, 0]
                                           + casterOmega[LorR, steer] / (2.0 * Math.PI) * Constants.GearCorrectionInverseMatrix[WorS, 1];
                }
            }
        }
        private void CalcMotorRpsFromEncoderRps(out double motor, double encoder, int WorS)
        {
            motor = Constants.MotorToEncoderGearRatio[WorS] * encoder;
        }

        private void CalcMotorVoltageFromMotorRps(out double volt, double rps)
        {
            volt = Constants.VoltagePerRps * rps;
        }

        async private void ApplyVoltageToMotor(double[,] voltage)
        {
            for (int LorR = 0; LorR < 2; LorR++)
            {
                System.Console.Write("LorR:" + LorR + " ");
                float[] output = new float[2];
                for (int WorS = 0; WorS < 2; WorS++)
                {
                    voltage[LorR, WorS] *= Constants.MotorDirection[LorR, WorS];
                    if (Math.Abs(voltage[LorR, WorS]) < 0.01)
                    {
                        voltage[LorR, WorS] = 0;
                        device.SetAioOutputDoBit(LorR, (short)(0 + 2 * WorS), 0);
                        device.SetAioOutputDoBit(LorR, (short)(1 + 2 * WorS), 0);
                    }
                    else if (voltage[LorR, WorS] > 0)
                    {
                        device.SetAioOutputDoBit(LorR, (short)(0 + 2 * WorS), 1);
                        device.SetAioOutputDoBit(LorR, (short)(1 + 2 * WorS), 0);
                    }
                    else if (voltage[LorR, WorS] < 0)
                    {
                        device.SetAioOutputDoBit(LorR, (short)(0 + 2 * WorS), 0);
                        device.SetAioOutputDoBit(LorR, (short)(1 + 2 * WorS), 1);
                    }


                    output[WorS] = (float)Math.Abs(voltage[LorR, WorS])
                    //;
                    - (float)Constants.VoltageOffset[LorR, WorS];
                }

                await device.SetAioOutputVoltage(LorR, output);

            }
            System.Console.WriteLine("");

        }
        private void CalcIdealPosition(double[] idealPosition, ref double idealAngle, double[] targetVelo, double targetAngleVelo, double dt)
        {
            for (int i = 0; i < 2; i++)
            {
                idealPosition[i] += targetVelo[i] * dt;
            }
            idealAngle += targetAngleVelo * dt;
        }
        private void switchClutch(bool connect)
        {
            byte output;
            if (connect)
                output = 15;
            else
                output = 0;

            if (!device.SetDioOutputByte(Constants.ClutchPortNo, output))
            {
                System.Console.WriteLine("clutch error");
                //todo: エラー処理
            }
        }
        private void switchBrake(bool enable)
        {
            byte output;
            if (enable)
                output = 0;
            else
                output = 15;

            if (!device.SetDioOutputByte(Constants.BrakePortNo, output))
            {
                System.Console.WriteLine("brake error");
                //todo: エラー処理
            }
        }

    }
}
