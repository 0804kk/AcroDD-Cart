using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

namespace AcroDD_Cart
{
    public partial class Form1
    {

        //仮想ジョイパッド
        Vector joyValue = new Vector();
        double joyValueZ = 0;
        Vector joyValueFilter = new Vector();
        double joyValueFilterZ = 0;
        Vector joyDirection = new Vector();
        int padDiameter = 70;
        int padEdge = 5;
        int frameMargin = 20;
        int frameDiameter = 100;
        int frameEdgeDiameter = 5;

        bool padDraged = false;
        bool scrolling = false;
        private void DrawJoypad()
        {
            //joypadの値更新
            float padY = 0f;
            float padX = 0f;
            if (joyErr == JoyPad.JOYERR.NOERROR)
            {
                pad.GetPosEx(Constants.padIndex);
                padY = -((float)pad.JoyInfoEx.dwXpos - 32767f) / 32768f;
                padX = -((float)pad.JoyInfoEx.dwYpos - 32767f) / 32768f;
            }

            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            //画面座標をクライアント座標に変換する
            System.Drawing.Point cp = this.PointToClient(sp);
            var mauseX = cp.X - groupBox_joypad.Location.X - pictureBox_joypad.Location.X;
            var mauseY = cp.Y - groupBox_joypad.Location.Y - pictureBox_joypad.Location.Y;

            //System.Console.WriteLine(pad.JoyInfoEx.dwFlags + " " + pad.JoyInfoEx.dwButtons);
            if (joyErr == JoyPad.JOYERR.NOERROR) { 
                if (pad.JoyInfoEx.dwButtons == 32) joyValueZ = -1.0;
                else if (pad.JoyInfoEx.dwButtons == 16) joyValueZ = 1.0;
                else joyValueZ = 0.0;
            }
            if (padDraged)
            {
                joyValue.X = -(mauseY - pictureBox_joypad.Width / 2.0) / (frameDiameter / 2.0);
                joyValue.Y = -(mauseX - pictureBox_joypad.Height / 2.0) / (frameDiameter / 2.0);
                joyDirection = joyValue;
                joyDirection.Normalize();
                if (joyValue.Length >= 1.0)
                {
                    joyValue.X = 1.0 * joyDirection.X;
                    joyValue.Y = 1.0 * joyDirection.Y;
                }
            }
            else
            {
                joyValue.X = 0;
                joyValue.Y = 0;
            }
            if (Math.Abs(padX) > 0.1) joyValue.X = padX;
            if (Math.Abs(padY) > 0.1) joyValue.Y = padY;

            if(!scrolling)
                trackBar_joypad.Value = (int)(trackBar_joypad.Maximum * joyValueZ);
            if (trackBar_joypad.Value!=0)
                joyValueZ = (double)trackBar_joypad.Value / (double)trackBar_joypad.Maximum;

            joyValueFilter.X = joyValueFilter.X * 0.8 + joyValue.X * 0.2;
            joyValueFilter.Y = joyValueFilter.Y * 0.8 + joyValue.Y * 0.2;
            joyValueFilterZ = joyValueFilterZ * 0.8 + joyValueZ * 0.2;

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(pictureBox_joypad.Width, pictureBox_joypad.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);

            double joyX = frameDiameter / 2 * joyValueFilter.Y;
            double joyY = -frameDiameter / 2 * joyValueFilter.X;

            Rectangle rectFrame = GetRectangle(pictureBox_joypad.Width, pictureBox_joypad.Height, 0, 0, frameDiameter + frameMargin);
            Rectangle rectFrameEdge = GetRectangle(pictureBox_joypad.Width, pictureBox_joypad.Height, 0, 0, frameDiameter + frameMargin + frameEdgeDiameter);
            Rectangle rectPad = GetRectangle(pictureBox_joypad.Width, pictureBox_joypad.Height, (int)(joyX), (int)(joyY), padDiameter);
            Rectangle rectPadEdge = GetRectangle(pictureBox_joypad.Width, pictureBox_joypad.Height, (int)(joyX), (int)(joyY), padDiameter + padEdge);

            // 線型グラデーションブラシ
            LinearGradientBrush brushFrame = new LinearGradientBrush(rectFrame, Color.Gray, Color.DimGray, LinearGradientMode.ForwardDiagonal);
            LinearGradientBrush brushFrameEdge = new LinearGradientBrush(rectFrameEdge, Color.LightGray, Color.Gray, LinearGradientMode.ForwardDiagonal);
            LinearGradientBrush brushPad = new LinearGradientBrush(rectPad, Color.Gray, Color.Black, LinearGradientMode.ForwardDiagonal);
            LinearGradientBrush brushPadEdge = new LinearGradientBrush(rectPadEdge, Color.Black, Color.DimGray, LinearGradientMode.ForwardDiagonal);

            g.FillEllipse(brushFrameEdge, rectFrameEdge);
            g.FillEllipse(brushFrame, rectFrame);
            g.FillEllipse(brushPadEdge, rectPadEdge);
            g.FillEllipse(brushPad, rectPad);

            //リソースを解放する
            g.Dispose();

            brushFrame.Dispose();
            brushFrameEdge.Dispose();
            brushPad.Dispose();
            brushPadEdge.Dispose();

            if (groupBox_joypad.Enabled == false)
            {
                canvas = (Bitmap)CreateGrayscaleImage(canvas);
            }
            //PictureBox1に表示する
            pictureBox_joypad.Image = canvas;

            textBox_joyX.Text = joyValueFilter.X.ToString("0.000");
            textBox_joyY.Text = joyValueFilter.Y.ToString("0.000");
            textBox_joyZ.Text = joyValueFilterZ.ToString("0.000");
        }


