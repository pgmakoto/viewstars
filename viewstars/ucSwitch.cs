using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ucPgmac
{
    //using ucPgmac;
    public partial class ucSwitch : UserControl
    {
        public event BoolEventHandler CheckedChanged;
        Bitmap body ;
        Bitmap slider ;
        int pos = 0;

        int falsePos = -10;

        [Browsable(true)//←これにより、プロパティがプロパティウィンドウに表示されます
            , Description("16bitの整数値")
            , Category("SQU")]
        bool _checked = false;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    if (CheckedChanged != null) CheckedChanged(this, new BoolEventArgs(_checked));
                    if (_checked) pos = 0;
                    else pos = falsePos; 

                    this.Refresh();
                }
            }
        }


        public Color _ColorOn;
        public Color _ColorOff = DefaultBackColor;
        public Color ColorOn
        {
            get { return _ColorOn; }
            set
            {
                if (_ColorOn != value)
                {
                    _ColorOn = value;
                    this.ucSwitch_Resize(null, null);
                }
            }
        }
        public Color ColorOff
        {
            get { return _ColorOff; }
            set
            {
                if (_ColorOff != value)
                {
                    _ColorOff = value;
                    this.ucSwitch_Resize(null, null);
                }
            }
        }

        public ucSwitch()
        {
            InitializeComponent();
        }

        Point L, R;
        int ed = 1;
        private void CreateBody()
        {
                int H = ((int)(Height / 2)) * 2;
                int W = ((int)(Width / 2)) * 2;

                Brush brush = new SolidBrush(BackColor);
                Brush brushH = new SolidBrush(HsvColor.cvt(BackColor, 0f,0.2f));
                Brush brushD = new SolidBrush(HsvColor.cvt(BackColor, -0.2f));
                ed = (H / 20);
                if (ed < 1) ed = 1;
                if (ed > 3) ed = 3;

                L = new Point(H / 2+ed, H / 2);
                R = new Point(W - H / 2-ed, H / 2);
                falsePos = - R.X + L.X;
                
                int r = H / 2 - ed;
                int d = r * 2;
                //描画先とするImageオブジェクトを作成する
                Bitmap canvas = new Bitmap(W, H);
                Graphics g = Graphics.FromImage(canvas);
                g.FillRectangle(brush, 0, 0, W , H);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                
                g.FillEllipse(brushD ,  L.X-r-ed, L.Y - r-ed, d+ed, d+ed);
                g.FillEllipse(brushD ,  R.X - r, R.Y - r-ed, d+ed, d+ed);
                g.FillRectangle(brushD, L.X -ed, L.Y - r-ed, R.X - L.X, d+ed);

                g.FillEllipse(brushH, L.X - r , L.Y - r + ed, d, d);
                g.FillEllipse(brushH  , R.X - r+ed, R.Y - r+ed, d, d);
                g.FillRectangle(brushH, L.X + ed, L.Y - r+ed, R.X - L.X , d);

      //        g.FillEllipse(brush, L.X - r - e, L.Y - r , d, d);

                r -= ed;
                d = r * 2;
                Pen pen = new Pen(Color.FromArgb(200, Color.Gray), 2);
                g.DrawArc(pen, L.X - r, L.Y - r, d, d,90,180);
                g.DrawArc(pen, R.X - r-0.5f, R.Y - r, d, d, 270, 180);
                g.DrawLine(pen, L.X , R.Y -r+0.5f, R.X+1 , R.Y - r+0.5f);
                g.DrawLine(pen, L.X, R.Y + r, R.X+1 , R.Y + r);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                g.FillEllipse(Brushes.Yellow, L.X - r, L.Y - r, d, d);
                g.FillEllipse(Brushes.Yellow, R.X - r, R.Y - r, d, d);
                g.FillRectangle(Brushes.Yellow, L.X, L.Y - r, R.X - L.X, d);


                //リソースを解放する
                g.Dispose();
                //リソースを解放する
                brush.Dispose();
                brushH.Dispose();
                brushD.Dispose();

                Bitmap gab = body;
                body = canvas;
                body.MakeTransparent(Color.Yellow);
                if (gab != null) gab.Dispose();
        }
        private void CreateSlider()
        {
            int H = ((int)(Height / 2)) * 2;
            int W = ((int)(Width / 2)) * 2;
            ed = (H / 20);
            if (ed < 1) ed = 1;
            if (ed > 3) ed = 3;

            int r = H / 2 -ed;
            int d = r * 2;


            Brush brush = new SolidBrush(BackColor);
            Brush brushH = new SolidBrush(HsvColor.cvt(BackColor, 0f,0.2f));
            Brush brushD = new SolidBrush(HsvColor.cvt(BackColor, -0.2f));

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(W - falsePos, H);
            Point P = new Point(L.X - falsePos, L.Y);

            Graphics g = Graphics.FromImage(canvas);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            g.FillRectangle(new SolidBrush(ColorOn), 0, 0, P.X, H);
            g.FillRectangle(new SolidBrush(ColorOff), P.X, 0, P.X, H);

            //中身の上下の影
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 2);
            g.DrawLine(pen, L.X, R.Y - r + 1.5f, R.X - falsePos, R.Y - r + 1.5f);
            g.DrawLine(pen, L.X, R.Y + r - 1.0f, R.X - falsePos, R.Y + r - 1.0f);
            pen.Dispose();

            //          g.FillEllipse(Brushes.Gray, P.X - r-1, P.Y - r-1, d+2, d+2);
            r -= ed; d = r * 2;
            g.FillEllipse(brush, P.X - r, P.Y - r, d, d);
            g.FillEllipse(brushH, P.X - r, P.Y - r, d - ed*2, d - ed*2);
            g.FillEllipse(brushD, P.X - r+ed*2, P.Y - r+ed*2, d - ed*2, d - ed*2);


            pen = new Pen(Color.FromArgb(200,Color.Gray), 2);
            g.DrawEllipse(pen, P.X - r, P.Y - r, d, d);
            g.DrawArc(pen, L.X - r, L.Y - r, d, d, 90, 180);
            g.DrawArc(pen, R.X - r - falsePos, R.Y - r, d, d, 270, 180);
            g.DrawLine(pen, L.X, R.Y - r + 0.5f, R.X - falsePos, R.Y - r + 0.5f);
            g.DrawLine(pen, L.X, R.Y + r       , R.X - falsePos, R.Y + r);

            r -= ed*2; d = r * 2;

            g.FillEllipse(brush, P.X - r , P.Y - r , d , d );

            //リソースを解放する
            g.Dispose();
            //リソースを解放する
            /*                */
            brush.Dispose();
            brushH.Dispose();
            brushD.Dispose();

            Bitmap gab = slider;
            slider = canvas;
      //      slider.MakeTransparent(Color.Yellow);
            if (gab != null) gab.Dispose();
        }

        private void ucSwitch_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
        }

        private void ucSwitch_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                g.DrawImage(slider, pos, 0);
                g.DrawImage(body, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(" {0}: {1}", ex.GetType().Name, ex.Message);

            }
        }

        private void ucSwitch_Resize(object sender, EventArgs e)
        {

            if (Height < 24) Height = 24;
            if (Width < Height * 2) Width = Height * 2;
            CreateBody();
            CreateSlider();
            if (_checked) pos = 0;
            else pos = falsePos;
            this.Refresh();

        }
        int Mouse_Down = 0;

        private void ucSwitch_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Mouse_Down = e.X ;
            }
        }

        private void ucSwitch_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int newPos = e.X ;
                pos = pos + (newPos - Mouse_Down);
                if (pos < falsePos) { Checked = false; pos = falsePos; }
                if (pos > 0) { Checked = true; pos = 0; }
                Mouse_Down = newPos;
                this.Refresh();
            }
        }



        private void ucSwitch_Load_1(object sender, EventArgs e)
        {

        }

        private void ucSwitch_MouseUp(object sender, MouseEventArgs e)
        {
            Mouse_Down = 0;
            if (Checked) pos = 0;
            else pos = falsePos;
            this.Refresh();
        }

        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ucSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ucSwitch";
            this.Size = new System.Drawing.Size(116, 33);
            this.Load += new System.EventHandler(this.ucSwitch_Load);
            this.BackColorChanged += new System.EventHandler(this.ucSwitch_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ucSwitch_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ucSwitch_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ucSwitch_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ucSwitch_MouseUp);
            this.Resize += new System.EventHandler(this.ucSwitch_Resize);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
