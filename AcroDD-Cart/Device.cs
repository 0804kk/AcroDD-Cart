using System;
using System.Threading.Tasks;
using CaioCs;
using CdioCs;


namespace AcroDD_Cart
{
    class Device
    {
        System.IntPtr hDevice;   // デバイスハンドル(カウンタ)

        Cdio dio = new Cdio();
        Caio aio = new Caio();
        short[] dioId = new short[2];
        short[] aioId = new short[2];


        async public Task<bool> Init(string[] dioReturnCode, string[] aioReturnCode, string[] counterReturnCode)
        {
            bool result = await Task.Run(() =>
            {
                if (dioReturnCode.Length != 2) return false;
                if (aioReturnCode.Length != 2) return false;
                if (counterReturnCode.Length != 3) return false;

                for (int i = 0; i < 2; i++)
                {
                    string returnCode;
                    var ret = DioInit( Constants.DioDeviceName[i], out dioId[i], out returnCode);
                    dioReturnCode[i] = returnCode;
                    if (!ret) return false;
                }
                dio.SetIoDirection(dioId[Constants.DioInputIndex], 0);//todo
                dio.SetIoDirection(dioId[Constants.DioOutputIndex], 5); //1:portAout, 2:portBout, 4:portCout then 5:portAandCout

                for (int i = 0; i < 2; i++)
                {
                    string returnCode;
                    var ret = AioInit( Constants.AioDeviceName[i], out aioId[i], out returnCode);
                    aioReturnCode[i] = returnCode;
                    if (!ret) return false;
                }
                for (int i = 0; i < 2; i++)//todo
                {
                    var ret = aio.SetAoRangeAll(aioId[i], (short)CaioConst.PM10);//0~10V
                    if (ret!= (int)CdioConst.DIO_ERR_SUCCESS) return false;
                }

                {
                    string returnCode;
                    var ret = CounterBoardInit(out returnCode);
                    counterReturnCode[0] = returnCode;
                    if (!ret) return false;
                }
                for (int i = 0; i < 2; i++)
                {
                    string returnCode;
                    var ret = CounterCHSetting(Constants.CounterCH[i], out returnCode);
                    counterReturnCode[1+i] = returnCode;
                    if (!ret) return false;
                }
                return true;
            });
            return result;
        }
        private bool DioInit(string deviceName, out short dioId, out string returnCode)
        {
            // Initialization handling
            int Ret = dio.Init(deviceName, out dioId);

            string ErrorString;
            dio.GetErrorString(Ret, out ErrorString);
            returnCode = "Ret = " + System.Convert.ToString(Ret) + " : " + ErrorString;
            Console.WriteLine(returnCode);
            if (Ret != (int)CdioConst.DIO_ERR_SUCCESS)
            {
                return false;
            }
            return true;
        }
        private bool AioInit(string deviceName, out short aioId, out string returnCode)
        {
            // Initialization handling
            int Ret = aio.Init(deviceName, out aioId);

            string ErrorString;
            aio.GetErrorString(Ret, out ErrorString);
            returnCode = "Ret = " + System.Convert.ToString(Ret) + " : " + ErrorString;
            Console.WriteLine(returnCode);
            if (Ret != 0)
            {
                return false;
            }
            return true;
        }

        private bool CounterBoardInit(out string returnCode)
        {
            int Status;
            int Number = 0;

            // 接続デバイス数取得
            Status = EPX18QC.EPX18QC_GetNumberOfDevices(ref Number);
            if (Status != EPX18QC.EPX18QC_OK)
            {
                returnCode = "EPX18QC_GetNumberOfDevices() Error";
                return false;
            }
            if (Number == 0)
            {
                returnCode = "DeviceNumber = 0";
                return false;
            }
            
            // デバイス接続
            Status = EPX18QC.EPX18QC_Open(ref hDevice);
            if (Status != EPX18QC.EPX18QC_OK)
            {
                returnCode = "EPX18QC_Open() Error";
                return false;
            }
            returnCode = "EPX18QC Init Succesed";
            return true;
        }

