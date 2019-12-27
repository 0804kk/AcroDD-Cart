using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Timers;



namespace AcroDD_Cart
{
    public partial class Form1 : Form
    {
        Device device = new Device();

        enum ModeEnum
        {
            ManualMode,
            JoypadMode,
            AutoMode,
        }
        ModeEnum mode = ModeEnum.ManualMode;

        enum OriginEnum
        {
            Front,
            Center,
            Rear,
        }
        OriginEnum origin = OriginEnum.Rear;

        TextBox[] textBox_dio = new TextBox[2];
        TextBox[] textBox_aio = new TextBox[2];
        TextBox[] textBox_counter = new TextBox[3];

        const int left = 0;
        const int right = 1;
        const int wheel = 0;
        const int steer = 1;

        // 2x2行列
        //①＼②    0         1
        // 0 [wheel_L, steer_L]
        // 1 [wheel_R, steer_R]
        int[,] encoderRawValue = new int[2,2];//エンコーダの生値
        double[,] encoderRotation = new double[2,2];//エンコーダ軸の回転数
        double[,] preEncoderRotation = new double[2,2];//1制御周期前のエンコーダ軸の回転数[周]
        double[,] encoderRps = new double[2,2];//エンコーダ軸の回転速度[rps]
        double[,] casterOmega = new double[2,2];//ギアの関係を考慮したキャスタの現在回転速度[rad/s]

        double[,] targetCasterOmega = new double[2,2];//ギアの関係を考慮したキャスタの目標回転速度[rad/s]
        double[,] targetEncoderRps = new double[2,2];//エンコーダ軸の目標回転速度[rps]
        double[,] preTargetEncoderRps = new double[2,2];//エンコーダ軸の目標回転速度[rps]
        double[,] targetEncoderRpsps = new double[2,2];//エンコーダ軸の目標回転速度[rps/s]
        double[,] targetMotorRps = new double[2,2];//モータ軸の目標回転速度[rps]
        double[,] targetMotorVoltage = new double[2,2];//モータ軸の出力電圧[V](-10 ～ +10 V) 最終的には(0 ～ +10 V)

        double[,] virtualEncoderRps = new double[2,2];//エンコーダ軸のシミュレーション用回転速度[rps]

        double[] steerAngle = new double[2];//[rad]
        double[] steerAngleDeg = new double[2];//[deg]

        double[] targetCartVelocity = new double[2];//[mm/s, mm/s]
        double targetCartAngularVelocity = 0.0;//[rad/s]
        double[,] targetCasterVelocity = new double[2,2];//[mm/s]

        double[,] casterVelocity = new double[2,2];//[mm/s]
        //double[] casterPosition = new double[2];
        double[] cartVelocityRear = new double[2];//[mm/s, mm/s]
        double[] cartVelocityCenter = new double[2];//[mm/s, mm/s]
        double[] cartVelocityFront = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityRearTan = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityCenterTan = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityFrontTan = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityRearSin = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityCenterSin = new double[2];//[mm/s, mm/s]
        //double[] cartVelocityFrontSin = new double[2];//[mm/s, mm/s]

        double cartAngularVelocity = 0.0;//[rad/s]一周期前のやつを使わない（近似使用）
        //double cartAngularVelocityTan = 0.0;//[rad/s]一周期前のやつを使わない（近似使用）（tan）
        //double cartAngularVelocitySin = 0.0;//[rad/s]一周期前のやつを使わない（近似使用）（sin）

        double cartAngularVelocityDeg = 0.0;//[deg/s]

        double cartAngle = 0.0;//[rad]
        double cartAngleDeg = 0.0;//[deg]
        double cartAngleTan  = 0.0;//[rad]
        double cartAngleSin  = 0.0;//[rad]
        double[] cartPosition = new double[2];//台車の座標[mm, mm, rad]
        double[] cartPositionRear = new double[2] { 0.0, 0.0};//台車後輪の座標[mm, mm, rad]
        double[] cartPositionCenter = new double[2];//台車中心の座標[mm, mm, rad]
        double[] cartPositionFront = new double[2];//台前後輪の座標[mm, mm, rad]
        //double[] cartPositionRearTan = new double[2] { 0.0, 0.0 };//台車後輪の座標[mm, mm, rad]
        //double[] cartPositionCenterTan = new double[2];//台車中心の座標[mm, mm, rad]
        //double[] cartPositionFrontTan = new double[2];//台前後輪の座標[mm, mm, rad]
        //double[] cartPositionRearSin = new double[2] { 0.0, 0.0 };//台車後輪の座標[mm, mm, rad]
        //double[] cartPositionCenterSin = new double[2];//台車中心の座標[mm, mm, rad]
        //double[] cartPositionFrontSin = new double[2];//台前後輪の座標[mm, mm, rad]

