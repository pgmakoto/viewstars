using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ucPgmac
{
    public partial class ucThumbWheel : UserControl
    {
        bool consoledbg = false;
        Bitmap roter6;
        Bitmap roter;
        Bitmap roterS;
        Bitmap roterC;
        Pen pens;
        Pen penl;
        Font font;
        private int intvalue;
        private int wid;
        private int min;
        private int max;
        public event IntEventHandler ValueChanged;

        private float[] pos = new float[7];

        public string _fmt = "00:60:60";
        public string fmt {
            get
            {
                return _fmt;
            }
            set
            {
                _fmt = value;
                autowidth();
                //                NumWidth = _fmt.Length;
            }
        }
        public char fm(int p)
        {
            try {
                return _fmt[NumWidth - p - 1];
            } catch (Exception ex)
            {
                Console.WriteLine(" {0}: {1}", ex.GetType().Name, ex.Message);
            }
            return 'E';
        }

        // タイマーの間隔(ミリ秒)
        Timer timer = new Timer();

        public ucThumbWheel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            //       roter = new Bitmap(Properties.Resources.thWheel);
            //        roterC = createRoterC();
            //       roter = createRoter(10); //roterm = createRoter(-10);
            //      roter6 = createRoter(6);
            //     roterS = createRoterS();

            //ホイールイベントの追加  
            this.MouseWheel
                += new System.Windows.Forms.MouseEventHandler(this._MouseWheel);

        }
        private Bitmap createRoter(int num)
        {
            int n = num;
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(ColWidth, ColHeight * (n * 2 + 2));
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            //Penオブジェクトの作成(幅1の黒色)
            //(この場合はPenを作成せずに、Pens.Blackを使っても良い)
            // Pen p = new Pen(Color.Black, 1);
            // アンチエイリアスの設定
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //      var font = new Font("Segoe UI Semibold", (float)ColHeight*0.55f);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Center;  // 上下方向は中心寄せ

            //new SolidBrush(Color.FromArgb(255, 64, 64, 64))
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, ColWidth, ColHeight * (n * 2 + 2));
            g.DrawLine(penl, 0, 0, 0, ColHeight * (9 + 2));
            g.DrawLine(pens, ColWidth - 1, 0, ColWidth - 1, ColHeight * (9 + 2));
            int y;
            for (y = colheight / 2; y < ColHeight * (num + 1); y += ColHeight)//度
            {
                g.DrawString((n % num).ToString(), font, new SolidBrush(ForeColor), ColWidth / 2, y, format);
                n--;
            }
            for (; y < ColHeight * (num * 2 + 1); y += ColHeight)//度
            {
                n++;
                g.DrawString((n % num).ToString(), font, new SolidBrush(ForeColor), ColWidth / 2, y, format);
            }

            //リソースを解放する
            g.Dispose();

            //thisに表示する
            //     this.Image = canvas;
            return canvas;
        }

        private Bitmap createRoterS()
        {
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(ColWidth, ColHeight * (9 + 2));
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            //Penオブジェクトの作成(幅1の黒色)
            //(この場合はPenを作成せずに、Pens.Blackを使っても良い)
            //    Pen p = new Pen(Color.Black, 1);
            // アンチエイリアスの設定
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //       var font = new Font("Arial", (float)ColHeight * 0.6f);
            //        var font = new Font("Segoe UI Semibold", (float)ColHeight * 0.55f);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Center;  // 上下方向は中心寄せ

            //   g.DrawRectangle(Pens.Black, 10, 20, 100, 80);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, ColWidth, ColHeight * (9 + 2));
            g.DrawLine(penl, 0, 0, 0, ColHeight * (9 + 2));
            g.DrawLine(pens, ColWidth - 1, 0, ColWidth - 1, ColHeight * (9 + 2));

            //       int n = 2;
            //       for (int y = colheight / 2; y < ColHeight * (2 + 1); y += ColHeight)//度
            //       {
            //           g.DrawString((n % 2).ToString(), font, new SolidBrush(Color.White), ColWidth / 2, y, format);
            //           n--;
            //       }
            SolidBrush brush = new SolidBrush(ForeColor);
            g.DrawString("+", font, brush, ColWidth / 2, colheight + colheight / 2, format);
            g.DrawString("-", font, brush, ColWidth / 2, colheight * 2 + colheight / 2, format);
            g.DrawString(":", font, brush, ColWidth / 2, colheight * 3 + colheight / 2, format);
            g.DrawString(".", font, brush, ColWidth / 2, colheight * 4 + colheight / 2, format);
            g.DrawString("\'", font, brush, ColWidth / 2, colheight * 5 + colheight / 2, format);
            g.DrawString("\"", font, brush, ColWidth / 2, colheight * 6 + colheight / 2, format);
            g.DrawString("°", font, brush, ColWidth / 2, colheight * 7 + colheight / 2, format);
            //  g.DrawString("+", font, new SolidBrush(Color.White), ColWidth / 2, colheight *2 + colheight / 2, format);

            //リソースを解放する
            g.Dispose();
            //thisに表示する
            //     this.Image = canvas;
            pens.Dispose();
            penl.Dispose();
            return canvas;

        }
        private Bitmap createRoterC()
        {
            Color c = HsvColor.cvt(BackColor, -0.2f);
            pens = new Pen(c, 1);//shadow
            c = HsvColor.cvt(BackColor, 0f, 0.2f);
            penl = new Pen(c, 1);
            font = new Font(this.Font.FontFamily, (float)ColHeight * 0.55f, this.Font.Style);

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(ColWidth / 2, ColHeight * (9 + 2));
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            //Penオブジェクトの作成(幅1の黒色)
            //(この場合はPenを作成せずに、Pens.Blackを使っても良い)
            //    Pen p = new Pen(Color.Black, 1);
            // アンチエイリアスの設定
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Center;  // 上下方向は中心寄せ

            //   g.DrawRectangle(Pens.Black, 10, 20, 100, 80);
            g.FillRectangle(new SolidBrush(BackColor), 0, 0, ColWidth / 2, ColHeight * (9 + 2));
            g.DrawLine(penl, 0, 0, 0, ColHeight * (9 + 2));
            g.DrawLine(pens, ColWidth - 1, 0, ColWidth - 1, ColHeight * (9 + 2));


            SolidBrush brush = new SolidBrush(ForeColor);
            g.DrawString("+", font, brush, ColWidth / 4, colheight + colheight / 2, format);
            g.DrawString("-", font, brush, ColWidth / 4, colheight * 2 + colheight / 2, format);
            g.DrawString(":", font, brush, ColWidth / 4, colheight * 3 + colheight / 2, format);
            g.DrawString(".", font, brush, ColWidth / 4, colheight * 4 + colheight / 2, format);
            g.DrawString("\'", font, brush, ColWidth / 4, colheight * 5 + colheight / 2, format);
            g.DrawString("\"", font, brush, ColWidth / 4, colheight * 6 + colheight / 2, format);
            g.DrawString("°", font, brush, ColWidth / 4, colheight * 7 + colheight / 2, format);
            //  g.DrawString("+", font, new SolidBrush(Color.White), ColWidth / 4, colheight *2 + colheight / 2, format);

            //リソースを解放する
            g.Dispose();
            //thisに表示する
            //     this.Image = canvas;
            return canvas;

        }

        private void ucThumbWheel_Load(object sender, EventArgs e)
        {
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Refresh();
            // throw new NotImplementedException();
        }

        [Browsable(true)//←これにより、プロパティがプロパティウィンドウに表示されます
        , Description("16bitの整数値")
        , Category("表示")]
        public int Value
        {
            get { return intvalue; }
            set
            {
                if (value < min)
                {
                    intvalue = min;
                }
                if (value > max)
                {
                    intvalue = max;
                }
                if (value >= min && value <= max && intvalue != value)
                {
                    intvalue = value;
                    this.Refresh();
                    if (ValueChanged != null) ValueChanged(this, new IntEventArgs(intvalue));
                }
            }
        }

        public int NumWidth
        {
            get { return wid; }
            set
            {
                if (wid != value)
                {
                    wid = value;
                    Array.Resize(ref pos, wid);
                    this.Refresh();
                }
            }
        }

        private void autowidth()
        {

            NumWidth = fmt.Length;
            this.Refresh();
        }

        public int Min
        {
            get { return min; }
            set
            {
                if (value <= max)
                {
                    min = value;
                }
            }
        }
        public int Max
        {
            get { return max; }
            set
            {
                if (min < value)
                {
                    max = value;
                }
            }
        }

        public int colheight = 22;
        public int ColHeight
        {
            get { return colheight; }
            set {
                if (colheight != value)
                {
                    colheight = value;
                    //     ucThumbWheel_Resize(null, null);
                    Height = ColHeight;// 22 + this.Padding.Top + this.Padding.Bottom;
                    Width = NumWidth * ColWidth + 10;
                }
            }
        }
        public int ColWidth
        {
            get { return colheight * 3 / 4; }
        }

        public bool match = false;

        private void ucThumbWheel_Paint(object sender, PaintEventArgs e)
        {
            //        timer.Interval = 100;
            Graphics g = e.Graphics;
            int v = Value;
            if (v < 0) v = -v;
            match = true;
            int gx = Width - 8;
            for (int x = 0; x < NumWidth; x++)
            {
                if (fm(x) == '-')
                {
                    if (selcol == -1)//マウス操作中は自動で動かない
                    {
                        float dif = ((Value < 0) ? 0f : 1f) - pos[x];
                        if (Math.Abs(dif) < 0.05) dif = 0;
                        else match = false;
                        pos[x] += dif * 0.2f;
                        if (pos[x] < 0) pos[x] = 0;
                        if (pos[x] > 1) pos[x] = 1f;
                    }
                    gx -= ColWidth;
                    g.DrawImage(roterS, gx,
                       +pos[x] * ColHeight - ColHeight * 2);
                }
                else if (fm(x) == '6' || fm(x) == '0')
                {//数値
                    int num = 10;
                    Bitmap img = roter;
                    if (fm(x) == '6')
                    {
                        num = 6;
                        img = roter6;
                    }
                    int n = v % num;
                    v = (v - n) / num;
                    if (selcol == -1)//マウス操作中は自動で動かない
                    {
                        if (pos[x] < (-0.5f)) pos[x] += num;
                        if (pos[x] > ((float)num - 0.5f)) pos[x] -= num;
                        float dif = n - pos[x];
                        if (dif > num / 2) dif -= num;
                        if (dif < -num / 2) dif += num;
                        if (dif < -2f) dif = -2f;
                        if (dif > 2f) dif = 2f;
                        if (Math.Abs(dif) < 0.01) { pos[x] += dif; }
                        else { match = false;
                            pos[x] += dif * 0.2f; }
                        if (pos[x] < (-0.5f)) pos[x] += num;
                        if (pos[x] > ((float)num - 0.5f)) pos[x] -= num;

                    }
                    gx -= ColWidth;
                    g.DrawImage(img, gx, +pos[x] * ColHeight - ColHeight * num);

                }
                else
                {
                    int y = -ColHeight;
                    if (fm(x) == ':')
                    {
                        y *= 3;
                    }
                    if (fm(x) == '.')
                    {
                        y *= 4;
                    }
                    if (fm(x) == '\'')
                    {
                        y *= 5;
                    }
                    if (fm(x) == '\"')
                    {
                        y *= 6;
                    }
                    if (fm(x) == '°')
                    {
                        y *= 7;
                    }
                    gx -= ColWidth / 2;
                    g.DrawImage(roterC, gx, y);
                }
                if (selcol == x)
                {
                    g.DrawRectangle(Pens.Orange, gx, 0, ColWidth - 2, ColHeight - 6);
                }
            }

            if (match == false)
            {
                if (timer.Enabled != true) timer.Enabled = true;
            }
            else if (timer.Enabled == true) timer.Enabled = false;

        }

        private void ucThumbWheel_Resize(object sender, EventArgs e)
        {
            Height = ColHeight;

            roterC = createRoterC();
            roter = createRoter(10);
            roter6 = createRoter(6);
            roterS = createRoterS();
            int gx = 0;
            for (int c = 0; c < NumWidth; c++)
            {
                if (fm(c) == '-' || fm(c) == '6' || fm(c) == '0')
                {
                    gx += ColWidth;
                }
                else
                {
                    gx += ColWidth / 2;
                }
            }
            Width = gx + 10;
            this.Refresh();
        }

        int _selcol = -1;
        int selcol
        {
            get {
                return _selcol;
            }
            set
            { if (_selcol != value)
                {
                    _selcol = value;
                    this.Refresh();
                }
            }
        }

        // マウスホイールイベント  
        private void _MouseWheel(object sender, MouseEventArgs e)
        {
            // スクロール量（方向）の表示  
            //  = (e.Delta ).ToString();

            int num = 10;
            if (fm(selcol) == '6') num = 6;
            if (selcol < 0) return;
            if (selcol > NumWidth) return;
            float ydif = -(float)e.Delta / 30;// e.Y - md.Y;
            pos[selcol] += (float)ydif / colheight;
            if (consoledbg) Console.WriteLine(pos[selcol]);
            if (fm(selcol) == '-')
            {
                if (pos[selcol] < 0f) pos[selcol] = 0f;
                if (pos[selcol] > 1f) pos[selcol] = 1f;
                this.Refresh();
                return;
            }

            if (pos[selcol] < 0) pos[selcol] += num;
            if (pos[selcol] > num) pos[selcol] -= num;
            int col = selcol;
            while (pos[col] > num - 1 && col + 1 < NumWidth)
            {
                col++;
                if (fm(col) == '-') break;
                num = 1;
                if (fm(col) == '6') num = 6;
                else if (fm(col) == '0') num = 10;
                else continue;

                if (pos[col] < 0) pos[col] += num;
                if (pos[col] > num) pos[col] -= num;
                //    if (pos[col] > num-1)
                //    {
                //          pos[col] = (int)pos[col]+ pos[selcol]-(int)pos[selcol];
                //     }
                //    else
                {
                    pos[col] += (float)ydif / colheight;
                    if (pos[col] < 0) pos[col] += num;
                    if (pos[col] > num) pos[col] -= num;
                }
            }
            Value = getcurvalue();
            this.Refresh();

        }

        Point md;
        private void ucThumbWheel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (!Enabled) return;
            md = new Point(e.X, e.Y);
            //selcol = -(int)(e.X / ColWidth) - 1 + NumWidth;
            //(NumWidth - x - 1) * roter.Width;
            if (consoledbg) Console.WriteLine("selcol" + selcol.ToString());
            for (int col = 0; col < NumWidth; col++)
            {
                int num = 10;
                if (fm(col) == '0') num = 10;
                else if (fm(col) == '6') num = 6;
                else if (fm(col) == '-') num = 2;
                else
                {
                    pos[col] = 3;
                    continue;
                }
                if (pos[col] < -0) pos[col] += num;
                if (pos[col] > num) pos[col] -= num;
            }
        }

        private void ucThumbWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;
            if (e.Button != MouseButtons.Left)
            {
                int gx = Width - 8;
                for (int c = 0; c < NumWidth; c++)
                {
                    if (fm(c) == '-' || fm(c) == '6' || fm(c) == '0')
                    {
                        gx -= ColWidth;
                        if (e.X > gx)
                        {
                            selcol = c;
                            return;
                        }
                    }
                    else
                    {
                        gx -= ColWidth / 2;
                        if (e.X > gx) break;
                    }
                }
                selcol = -1;
                return;
            }
            int num = 10;
            if (fm(selcol) == '6') num = 6;
            if (selcol < 0) return;
            if (selcol > NumWidth) return;
            float ydif = (float)(e.Y - md.Y) / colheight;
            if (ydif > 0.2f) ydif = 0.2f;
            if (ydif < -0.2f) ydif = -0.2f;
            pos[selcol] += ydif;
            if (consoledbg) Console.WriteLine(pos[selcol]);
            if (fm(selcol) == '-')
            {
                if (pos[selcol] < 0f) pos[selcol] = 0f;
                if (pos[selcol] > 1f) pos[selcol] = 1f;
                md.Y = e.Y;
                this.Refresh();
                return;
            }

            if (pos[selcol] < 0) pos[selcol] += num;
            if (pos[selcol] > num) pos[selcol] -= num;
            int col = selcol;
            while (pos[col] > num - 1 && col + 1 < NumWidth)
            {
                col++;
                if (fm(col) == '-') break;
                num = 1;
                if (fm(col) == '6') num = 6;
                else if (fm(col) == '0') num = 10;
                else continue;

                //        if (pos[col] < 0) pos[col] += num;
                //        if (pos[col] > num) pos[col] -= num;
                //        if (pos[col] > num - 1)
                //        {
                //       float d =(int)( pos[col] + ydif- pos[selcol] );
                //   pos[col] = pos[selcol]+d;
                //       if (s - d > 0.5) pos[col] = (int)(pos[col] + s + 1;
                //       else if (s - d < -0.5) pos[col] = (int)pos[col] + s - 1;
                //     else 


                //       }
                //     else
                {
                    pos[col] += ydif;
                    if (pos[col] < 0) pos[col] += num;
                    if (pos[col] > num) pos[col] -= num;
                }
            }
            md.Y = e.Y;
            Value = getcurvalue();
            this.Refresh();
        }

        private void ucThumbWheel_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;
            int k = 1;
            int curval = 0;
            int v = Value;
            for (int x = 0; x < NumWidth; x++)
            {
                if (fm(x) == '-')
                {
                    if (pos[x] < 0f) pos[x] = 0;
                    if (pos[x] > 1f) pos[x] = 1f;
                    if (pos[x] < 0.5f) curval = -curval;
                    break;
                }
                int num = 10;
                if (fm(x) == '6') num = 6;
                else if (fm(x) == '0') num = 10;
                else continue;

                int n;// = v % num;
                      //      v = (v - n) / num;
                      //      if (selcol == x)
                {
                    if (pos[x] < -0.5f) pos[x] += num;
                    if (pos[x] > num - 0.5f) pos[x] -= num;
                    n = (int)(pos[x] + 0.5);
                }
                curval += k * n;
                k *= num;
            }
            Value = curval;
            if (consoledbg) Console.WriteLine(Value);
            selcol = -1;
            this.Refresh();
        }

        private int getcurvalue()
        {
            int curval = 0, n, k = 1;
            for (int x = 0; x < NumWidth; x++)
            {
                if (fm(x) == '-')
                {
                    if (pos[x] < 0f) pos[x] = 0;
                    if (pos[x] > 1f) pos[x] = 1f;
                    if (pos[x] < 0.5f) curval = -curval;
                    break;
                }
                int num = 10;
                if (fm(x) == '6') num = 6;
                else if (fm(x) == '0') num = 10;
                else continue;
                //      if (selcol == x)
                {
                    if (pos[x] < -0.5f) pos[x] += num;
                    if (pos[x] > num - 0.5f) pos[x] -= num;
                    n = (int)(pos[x] + 0.5);
                }
                curval += k * n;
                k *= num;
            }
            if (ValueChanged != null) ValueChanged(this, new IntEventArgs(curval));
            return curval;
        }

        private void ucThumbWheel_MouseLeave(object sender, EventArgs e)
        {
            selcol = -1;
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
            // ucThumbWheel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ucThumbWheel";
            this.Size = new System.Drawing.Size(148, 41);
            this.Load += new System.EventHandler(this.ucThumbWheel_Load);
            this.BackColorChanged += new System.EventHandler(this.ucThumbWheel_Resize);
            this.FontChanged += new System.EventHandler(this.ucThumbWheel_Resize);
            this.ForeColorChanged += new System.EventHandler(this.ucThumbWheel_Resize);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ucThumbWheel_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ucThumbWheel_MouseDown);
            this.MouseLeave += new System.EventHandler(this.ucThumbWheel_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ucThumbWheel_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ucThumbWheel_MouseUp);
            this.Resize += new System.EventHandler(this.ucThumbWheel_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}