        private bool CounterCHSetting(byte CH, out string returnCode)
        {
            int Status;
            // カウンタ動作モード設定
            Status = EPX18QC.EPX18QC_SetCounterMode(hDevice, CH, EPX18QC.EPX18QC_CNT_MODE_2PHASE); // 2相パルス入力モード
            if (Status != EPX18QC.EPX18QC_OK)
            {
                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
                returnCode = "EPX18QC_SetCounterMode() Error";
                return false;
            }

            // カウント方向設定
            Status = EPX18QC.EPX18QC_SetCounterDirection(hDevice, CH, EPX18QC.EPX18QC_CNT_DIR_UP); // アップカウント
            if (Status != EPX18QC.EPX18QC_OK)
            {
                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
                returnCode = "EPX18QC_SetCounterDirection() Error";
                return false;
            }

            // カウンタリセット
            Status = EPX18QC.EPX18QC_ResetCounter(hDevice, CH);
            if (Status != EPX18QC.EPX18QC_OK)
            {
                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
                returnCode = "EPX18QC_ResetCounter() Error";
                return false;
            }

            // カウンタ動作有効
            Status = EPX18QC.EPX18QC_SetCounterControl(hDevice, CH, EPX18QC.EPX18QC_CNT_ENABLE);
            if (Status != EPX18QC.EPX18QC_OK)
            {
                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
                returnCode = "EPX18QC_SetCounterControl() Error";
                return false;
            }
            returnCode = $"EPX18QC CH{CH} Setting Succesed";
            return true;
        }

        public bool GetCounterValue(byte CH, ref int value ,out string returnCode)
        {
            int Status;
            // カウンタ値取得
            Status = EPX18QC.EPX18QC_GetCounterValue(hDevice, CH, ref value);
            if (Status != EPX18QC.EPX18QC_OK)
            {
                EPX18QC.EPX18QC_Close(hDevice);	// デバイス切断
                returnCode = "EPX18QC_GetCounterValue() Error";
                return false;
            }
            returnCode = "EPX18QC_GetCounterValue() Succesed";
            return true;
        }
        public bool GetDioInputValue(byte[] Data, out string returnCode)
        {
            short[] PortNo = new short[8];
            //-----------------------------
            // Set Port No.
            //-----------------------------
            for (short InPort = 0; InPort < 3; InPort++)
            {
                PortNo[InPort] = InPort;
            }
            //-----------------------------
            // Port input
            //-----------------------------
            int Ret;
            Ret = dio.InpMultiByte(dioId[Constants.DioInputIndex], PortNo, 3, Data);
            //-----------------------------
            // Error process
            //-----------------------------
            dio.GetErrorString(Ret, out returnCode);
            if (Ret != (int)CdioConst.DIO_ERR_SUCCESS)
            {
                return false;
            }
            return true;
        }
        public bool SetDioOutputByte(short OutPortNo, byte OutPortData)
        {
            //-----------------------------
            // Port input
            //-----------------------------
            var Ret = dio.OutByte(dioId[Constants.DioOutputIndex], OutPortNo, OutPortData);
            //-----------------------------
            // Error process
            //-----------------------------
            string ErrorString;
            dio.GetErrorString(Ret, out ErrorString);
            //textBox_ReturnCode.Text = "Ret = " + System.Convert.ToString(Ret) + " : " + ErrorString;
            if (Ret != (int)CdioConst.DIO_ERR_SUCCESS)
            {
                return false;
            }
            return true;
        }