        double[] IdealCartPosition = new double[2];//目標速度から計算した台車の座標[mm, mm, rad]
        double IdealCartAngle = 0;

        double axisCenterFromRear;//台車の設定した中心座標の後輪からの距離

        //PIDController[,] virtualPID = new PIDController[2, 2] { { new PIDController(1.0, 1.0, 0), new PIDController(1.0, 1.0, 0) },
        //                                                         { new PIDController(1.0, 1.0, 0), new PIDController(1.0, 1.0, 0) } };
        //PIDController pidTest = new PIDController(1, 0.0, 0);

        int cnt = 0;
        DateTime pre_time;
        double time = 0;

        bool deviceOpened = false;
        bool fileOpened = false;

        JoyPad pad = new JoyPad();
        JoyPad.JOYERR joyErr;

        public Form1()
        {
            InitializeComponent();
            textBox_dio[0] = textBox1;
            textBox_dio[1] = textBox2;
            textBox_aio[0] = textBox3;
            textBox_aio[1] = textBox4;
            textBox_counter[0] = textBox5;
            textBox_counter[1] = textBox6;
            textBox_counter[2] = textBox7;
            // タイマーの生成
            //timer = new TickTimer(OnElapsed_TimersTimer, Constants.ControllInterval);
            timer1.Interval = Constants.ControllInterval;
            this.Text = "全方向移動プログラム(新)";
        }
        //TickTimer timer;

        private void Form1_Load(object sender, EventArgs e)
        {
            joyErr = pad.GetPosEx(Constants.PadIndex);

            if (joyErr != JoyPad.JOYERR.NOERROR)
            {
                MessageBox.Show("Joypadが認識できていません");
            }

            RadioButton_mode_CheckedChanged(sender, e);
            RadioButton_origin_CheckedChanged(sender, e);

            InitGraph();
            InitLog();

            DrawJoypad();
            DrawCart();

            initPathRecoding();
            CreateCircle();
            CreateEqualIntervalCircle();
            CreateSquare();
            CreateLine();

            listBox_path_SelectedIndexChanged(sender, e);

        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (deviceOpened)
            {
                switchBrake(true);
                switchClutch(false);
                device.close();
                deviceOpened = false;
            }
            if (fileOpened)
                sw.Close();
            //timer.Exit();

        }

        async private void Button_init_Click(object sender, EventArgs e)
        {
            string[] dioReturnCode = new string[2];
            string[] aioReturnCode = new string[2];
            string[] couterReturnCode = new string[3];
            var task = await device.Init(dioReturnCode, aioReturnCode, couterReturnCode);
            deviceOpened = task;
            for (int i = 0; i < 2; i++)
            {
                textBox_dio[i].Text = dioReturnCode[i];
                textBox_aio[i].Text = aioReturnCode[i];
            }
            for (int i = 0; i < 3; i++)
            {
                textBox_counter[i].Text = couterReturnCode[i];

            }
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    targetMotorVoltage[i, j] = 0.0;//念のため初期化                
            if (deviceOpened)
            {
                ApplyVoltageToMotor(targetMotorVoltage);//0を出力

                switchBrake(false);
                switchClutch(false);
            }
        }