        private Rectangle GetRectangle(int width, int height, int posX, int posY, int radius)
        {
            return new Rectangle(width / 2 - radius / 2 - posX, height / 2 - radius / 2 + posY, radius, radius);
        }

        private void PictureBox_joypad_MouseDown(object sender, MouseEventArgs e)
        {
            padDraged = true;
        }

        private void PictureBox_joypad_MouseUp(object sender, MouseEventArgs e)
        {
            padDraged = false;
        }

        private void TrackBar_joypad_MouseUp(object sender, MouseEventArgs e)
        {
            trackBar_joypad.Value = 0;
            joyValueZ = 0;
            scrolling = false;
        }
        private void TrackBar_joypad_MouseDown(object sender, MouseEventArgs e)
        {
            scrolling = true;
        }
        private void Form1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            joyValueZ += e.Delta / 120;
            if (joyValueZ > 1.0) joyValueZ = 1.0;
            if (joyValueZ < -1.0) joyValueZ = -1.0;
        }
        private  void DrawCart()
        {
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(pictureBox_cart.Width, pictureBox_cart.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            int ratio = 6;
            int casterInterval= (int)Constants.Wc / ratio;//Wc/2
            int cartWidth = casterInterval + 20;
            int cartLength = (int)Constants.Lc * 2 / ratio;//Lc
            int cartFrameLength = cartLength + 40;

            int casterWidth = 15;
            int casterRadius = (int)Constants.WheelRadius / ratio;//radius
            int casterOffset = (int)Constants.CasterOffset / ratio;//radius

            int centerX = pictureBox_cart.Width/2;
            int centerY = pictureBox_cart.Height/2;
            var angle = cartAngle;
            //var angle = 0;
            var cartAngleDraw = -angle + Math.PI / 2;


            PointF[] casterPosition = { new PointF(centerX + cartLength / 2 * (float)Math.Cos(cartAngleDraw) - casterInterval / 2 * (float)Math.Cos(angle), centerY + cartLength/2* (float)Math.Sin(cartAngleDraw) - casterInterval / 2 * (float)Math.Sin(-angle)),
                                        new PointF(centerX + cartLength / 2 * (float)Math.Cos(cartAngleDraw) + casterInterval / 2 * (float)Math.Cos(angle), centerY + cartLength/2* (float)Math.Sin(cartAngleDraw) + casterInterval / 2 * (float)Math.Sin(-angle)) };
            PointF[] caster1 = new PointF[2];
            PointF[] caster2 = new PointF[2];
            PointF cartFront = new PointF(centerX + cartFrameLength / 2 * (float)Math.Cos(cartAngleDraw - Math.PI), centerY + cartFrameLength / 2 * (float)Math.Sin(cartAngleDraw - Math.PI));
            PointF cartRear = new PointF(centerX - cartFrameLength / 2 * (float)Math.Cos(cartAngleDraw - Math.PI), centerY - cartFrameLength / 2 * (float)Math.Sin(cartAngleDraw - Math.PI));
            Rectangle[] rect = new Rectangle[2];

            PointF[] veloVec1 = new PointF[2];
            PointF[] veloVec2 = new PointF[2];
            PointF veloVecCart1 = new PointF(centerX , centerY );
            PointF veloVecCart2 = new PointF(centerX - (float)cartVelocityRear[1], centerY - (float)cartVelocityRear[0]);


            Rectangle rectCenter = GetRectangle(centerX*2, centerY*2, 0, 0, 15);

            Pen pen = new Pen(Color.Black, casterWidth);
            Pen penVelo = new Pen(Color.Lime, 4);
            penVelo.EndCap = LineCap.ArrowAnchor;
            Pen penVeloCart = new Pen(Color.Red, 4);
            penVeloCart.EndCap = LineCap.ArrowAnchor;
            Pen penCart = new Pen(Color.SlateGray, cartWidth);

            //pictureBox_cart.BackColor = Color.PowderBlue;
            //HatchBrushオブジェクトの作成
            HatchBrush myBrush = new HatchBrush(HatchStyle.Percent50, Color.DarkGray, Color.Silver);
            //四角を塗りつぶす
            g.FillRectangle(myBrush, 0, 0, pictureBox_cart.Width, pictureBox_cart.Height);

            g.DrawLine(penCart, cartFront, cartRear);
            g.FillEllipse(Brushes.DarkBlue, rectCenter);
            for (int i = 0; i < 2; i++)
            {
                var steerAngleDraw = -steerAngle[i] + cartAngleDraw;
                rect[i] = GetRectangle((int)casterPosition[i].X * 2, (int)casterPosition[i].Y * 2, 0, 0, 10);
                caster1[i].X = casterPosition[i].X +(casterOffset + casterRadius) * (float)Math.Cos(steerAngleDraw);
                caster1[i].Y = casterPosition[i].Y +(casterOffset + casterRadius) * (float)Math.Sin(steerAngleDraw);

                caster2[i].X = casterPosition[i].X + (casterOffset - casterRadius) * (float)Math.Cos(steerAngleDraw);
                caster2[i].Y = casterPosition[i].Y + (casterOffset - casterRadius) * (float)Math.Sin(steerAngleDraw);
                g.DrawLine(pen, caster2[i], caster1[i]);
                g.FillEllipse(Brushes.Crimson, rect[i]);//todo

                veloVec1[i].X = casterPosition[i].X + (casterOffset) * (float)Math.Cos(steerAngleDraw);
                veloVec1[i].Y = casterPosition[i].Y + (casterOffset) * (float)Math.Sin(steerAngleDraw);
                veloVec2[i].X = veloVec1[i].X - (int)(30 * casterOmega[i, 0]) * (float)Math.Cos(steerAngleDraw);
                veloVec2[i].Y = veloVec1[i].Y - (int)(30 * casterOmega[i, 0]) * (float)Math.Sin(steerAngleDraw);
                g.DrawLine(penVelo, veloVec1[i], veloVec2[i]);
            }
            //g.DrawLine(penVeloCart, veloVecCart1, veloVecCart2);


            //リソースを解放する
            g.Dispose();
            pen.Dispose();
            penVelo.Dispose();
            penCart.Dispose();
            myBrush.Dispose();

            //PictureBox1に表示する
            pictureBox_cart.Image = canvas;
        }

        private void Button_up_Click(object sender, EventArgs e)
        {
            joyValue.X = 1.0;
            joyValue.Y = 0;
        }
        private void Button_left_Click(object sender, EventArgs e)
        {
            joyValue.X = 0;
            joyValue.Y = 1.0;
        }

        private void Button_right_Click(object sender, EventArgs e)
        {
            joyValue.X = 0;
            joyValue.Y = -1.0;
        }

        private void Button_down_Click(object sender, EventArgs e)
        {
            joyValue.X = -1.0;
            joyValue.Y = 0;
        }
        private void Button_maru_Click(object sender, EventArgs e)
        {
            joyValue.X = 0;
            joyValue.Y = 0;
        }
        public static Image CreateGrayscaleImage(Image img)
        {
            //グレースケールの描画先となるImageオブジェクトを作成
            Bitmap newImg = new Bitmap(img.Width, img.Height);
            //newImgのGraphicsオブジェクトを取得
            Graphics g = Graphics.FromImage(newImg);

            //ColorMatrixオブジェクトの作成
            //グレースケールに変換するための行列を指定する
            System.Drawing.Imaging.ColorMatrix cm =
                new System.Drawing.Imaging.ColorMatrix(
                    new float[][]{
                        new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                        new float[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f },
                        new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f },
                        new float[] { 0.0f, 0.0f, 0.0f, 0.5f, 0.0f },
                        new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
                    });
            //ImageAttributesオブジェクトの作成
            System.Drawing.Imaging.ImageAttributes ia =
                new System.Drawing.Imaging.ImageAttributes();
            //ColorMatrixを設定する
            ia.SetColorMatrix(cm);

            //ImageAttributesを使用してグレースケールを描画
            g.DrawImage(img,
                new Rectangle(0, 0, img.Width, img.Height),
                0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

            //リソースを解放する
            g.Dispose();

            return newImg;
        }

    }
}