        async public Task<bool> SetAioOutputVoltage(int index, float[] voltage)//left:0 ,right:1
        {
            bool result = await Task.Run(() =>
            {
                if (voltage.Length != 2) return false;
                for (int i = 0; i < 2; i++)
                {
                    if (Math.Abs(voltage[i]) >= 3.0)
                    {
                        System.Console.WriteLine("over voltage");
                        return false;
                    }

                }
                //アナログ出力
                int Ret = 0;
                Ret = aio.MultiAoEx(aioId[index], 2, voltage);
                if (Ret != 0)
                {
                    string ErrorString;
                    aio.GetErrorString(Ret, out ErrorString);
                    //label_Information.Text = "aio.MultiAoEx = " + Ret.ToString() + " : " + ErrorString;
                    return false;
                }
                System.Console.Write("W:{0} S:{1} ", voltage[0], voltage[1]);
                //Task.Delay(1000);
                return true;
            });
            return result;
        }
        public bool SetAioOutputDoBit(int index, short DoBit, short DoData)//left:0 ,right:1
        {
            //デジタル出力
            int Ret = 0;
            Ret = aio.OutputDoBit(aioId[index], DoBit, DoData);
            if (Ret != 0)
            {
                string ErrorString;
                aio.GetErrorString(Ret, out ErrorString);
                //label_Information.Text = "aio.OutputDoBit = " + Ret.ToString() + " : " + ErrorString;
                return false;
            }
            System.Console.Write("({0}){1} ", DoBit, DoData);
            return true;
        }

        public void close()//todo ちゃんとかく
        {


            for (int i = 0; i < 2; i++)
            {
                dio.Exit(dioId[i]);
            }
            for (int i = 0; i < 2; i++)
            {
                aio.Exit(aioId[i]);
            }
            EPX18QC.EPX18QC_Close(hDevice);
            
        }
    }
}

//namespace AcroDD_Cart
//{
//    class Device
//    {
//        System.IntPtr hDevice;   // デバイスハンドル(カウンタ)

//        Cdio[] dio = new Cdio[2];
//        Caio[] aio = new Caio[2];
//        short[] dioId = new short[2];
//        short[] aioId = new short[2];

//        bool deviceOpened = false;

//        public bool Init(string[] dioReturnCode, string[] aioReturnCode, string[] counterReturnCode)
//        {
//            if (dioReturnCode.Length != 2) return false;
//            if (aioReturnCode.Length != 2) return false;
//            if (counterReturnCode.Length != 3) return false;

//            for (int i = 0; i < 2; i++)
//            {
//                dio[i] = new Cdio();
//                string returnCode;
//                var ret = DioInit(dio[i], Constants.DioDeviceName[i], out dioId[i], out returnCode);
//                dioReturnCode[i] = returnCode;
//                if (!ret) return false;
//            }
//            dio[Constants.DioInputIndex].SetIoDirection(dioId[Constants.DioInputIndex], 0);
//            dio[Constants.DioOutputIndex].SetIoDirection(dioId[Constants.DioOutputIndex], 5);

//            for (int i = 0; i < 2; i++)
//            {
//                aio[i] = new Caio();
//                string returnCode;
//                var ret = AioInit(aio[i], Constants.AioDeviceName[i], out aioId[i], out returnCode);
//                aioReturnCode[i] = returnCode;
//                if (!ret) return false;
//            }
//            {
//                string returnCode;
//                var ret = CounterBoardInit(out returnCode);
//                counterReturnCode[0] = returnCode;
//                if (!ret) return false;
//            }
//            for (int i = 0; i < 2; i++)
//            {
//                string returnCode;
//                var ret = CounterCHSetting(Constants.CounterCH[i], out returnCode);
//                counterReturnCode[1 + i] = returnCode;
//                if (!ret) return false;
//            }
//            deviceOpened = true;
//            return true;
//        }
//        private bool DioInit(Cdio dio, string deviceName, out short dioId, out string returnCode)
//        {
//            // Initialization handling
//            int Ret = dio.Init(deviceName, out dioId);

//            string ErrorString;
//            dio.GetErrorString(Ret, out ErrorString);
//            returnCode = "Ret = " + System.Convert.ToString(Ret) + " : " + ErrorString;
//            Console.WriteLine(returnCode);
//            if (Ret != (int)CdioConst.DIO_ERR_SUCCESS)
//            {
//                return false;
//            }
//            return true;
//        }
//        private bool AioInit(Caio aio, string deviceName, out short aioId, out string returnCode)
//        {
//            // Initialization handling
//            int Ret = aio.Init(deviceName, out aioId);