        bool start = false;
        int startCnt = 0;
        private void Button_start_stop_Click(object sender, EventArgs e)
        {
            if (start)
            {
                timer1.Stop();
                //timer.Stop();
                button_start_stop.Text = "Start";
                //time = 0;
                cnt = 0;
                start = false;
            }
            else
            {
                timer1.Start();
                //if (startCnt == 0) 
                //    timer.Start();
                //else
                //    timer.Restart();
                startCnt++;
                button_start_stop.Text = "Stop";
                start = true;
                //Button_reset_Click(sender,e);

            }
        }
        //double dt2 = 0;
        //System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();
        //delegate void delegate1();
        double dt = 0;
        System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
        private void timer1_Tick(object sender, EventArgs e)
        {
            //DateTime now_time = DateTime.Now;
            //if (cnt == 0) { pre_time = DateTime.Now; }

            //TimeSpan delt_time = now_time - pre_time; // 時間の差分を取得

            //dt = delt_time.TotalSeconds;//sec
            //                            //System.Console.WriteLine("dt:" + dt);


            if (cnt != 0) sw1.Stop();//ストップウォッチを止める

            dt = sw1.Elapsed.TotalSeconds;
            time += dt;//sec
            //結果を表示する
            //Console.WriteLine(dt);
            sw1.Reset();
            sw1.Start();//ストップウォッチを開始する
            //Console.WriteLine(System.Diagnostics.Stopwatch.Frequency+" " + System.Diagnostics.Stopwatch.IsHighResolution);



            if (deviceOpened)
                GetEncoderRawValue();
            for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
            {
                for (int WorS = 0; WorS < 2; WorS++)//wheel:0 or steer:1
                {
                    if (!deviceOpened)
                    {
                        if (dt > 0.0010)
                            virtualEncoderRps[LorR, WorS] += GetPIDValue(targetEncoderRps[LorR, WorS], virtualEncoderRps[LorR, WorS], dt);
                        encoderRotation[LorR, WorS] += virtualEncoderRps[LorR, WorS] * dt;//for debug
                        if (WorS == steer)
                        {
                            if (encoderRotation[LorR, WorS] >= 0.5) encoderRotation[LorR, WorS] -= 1.0;
                            if (encoderRotation[LorR, WorS] <= -0.5) encoderRotation[LorR, WorS] += 1.0;
                        }
                    }
                    else
                    {
                        CalcRotationFromRawValue(out encoderRotation[LorR, WorS], encoderRawValue[LorR, WorS], Constants.PulsePerRotation[WorS], WorS, Constants.SteerOffset[LorR]);
                    }
                }

                steerAngle[LorR] = encoderRotation[LorR, steer] * (2.0 * Math.PI);
                steerAngleDeg[LorR] = encoderRotation[LorR, steer] * (360.0);

                for (int WorS = 0; WorS < 2; WorS++)//wheel:0 or steer:1
                {
                    CalcEncoderRpsFromEncoderRotation(out encoderRps[LorR, WorS], dt, encoderRotation[LorR, WorS], preEncoderRotation[LorR, WorS], WorS);
                    preEncoderRotation[LorR, WorS] = encoderRotation[LorR, WorS];
                }
            }
            CalcCasterOmegaFromEncoderRps(casterOmega, encoderRps);
            CalcCartAngularVelocity(out cartAngularVelocity, casterOmega, steerAngle, dt);
            CalcCasterVelocity(casterVelocity, casterOmega, steerAngle, cartAngularVelocity);

            //自己位置推定
            CalcCartAngle(ref cartAngle, cartAngularVelocity, casterVelocity);
            CalcCartVelocity(cartVelocityRear, cartVelocityCenter, cartVelocityFront, cartAngle, dt, casterVelocity);
            CalcCartPosition(cartPositionRear, cartPositionCenter, cartPositionFront, dt, cartVelocityRear, cartVelocityCenter, cartVelocityFront, cartAngle);

            for (int i = 0; i < 2; i++)
            {
                switch (origin)
                {
                    case OriginEnum.Rear:
                        cartPosition[i] = cartPositionRear[i];
                        break;
                    case OriginEnum.Center:
                        cartPosition[i] = cartPositionCenter[i];
                        break;
                    case OriginEnum.Front:
                        cartPosition[i] = cartPositionFront[i];
                        break;
                    default:
                        break;
                }
            }
            //台車目標速度決定
            if (mode == ModeEnum.JoypadMode)
            {
                GetTargetCartVelocityByJoypad();
                RecordPath();
            }
            else if (mode == ModeEnum.AutoMode)
            {
                CalcTargetVelocity(targetCartVelocity, ref targetCartAngularVelocity, cartPosition, cartAngle);
            }
            else if (mode == ModeEnum.ManualMode)
            {
                RecordPath();
            }


            if (mode != ModeEnum.ManualMode)
            {
                //台車目標速度追従
                CalcCasterVelocityFromCartVelocity(targetCasterVelocity, targetCartVelocity, targetCartAngularVelocity, cartAngle);
                CalcCasterOmegaFromCasterVelocity(targetCasterOmega, targetCasterVelocity, steerAngle);
                CalcEncoderRpsFromCasterOmega(targetEncoderRps, targetCasterOmega);
                for (int LorR = 0; LorR < 2; LorR++)//left:0 or right:1
                {
                    for (int WorS = 0; WorS < 2; WorS++)//wheel:0 or steer:1
                    {
                        targetEncoderRpsps[LorR, WorS] = (targetEncoderRps[LorR, WorS] - preTargetEncoderRps[LorR, WorS]) / dt;
                        if (Math.Abs(targetEncoderRpsps[LorR, WorS]) >= Constants.EncoderRpspsLimit[WorS])
                        {
                            //MessageBox.Show(targetEncoderRps[LorR, WorS].ToString());
                            //timer1.Enabled = false;//発散防止
                        }
                        CalcMotorRpsFromEncoderRps(out targetMotorRps[LorR, WorS], targetEncoderRps[LorR, WorS], WorS);
                        CalcMotorVoltageFromMotorRps(out targetMotorVoltage[LorR, WorS], targetMotorRps[LorR, WorS]);

                        preTargetEncoderRps[LorR, WorS] = targetEncoderRps[LorR, WorS];
                    }
                }

                if (deviceOpened)
                    ApplyVoltageToMotor(targetMotorVoltage);
            }
            CalcIdealPosition(IdealCartPosition, ref IdealCartAngle, targetCartVelocity, targetCartAngularVelocity, dt);


            CreateLogFile();
            WriteLogData();
            //Invoke(new delegate1(PlotGraph));
            PlotGraph();


            //pre_time = now_time;
            cnt++;
            //ジョイパッドの描写
            DrawJoypad();
            //台車の描写
            DrawCart(); 
            textBox_wheelVoltL.Text = casterVelocity[0, 0].ToString("0.00");
            textBox_steerVoltL.Text = casterVelocity[0, 1].ToString("0.00");
            textBox_wheelVoltR.Text = casterVelocity[1, 0].ToString("0.00");
            textBox_steerVoltR.Text = casterVelocity[1, 1].ToString("0.00");
            textBox_steerAngleL.Text = steerAngleDeg[0].ToString("0.00");
            textBox_steerAngleR.Text = steerAngleDeg[1].ToString("0.00");

            cartAngleDeg = cartAngle * 180.0 / Math.PI;
            cartAngularVelocityDeg = cartAngularVelocity * 180.0 / Math.PI;
            textBox_angle.Text = cartAngleDeg.ToString("000.00");
            textBox_angularVelo.Text = cartAngularVelocityDeg.ToString("000.00");
            textBox_x.Text = cartPosition[0].ToString("000.00");
            textBox_y.Text = cartPosition[1].ToString("000.00");

            textBox_debug.Text = (casterVelocity[0, 1] - casterVelocity[1, 1]).ToString("0.00");
            updateButtonStatus_startDriving();

        }

