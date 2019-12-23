using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcroDD_Cart
{
    public partial class Form1
    {
        StreamWriter sw;
        string logFilePath;
        string logDirectoryPath;
        string defaultDirectoryPath;

        private void InitLog()
        {
            defaultDirectoryPath = Application.StartupPath;
            var logDirectoryPathFilePath = defaultDirectoryPath + @"\logDirectroyPath.txt";

            if (System.IO.File.Exists(logDirectoryPathFilePath))
            {
                string readText = File.ReadAllText(logDirectoryPathFilePath);
                Console.WriteLine(readText);
                logDirectoryPath = readText;

            }
            else
            {
                MessageBox.Show(logDirectoryPathFilePath + "が存在しません．");
                //FolderBrowserDialogクラスのインスタンスを作成
                FolderBrowserDialog fbd = new FolderBrowserDialog();

                //上部に表示する説明テキストを指定する
                fbd.Description = "Logフォルダを指定してください．";
                //ルートフォルダを指定する
                //デフォルトでDesktop
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                //最初に選択するフォルダを指定する
                //RootFolder以下にあるフォルダである必要がある
                fbd.SelectedPath = defaultDirectoryPath;
                //ユーザーが新しいフォルダを作成できるようにする
                //デフォルトでTrue
                fbd.ShowNewFolderButton = true;

                //ダイアログを表示する
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    //選択されたフォルダを表示する
                    Console.WriteLine(fbd.SelectedPath);
                }
                StreamWriter sw = File.CreateText(logDirectoryPathFilePath);
                sw.Write(fbd.SelectedPath);
                sw.Close();
                logDirectoryPath = fbd.SelectedPath;
            }

            DateTime now_time = DateTime.Now;

            string folderName = logDirectoryPath + @"\" + now_time.ToString("yyyyMMdd");
            if (!System.IO.Directory.Exists(folderName))
            {
                Console.WriteLine(folderName);
                Directory.CreateDirectory(folderName);//日付のフォルダが存在しなかったら作成
            }
            logFilePath = folderName + @"\" + now_time.ToString("yyyyMMdd#HHmmss#");
            textBox_csvName.Text = Path.GetFileName(logFilePath);
        }
        private void WriteLogHeader()
        {
            var headerList = new List<string>();

            headerList.Add(nameof(time));
            headerList.Add(nameof(dt));

            headerList.AddRange(GetArrayNameToList(nameof(cartPositionCenter), 2));
            //headerList.AddRange(GetArrayNameToList(nameof(cartPositionCenterTan), 2));
            //headerList.AddRange(GetArrayNameToList(nameof(cartPositionCenterSin), 2));
            //headerList.AddRange(GetArrayNameToList(nameof(IdealCartPosition), 2));
            //headerList.Add(nameof(IdealCartAngle));
            headerList.AddRange(GetArrayNameToList(nameof(targetPosition), 3));

            headerList.AddRange(GetArrayNameToList(nameof(cartVelocityCenter), 2));
            //headerList.AddRange(GetArrayNameToList(nameof(cartVelocityCenterTan), 2));
            //headerList.AddRange(GetArrayNameToList(nameof(cartVelocityCenterSin), 2));

            headerList.Add(nameof(cartAngularVelocity));
            //headerList.Add(nameof(cartAngularVelocityTan));
            //headerList.Add(nameof(cartAngularVelocitySin));
            headerList.Add(nameof(cartAngle));
            headerList.Add(nameof(cartAngleTan));
            headerList.Add(nameof(cartAngleSin));

            headerList.AddRange(GetArrayNameToList(nameof(targetCartVelocity), 2));
            headerList.Add(nameof(targetCartAngularVelocity));

            headerList.AddRange(GetArrayNameToList(nameof(encoderRps), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(targetEncoderRps), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(casterOmega), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(targetCasterOmega), 2, 2));

            headerList.AddRange(GetArrayNameToList(nameof(casterVelocity), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(targetCasterVelocity), 2, 2));


            headerList.AddRange(GetArrayNameToList(nameof(encoderRawValue), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(encoderRotation), 2, 2));
            headerList.AddRange(GetArrayNameToList(nameof(targetMotorRps), 2,2));
            headerList.AddRange(GetArrayNameToList(nameof(targetMotorVoltage), 2,2));

            headerList.AddRange(GetArrayNameToList(nameof(steerAngle), 2));
            WriteCsv(headerList);
        }


        private void WriteLogData()
        {
            var logList = new List<double>();

            logList.Add(time);
            logList.Add(dt);

            logList.AddRange(GetValueToList(cartPositionCenter, 2));
            //logList.AddRange(GetValueToList(cartPositionCenterTan, 2));
            //logList.AddRange(GetValueToList(cartPositionCenterSin, 2));
            //logList.AddRange(GetValueToList(IdealCartPosition, 2));
            //logList.Add(IdealCartAngle);
            logList.AddRange(GetValueToList(targetPosition, 3));

            logList.AddRange(GetValueToList(cartVelocityCenter, 2));
            //logList.AddRange(GetValueToList(cartVelocityCenterTan, 2));
            //logList.AddRange(GetValueToList(cartVelocityCenterSin, 2));

            logList.Add(cartAngularVelocity);
            //logList.Add(cartAngularVelocityTan);
            //logList.Add(cartAngularVelocitySin);
            logList.Add(cartAngle);
            logList.Add(cartAngleTan);
            logList.Add(cartAngleSin);

            logList.AddRange(GetValueToList(targetCartVelocity, 2));
            logList.Add(targetCartAngularVelocity);

            logList.AddRange(GetValueToList(encoderRps, 2, 2));
            logList.AddRange(GetValueToList(targetEncoderRps, 2, 2));
            logList.AddRange(GetValueToList(casterOmega, 2, 2));
            logList.AddRange(GetValueToList(targetCasterOmega, 2, 2));

            logList.AddRange(GetValueToList(casterVelocity, 2, 2));
            logList.AddRange(GetValueToList(targetCasterVelocity, 2, 2));


            logList.AddRange(GetValueToList(encoderRawValue, 2, 2));
            logList.AddRange(GetValueToList(encoderRotation, 2, 2));
            logList.AddRange(GetValueToList(targetMotorRps, 2, 2));
            logList.AddRange(GetValueToList(targetMotorVoltage, 2, 2));
            
            logList.AddRange(GetValueToList(steerAngle, 2));
            WriteCsv<double>(logList);
        }

        private List<string> GetArrayNameToList(string array, int length)
        {
            var nameList = new List<string>();
            for (int i = 0; i < length; i++)
            {
                string axis="";
                if (i == 0) axis = "X";
                if (i == 1) axis = "Y";
                if (i == 2) axis = "θ";
                nameList.Add(array + "[" + axis + "]");
            }
            return nameList;
        }
        private List<string> GetArrayNameToList(string array, int length1, int length2)
        {
            var nameList = new List<string>();
            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    nameList.Add(array + "[" + (i == 0 ? "L" : "R") + "][" + (j == 0 ? "W" : "S") + "]");
                }
            }
            return nameList;
        }
        private List<double> GetValueToList<T>(T[] array, int length)
        {
            var valueList = new List<double>();
            for (int i = 0; i < length; i++)
            {
                valueList.Add((double)(object)array[i]);
            }
            return valueList;
        }
        private List<double> GetValueToList<T>(T[,] array, int length1, int length2)
        {
            var valueList = new List<double>();
            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    valueList.Add(Convert.ToDouble(array[i, j]));
                }
            }
            return valueList;
        }
        private void WriteCsv<T>(List<T> list)
        {
            if (fileOpened)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    sw.Write(list[i] + ",");
                }
                sw.WriteLine("");
            }
        }

        private void CreateLogFile()
        {
            if (!fileOpened)
            {
                if (textBox_csvMemo.Text != "") logFilePath += "_" + textBox_csvMemo.Text;
                try
                {
                    sw = new StreamWriter(logFilePath + ".csv", false, Encoding.GetEncoding("shift_jis"));
                    fileOpened = true;
                }
                catch (Exception)
                {
                    //MessageBox.Show(logDirectoryPath + "を作成してください．");
                    //this.Close();
                    throw;
                }
                WriteLogHeader();
            }
            
        }

        private void Button_open_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            // デフォルトのフォルダを指定する
            ofDialog.InitialDirectory = defaultDirectoryPath;

            //ダイアログのタイトルを指定する
            ofDialog.Title = "ダイアログのタイトル";

            //ダイアログを表示する
            if (ofDialog.ShowDialog() == DialogResult.OK)
            {
                logFilePath = ofDialog.FileName;
                textBox_csvName.Text = Path.GetFileName(logFilePath);
            }
            else
            {
                Console.WriteLine("キャンセルされました");
            }

            // オブジェクトを破棄する
            ofDialog.Dispose();
        }
    }
}