//            string ErrorString;
//            aio.GetErrorString(Ret, out ErrorString);
//            returnCode = "Ret = " + System.Convert.ToString(Ret) + " : " + ErrorString;
//            Console.WriteLine(returnCode);
//            if (Ret != 0)
//            {
//                return false;
//            }
//            return true;
//        }
//        private bool CounterBoardInit(out string returnCode)
//        {
//            int Status;
//            int Number = 0;

//            // 接続デバイス数取得
//            Status = EPX18QC.EPX18QC_GetNumberOfDevices(ref Number);
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                returnCode = "EPX18QC_GetNumberOfDevices() Error";
//                return false;
//            }
//            if (Number == 0)
//            {
//                returnCode = "DeviceNumber = 0";
//                return false;
//            }

//            // デバイス接続
//            Status = EPX18QC.EPX18QC_Open(ref hDevice);
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                returnCode = "EPX18QC_Open() Error";
//                return false;
//            }
//            returnCode = "EPX18QC Init Succesed";
//            return true;
//        }

//        private bool CounterCHSetting(byte CH, out string returnCode)
//        {
//            int Status;
//            // カウンタ動作モード設定
//            Status = EPX18QC.EPX18QC_SetCounterMode(hDevice, CH, EPX18QC.EPX18QC_CNT_MODE_2PHASE); // 2相パルス入力モード
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
//                returnCode = "EPX18QC_SetCounterMode() Error";
//                return false;
//            }

//            // カウント方向設定
//            Status = EPX18QC.EPX18QC_SetCounterDirection(hDevice, CH, EPX18QC.EPX18QC_CNT_DIR_UP); // アップカウント
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
//                returnCode = "EPX18QC_SetCounterDirection() Error";
//                return false;
//            }

//            // カウンタリセット
//            Status = EPX18QC.EPX18QC_ResetCounter(hDevice, CH);
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
//                returnCode = "EPX18QC_ResetCounter() Error";
//                return false;
//            }

//            // カウンタ動作有効
//            Status = EPX18QC.EPX18QC_SetCounterControl(hDevice, CH, EPX18QC.EPX18QC_CNT_ENABLE);
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                EPX18QC.EPX18QC_Close(hDevice); // デバイス切断
//                returnCode = "EPX18QC_SetCounterControl() Error";
//                return false;
//            }
//            returnCode = $"EPX18QC CH{CH} Setting Succesed";
//            return true;
//        }

//        public bool GetCounterValue(byte CH, ref int value, out string returnCode)
//        {
//            int Status;
//            // カウンタ値取得
//            Status = EPX18QC.EPX18QC_GetCounterValue(hDevice, CH, ref value);
//            if (Status != EPX18QC.EPX18QC_OK)
//            {
//                EPX18QC.EPX18QC_Close(hDevice);	// デバイス切断
//                returnCode = "EPX18QC_GetCounterValue() Error";
//                return false;
//            }
//            returnCode = "EPX18QC_GetCounterValue() Succesed";
//            return true;
//        }
//        public bool GetDioInputValue(byte[] Data, out string returnCode)
//        {
//            short[] PortNo = new short[8];
//            //-----------------------------
//            // Set Port No.
//            //-----------------------------
//            for (short InPort = 0; InPort < 3; InPort++)
//            {
//                PortNo[InPort] = InPort;
//            }
//            //-----------------------------
//            // Port input
//            //-----------------------------
//            int Ret;
//            Ret = dio[Constants.DioInputIndex].InpMultiByte(dioId[Constants.DioInputIndex], PortNo, 3, Data);
//            //-----------------------------
//            // Error process
//            //-----------------------------
//            dio[Constants.DioInputIndex].GetErrorString(Ret, out returnCode);
//            if (Ret != (int)CdioConst.DIO_ERR_SUCCESS)
//            {
//                return false;
//            }
//            return true;
//        }


//        public void close()//todo ちゃんとかく
//        {
//            if (!deviceOpened)
//                return;

//            for (int i = 0; i < 2; i++)
//            {
//                dio[i].Exit(dioId[i]);
//            }
//            for (int i = 0; i < 2; i++)
//            {
//                aio[i].Exit(aioId[i]);
//            }
//            EPX18QC.EPX18QC_Close(hDevice);
//            deviceOpened = false;

//        }
//    }
//}