        private void OnElapsed_TimersTimer(object sender)
        {

            

        }
        public double GetPIDValue(double target, double control, double dt)
        {
            double pGain = 0.8;
            var diff = target - control;
            //System.Console.WriteLine(diff + " " + target + " " + control);

            var p = diff * pGain;
            //i += diff * iGain * dt;
            //d = (diff - preDiff) / dt * dGain;
            //preDiff = diff;
            return p;
            //+ i + d;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RadioButton_mode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_manual.Checked)
            {
                mode = ModeEnum.ManualMode;
                groupBox_joypad.Enabled = false;
                groupBox_autoMode.Enabled = false;
                if (deviceOpened)
                    switchClutch(false);

            }
            else if (radioButton_auto.Checked)
            {
                mode = ModeEnum.AutoMode;
                
                groupBox_joypad.Enabled = false;
                groupBox_autoMode.Enabled = true;

                if (deviceOpened)
                    switchClutch(true);
            }
            else if (radioButton_joypad.Checked)
            {
                mode = ModeEnum.JoypadMode;
                groupBox_joypad.Enabled = true;
                groupBox_autoMode.Enabled = false;

                if (deviceOpened)
                    switchClutch(true);
            }
            textBox_mode.Text = mode.ToString();
            DrawJoypad();
        }
        private void RadioButton_origin_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_front.Checked)
            {
                origin = OriginEnum.Front;
                axisCenterFromRear = Constants.Lc * 2.0;

            }
            else if (radioButton_center.Checked)
            {
                origin = OriginEnum.Center;
                axisCenterFromRear = Constants.Lc;
            }
            else if (radioButton_rear.Checked)
            {
                origin = OriginEnum.Rear;
                axisCenterFromRear = 0.0;
            }
        }

    }
}
