using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ucPgmac;
using System.Timers;
using System.IO;

//using Alea;
//using Alea.Parallel;

// alea ? https://kzmmtmt.pgw.jp/?p=1170
/*
 Alea GPUライブラリを使ってGPU並列処理をする
ソースファイル(Program.csなど)のusingディレクティブにusing Alea;とusing Alea.Parallel;を追加する。
Gpuインスタンスの取得はGpu.Defaultで可能である。
並列処理を行うにはGpuクラスのForメソッド(void Gpu.For(int from,int to,Action op))を用いる。
これによって(to-from)回繰り返しが行われ、Actionにはループカウンタの変数が渡される。
例えば、0～99までの数字をコンソールに出力するプログラムは普通に書くと
        for(int i=0;i<100;i++){Console.WriteLine(i);}であるが、
並列処理で書くと
        Gpu.Default.For(0,100,i=>{Console.WriteLine(i);});となる。
for文を使った場合は逐次処理されるので0から順番に99まで表示されるが、並列処理の場合は並列に実行されるので0から順番に表示される保証はない。


0～99までの数字をスペースで区切って表示するプログラムの例を示す。

using System;
using Alea;
using Alea.Parallel;

public static class Program{
  public static void Main(){
    var gpu=Gpu.Default; // GPUインスタンス取得
    gpu.For(0,100,i=>Console.Write("{0} ",i)); // 0～99までスペースで区切って出力
    Console.WriteLine("\n完了!");
  }
}

[実行結果]
完了!
96 97 98 99 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59 60 61 62 63 64 65 66 67 68 69 70 71 72 73 74 75 76 77 78 79 80 81 82 83 84 85 86 87 88 89 90 91 92 93 94 95
 
*/

/*How to compute planetary positions
 http://stjarnhimlen.se/comp/ppcomp.html


 */

namespace ucPgmac
{
    //http://tdc-www.harvard.edu/catalogs/index.html

    /// <summary>
    /// todo
    /// LST 等　時間表示
    /// 設定の表示の設定方法
    /// 
    /// need ucSwitch
    /// </summary>
    public partial class Tenqu : UserControl
    {
        public DateTime TargetTime = DateTime.Now;



        public wakusei eclipse = new wakusei();
        private Stopwatch sw = new Stopwatch();
        private Stopwatch swRefresh = new Stopwatch();
        public string basepath;
        public StReader reader = new StReader() ;
        public milkyway MilkeyWay;


        //MousePosition
        Quaternion Mpos = new Quaternion();

        double AZMITH = 0;  //マウスのさす高度方位
        double ALTITUDE = 0;
        double RA = 0;      //天球座標
        double DEC = 0;     //天球座標
        double RAMouse = 0; //天球座標
        double DECMouse = 0;//天球座標

        //マウスポインターで表示する情報(+ctrlで更新)
        double MinRange = 200;
        int StarMouse = -1;
        int NBRMouse = -1;
        st_NGC[] ngcs ;
        st_NGC ngc = new st_NGC();
        string ngcdraw = "";

        RectangleF InfoStar = new RectangleF();
        RectangleF InfoNGC = new RectangleF();

        //処理時間測定
        long elapsNGCs = 0;
        long elapsHips = 0;
        long elapsMilkeyWay = 0;
        
        private System.Timers.Timer aTimer;
        
        //表示拡大率
        float ScreenScale = 180.0f;
        float Magnitude = 1.0f;
        float FontMagnitude = 1.0f;//(+ctrl wheel)

        //姿勢変換
        Quaternion tenqu = new Quaternion(0, 0, 1, 0);
        Quaternion horiq = new Quaternion(0, 0, 1, 0);

        //天球メッシュ頂点座標　
        const int DECCNT = 37;
        const int RACNT = 96;
        //X,Z平面　XはそのままCOS((int)(deg/10))なのでscaleとして使用できる?
        Quaternion[,] ptArrQ = new Quaternion[DECCNT, RACNT];//1h づつ０hから24hまで
        Quaternion[,] ptArrV = new Quaternion[DECCNT, RACNT];//[DEC/5,RA/4]

        //水平線表示用
        Quaternion[] ptArrH = new Quaternion[RACNT];//0度　の　部分　1h づつ０hから24hまで

        //黄道表示用
        Quaternion[] ptArrEcliptic = new Quaternion[RACNT];

        Quaternion[,] Hipstars;
        Quaternion[,] Bscstars;

        Vector2 Center = new Vector2(100, 100);

        double AzmithFor = 0;
        double AltitudeFor =0;

        double viewAzmith = 0;
        double viewAltitude = 0;

        public Cel cel
        {
            get; set;
        }
        public double hourOffset = 0;
        private NumericUpDown TimeZone;
        public int dateOffset = 0;

        // equatorial 赤道　赤道儀式の　
        // equatorial coordinate system 赤道座標系  (24hFormat,+-90degFormat)
        // celestial coordinate 天球座標系   ()
        // galactic coordinates 銀河座標系   赤経12h49m、赤緯+27.4°を銀緯90°に、対赤道昇交点を銀経33°とする
        // supergalactic coordinate 超銀河座標系
        // ecliptic coordinate system 黄道座標系
        // 黄道の北極を北黄極（North Ecliptic Pole（NEP））と呼ぶ
        // NEPの赤道座標は（18h00m00.s, +66°33’38.”6）(J2000)、また銀河座標は（96.°3840, +29.°8114）(J2000) 
        //"\viewstars\datas\eclongal.png"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void readDatas(string path)
        {
            if (Directory.Exists(path)) // dirPathのディレクトリは存在する
            {
                //Hips;
                //Bsc5;
                MilkeyWay = new milkyway(path + @"\milkyway.csv");
                basepath = path;
                reader.NgcRead(basepath + @"\ic.csv");
                reader.ICs = reader.NGCs;
                reader.ICs_index = reader.NGCs_index;
                reader.NGCs_index = new List<int>();

                reader.NgcRead(basepath + @"\ngc.csv");
                reader.Ngc2000Read(basepath + @"\VII118\ngc2000.dat");
                reader.NgcNameRead(basepath + @"\VII118\names.dat");
                
                reader.Hips_index.Clear();
                reader.Bsc5Read(basepath + @"\bsc5.dat");
                
                reader.Hip100Read(basepath + @"\hip_100.csv");
                reader.HipRead(basepath + @"\hipparcos");
                reader.HipNameRead(basepath + @"\hip_proper_name.csv");
                reader.HipLineRead(basepath + @"\hip_constellation_line.csv");

                // lstNgcs.Add(reader.NGC2000s);
                // lstNgcs.Add(reader.IC2000s);

                // lstStars.Add(reader.Hips);
                // tenqu1.lstStars.Add(reader.Bsc5);

                Hipstars = getstars(reader.Hips);
                Bscstars = getstars(reader.Bsc5);
            }
        }

        public Quaternion[,] getstars(st_Star[] stars)
        {
            Quaternion[,] res = new Quaternion[stars.Length, 2];

            for (int i = 0; i < stars.Length; i++)
            {
                st_Star s = stars[i];
                res[i, 0] = s.Pos.Quat;

            }
            return res;
        }

        public Tenqu()
        {
            InitializeComponent();

            //ホイールイベントの追加  
            this.MouseWheel
                += new System.Windows.Forms.MouseEventHandler(this.this_MouseWheel);

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(30);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;

            // Hook up the Elapsed event for the timer. 
            //  aTimer.Enabled = true;


            this.DoubleBuffered = true;
            /*
            //キー（HKEY_CURRENT_USER\Software\test\sub）を読み取り専用で開く+
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\" + Application.ProductName + @"\tenqu", false);
            //キーが存在しないときは null が返される
            if (regkey == null)
            {
                 regkey =
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + Application.ProductName + @"\tenqu");
                //上のコードでは、指定したキーが存在しないときは新しく作成される。
                folderBrowserDialog1.SelectedPath = AppContext.BaseDirectory;
                if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
                {
                    regkey.SetValue("DataPath", folderBrowserDialog1.SelectedPath);
                }
            }

            //文字列を読み込む
            //指定した名前の値が存在しないときは null が返される
            string DataPath = (string)regkey.GetValue("DataPath", @"C:\Users\pg\source\repos\viewstars\datas");
            //キーに値が存在しないときに指定した値を返すようにするには、次のようにする
            //（ここでは"default"を返す）
            //string stringValue = (string) regkey.GetValue("string", "default");
            readDatas(DataPath);
            */
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (trace)
            {
                double difalt = Cel.Map180(-AltitudeFor - viewAltitude );
                double difazm = Cel.Map180(AzmithFor - viewAzmith);
                double len = Math.Sqrt(difalt* difalt  + difazm* difazm);
                if (Math.Abs(len) > 0.8) len = 0.8;
              //  if (difalt > 5) difalt = 5;
              //  if (difalt < -5) difalt = -5;
                viewAltitude += difalt*len*0.002* aTimer.Interval;

                //  if (difazm > 5) difazm =5;  
                //  if (difazm < -5) difazm = -5;
                viewAzmith += difazm * len * 0.002 * aTimer.Interval;
     //           if (Math.Abs(len) < 0.001 && Move.Checked==false) //difalt == 0 &&
       //             trace = false;

                tenqu_update();
         //       Console.Write("diff " + difalt.ToString("0.00") + " " + difalt.ToString("0.00"));
            }
            if (refreshlap > aTimer.Interval) aTimer.Interval = refreshlap + 100;
            if (refreshlap+100 < aTimer.Interval) aTimer.Interval = refreshlap + 100;
            if (needRefresh) tenqu_refresh();
            //  Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        }

        private void Tenqu_Load(object sender, EventArgs e)
        {
            //       trace = Move.Checked;

            this.DoubleBuffered = true;

            //キー（HKEY_CURRENT_USER\Software\test\sub）を読み取り専用で開く+
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\" + Application.ProductName + @"\tenqu", false);
            //キーが存在しないときは null が返される
            if (regkey == null)
            {
                regkey =
                   Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + Application.ProductName + @"\tenqu");
                //上のコードでは、指定したキーが存在しないときは新しく作成される。
                folderBrowserDialog1.SelectedPath = AppContext.BaseDirectory;
                if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
                {
                    regkey.SetValue("DataPath", folderBrowserDialog1.SelectedPath);
                }
            }

            //文字列を読み込む
            //指定した名前の値が存在しないときは null が返される
            string DataPath = (string)regkey.GetValue("DataPath", @"C:\Users\pg\source\repos\viewstars\datas");
            //キーに値が存在しないときに指定した値を返すようにするには、次のようにする
            //（ここでは"default"を返す）
            //string stringValue = (string) regkey.GetValue("string", "default");
            readDatas(DataPath);

            NGC名称.Checked = bool.Parse((string)regkey.GetValue("NGC名称", "false"));
            NGC表示.Checked = bool.Parse((string)regkey.GetValue("NGC表示", "true"));
            太陽系.Checked = bool.Parse((string)regkey.GetValue("太陽系", "true"));
            黄道.Checked = bool.Parse((string)regkey.GetValue("黄道", "true"));
            時間表示.Checked = bool.Parse((string)regkey.GetValue("時間表示", "true"));
            LST表示.Checked = bool.Parse((string)regkey.GetValue("LST表示", "true"));
            ステレオ投影.Checked = bool.Parse((string)regkey.GetValue("ステレオ投影", "true"));
            和名.Checked = bool.Parse((string)regkey.GetValue("和名", "true"));
            Laps.Checked = bool.Parse((string)regkey.GetValue("Laps", "true"));
            座標.Checked = bool.Parse((string)regkey.GetValue("座標", "true"));
            地平線.Checked = bool.Parse((string)regkey.GetValue("地平線", "true"));
            星座.Checked = bool.Parse((string)regkey.GetValue("星座", "true"));
            天の川.Checked = bool.Parse((string)regkey.GetValue("天の川", "false"));
            恒星名称.Checked = bool.Parse((string)regkey.GetValue("恒星名称", "true"));
            NGCNo.Checked = bool.Parse((string)regkey.GetValue("NGCNo", "false"));
            NGCtype.Checked = bool.Parse((string)regkey.GetValue("NGC", "true"));
            恒星_BSC5.Checked = bool.Parse((string)regkey.GetValue("恒星_BSC5", "false"));
            座標線.Checked = bool.Parse((string)regkey.GetValue("座標線", "true"));
            恒星_HIP.Checked = bool.Parse((string)regkey.GetValue("恒星_HIP", "true"));
            double tz = double.Parse((string)regkey.GetValue("TimeZone", "9.0"));
            TimeZone.Value = (decimal)tz;
            Cel cel = new Cel();
            //        cel.Latitude = 35.345500117631445;
            //        cel.Longitude = -137.1579632651267;

            cel.Latitude = double.Parse((string)regkey.GetValue("cel.Latitude", "35.345500117631445"));
            cel.Longitude = double.Parse((string)regkey.GetValue("cel.Longitude", "-137.1579632651267"));

            //    Width = (int)regkey.GetValue("Width", 640);
            //    Height = (int)regkey.GetValue("Height", 480);

            this.cel = cel;

            Center = new Vector2(Width / 2, Height / 2);
            createCeleLines();
            //       Move.Checked = true;
            eclipse.set2Now();
            tenqu_update();
            tenqu_refresh();
            aTimer.Enabled = true;

        }

        private void Tenqu_savesetting()
        {
            //キー（HKEY_CURRENT_USER\Software\test\sub）を読み取り専用で開く+
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\" + Application.ProductName + @"\tenqu", true);
            //キーが存在しないときは null が返される
            if (regkey != null)
            {

                regkey.SetValue("NGC名称", NGC名称.Checked);
                regkey.SetValue("NGC表示", NGC表示.Checked);
                regkey.SetValue("太陽系", 太陽系.Checked);
                regkey.SetValue("黄道", 黄道.Checked);
                regkey.SetValue("時間表示", 時間表示.Checked);
                regkey.SetValue("LST表示", LST表示.Checked);
                regkey.SetValue("ステレオ投影", ステレオ投影.Checked);
                regkey.SetValue("和名", 和名.Checked);
                regkey.SetValue("Laps", Laps.Checked);
                regkey.SetValue("座標", 座標.Checked);
                regkey.SetValue("地平線", 地平線.Checked);
                regkey.SetValue("星座", 星座.Checked);
                regkey.SetValue("天の川", 天の川.Checked);
                regkey.SetValue("恒星名称", 恒星名称.Checked);
                regkey.SetValue("NGCNo", NGCNo.Checked);
                regkey.SetValue("NGC", NGCtype.Checked);
                regkey.SetValue("恒星_BSC5", 恒星_BSC5.Checked);
                regkey.SetValue("座標線", 座標線.Checked);
                regkey.SetValue("恒星_HIP", 恒星_HIP.Checked);
                regkey.SetValue("TimeZone", TimeZone.Value);
                regkey.SetValue("cel.Latitude", cel.Latitude);
                regkey.SetValue("cel.Longitude", cel.Longitude);
          //      regkey.SetValue("Width", Width);
          //      regkey.SetValue("Height", Height);


            }



        }
        #region CeleLines
        /// <summary>
        /// 
        /// </summary>
        private void createCeleLines()
        {
            Quaternion[] ptArrDec = new Quaternion[DECCNT];//5deg づつ0度から180度(north)まで
            Quaternion rt1h = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(Math.PI * 2 / RACNT));

            Quaternion rt5deg = Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), (float)(Math.PI / (DECCNT-1)));
            Quaternion Conj = Quaternion.Conjugate(rt5deg);

            //天球を前north　後ろSouthに配置して　X　春分方向
            ptArrDec[0] = new Quaternion(  0,0,1, 0);//北極軸
            //赤緯0度の北極から南極までのベクトル配列
            for (int i = 1; i < DECCNT; i++)
            {
                ptArrDec[i] = rt5deg * ptArrDec[i - 1] * Conj;
            }
            //-----------------------------------
            Quaternion rtXh = new Quaternion(0, 0, 1, 0);
            for (int j = 0; j < RACNT; j++)
            {
                Conj = Quaternion.Conjugate(rtXh);
                for (int i = 0; i < DECCNT; i++)
                {
                    ptArrQ[i , j] = rtXh * ptArrDec[i] * Conj;
                }
                rtXh = rt1h * rtXh;
            }
        }

        private void rotateCeleLines(Quaternion Q)
        {
            Quaternion Conj = Quaternion.Conjugate(Q);
            //回転処理
            if (ステレオ投影.Checked)
            {
                for (int j = 0; j < RACNT; j++)
                {
                    for (int i = 0; i < DECCNT; i++)
                    {
                        ptArrV[i, j] = Q * ptArrQ[i, j] * Conj;
                        float dz = 2f / (1f + ptArrV[i, j].Z) * ScreenScale; //*= dz;
                        ptArrV[i, j].X *= dz;
                        ptArrV[i, j].Y *= dz;
                    }
                }

            }
            else
            {
                for (int j = 0; j < RACNT; j++)
                {
                    for (int i = 0; i < DECCNT; i++)
                    {
                        ptArrV[i, j] = Q * ptArrQ[i, j] * Conj;
                        ptArrV[i, j].X *= ScreenScale;
                        ptArrV[i, j].Y *= ScreenScale;

                    }
                }
            }
        }


        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="Location">ユーザーコントロールの左上を原点とした画面座標(マウス座標)</param>
        /// <returns></returns>
        public Quaternion transformScreenToEquatorial(Point location)
        {

            Quaternion Vpos;
            double xx = (location.X - this.Width / 2) / (ScreenScale);
            double yy = (location.Y - this.Height / 2) / (ScreenScale);
            double len = Math.Sqrt(1.0 - xx * xx - yy * yy);

            if (!ステレオ投影.Checked)
            {
                Vpos = new Quaternion((float)xx, (float)yy, (float)len, 0); //　単位ベクトル
            }
            else
            {   //ステレオ投影
                xx *= 0.5f;
                yy *= 0.5f;//　表示が２倍されている
                len = (1.0 + xx * xx + yy * yy);
                double zz = (-1.0 + xx * xx + yy * yy);
                xx = 2 * xx / len;
                yy = 2 * yy / len;
                zz = zz / len;
                Vpos = new Quaternion((float)xx, (float)yy, -(float)zz, 0);
            }
            return Vpos;
            /*
            Quaternion Choriq = Quaternion.Conjugate(horiq);
            Mpos = (Choriq * Vpos * horiq);
            ALTITUDE = (Math.Atan2(Mpos.Z, Math.Sqrt(1.0 - Mpos.Z * Mpos.Z)) * 180.0 / Math.PI);
            AZMITH = Cel.Map360(-Math.Atan2(Mpos.X, Mpos.Y) * 180.0 / Math.PI);

            Quaternion VposMouse = Quaternion.Conjugate(tenqu) * Vpos * tenqu;
            RAMouse = Cel.Map24(-Math.Atan2(VposMouse.X, VposMouse.Y) * 12.0 / Math.PI - 6.0);
            double l = Math.Sqrt(1 - VposMouse.Z * VposMouse.Z);
            DECMouse = Math.Atan2(VposMouse.Z, l) * 180.0 / Math.PI;

            return 0;
            */
        }

        private float getZclip(Point location =  new Point())
        {
     //       location = new Point(-Width / 3, -Height / 3);
            location = new Point();

            //    Quaternion Vpos;
            double xx = (location.X - this.Width / 2) / (ScreenScale);
            double yy = (location.Y - this.Height / 2) / (ScreenScale);
            //    double len = Math.Sqrt(1.0 - xx * xx - yy * yy);

            if (!ステレオ投影.Checked)
            {
                //   Vpos = new Quaternion((float)xx, (float)yy, (float)len, 0); //　単位ベクトル
                double d = 1.0 - xx * xx - yy * yy;
                if (d > 0)
                {
                    return (float)Math.Sqrt(d);
                }
                else
                {
                    return 0;
                }
            }
            else
            {   //ステレオ投影
                xx *= 0.5f;
                yy *= 0.5f;//　表示が２倍されている
                double len = (1.0 + xx * xx + yy * yy);
                double zz = (-1.0 + xx * xx + yy * yy);
                xx = 2 * xx / len;
                yy = 2 * yy / len;
                zz = zz / len;

                return -(float)zz;
                //            Vpos = new Quaternion((float)xx, (float)yy, -(float)zz, 0);
            }
    //        return Vpos;
        }
        //姿勢回転
        private void drawCeleLines(Graphics g)
        {
            Pen penH = new Pen(Color.FromArgb(120, 100, 200, 200));
            Pen penHq = new Pen(Color.FromArgb(80, 100, 200, 200));

            Pen pen ;
            Font font = new Font(FontFamily.GenericSansSerif, 15F* FontMagnitude);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Center;  // 上下方向は中心寄せ

            //描画処理　赤経線 
            int lpnskip =  RACNT / 12;

            float Zclip = getZclip(new Point(-Width/3,-Height/3));
            int i = 0;
            int k = 0;
            int st = 2; //描き始め位置

            if (Magnitude > 30) {
                lpnskip = RACNT / 96;
                k = 0;
                st = 1;
            }
            else if (Magnitude > 15)
            {
                lpnskip = RACNT / 48;
                k = 0;
                st = 2;
            }
            else if (Magnitude > 5)
            {
                lpnskip = RACNT / 24;
                k = 0;
                st = 2;
            }

            for (; k < RACNT; k += lpnskip)
            {
                i = st; //描き始め位置 北極付近には線を引かない
                int j = k;
                {
                    if(k % 4 ==0)
                        pen = penH;
                    else
                        pen = penHq;

                    float X0 = ptArrV[i, j].X;
                    float Y0 = ptArrV[i, j].Y;
                    float Z0 = ptArrV[i, j].Z;

                    bool prev_insight = (X0 > -Width * 0.45f && X0 < Width * 0.44f && Y0 > -Height * 0.45f && Y0 < Height * 0.45f);
                    for ( i = st; i < DECCNT - st; i++)
                    {
                        float X = ptArrV[i, j].X;
                        float Y = ptArrV[i, j].Y;
                        float Z = ptArrV[i, j].Z;

                        if (Z > Zclip || Z0 > Zclip)
                        {
                            g.DrawLine(pen, X0 , Y0 , X , Y );

                            if (座標.Checked )
                            {
                                bool insight = (X > -Width *0.45f && X < Width * 0.44f && Y > -Height * 0.45f && Y < Height * 0.45f);
                                
                                if(prev_insight != insight)
                                {
                                    string sth = (Cel.Map24(k * 24 / RACNT + 12)).ToString() + "h";

                                    int min = (k % 4) * 15;
                                    if (min > 0) sth += min.ToString() + "m";
                                    float deg = (float)(Math.Atan2(Y0 - Y, X0 - X) * 180 / Math.PI);
                                    float p = 0;

                                    if (insight)
                                    {   //X0 or Y0 is Outside 
                                        if (X0 < -Width * 0.45f)
                                        {
                                            p = (-Width * 0.45f - X0) / (X - X0);
                                        }
                                        else if (X0 > Width * 0.44f)
                                        {
                                            p = (Width * 0.44f - X0) / (X - X0);
                                        }
                                        else if (Y0 < -Height * 0.45f)
                                        {
                                            p = (-Height * 0.45f - Y0) / (Y - Y0);
                                        }
                                        else if (Y0 > Height * 0.45f)
                                        {
                                            p = (Height * 0.45f - Y0) / (Y - Y0);
                                        }
                                    }
                                    else
                                    {   //X or Y is Outside 
                                        if (X < -Width * 0.45f)
                                        {
                                            p = (-Width * 0.45f - X0) / (X - X0);
                                        }
                                        else if (X > Width * 0.44f)
                                        {
                                            p = (Width * 0.44f - X0) / (X - X0);
                                        }
                                        else if (Y < -Height * 0.45f)
                                        {
                                            p = (-Height * 0.45f - Y0) / (Y - Y0);
                                        }
                                        else if (Y > Height * 0.45f)
                                        {
                                            p = (Height * 0.45f - Y0) / (Y - Y0);
                                        }
                                    }
                                    DrawString(g, sth, font, Brushes.SkyBlue, X0 * (1f - p) + X * p, Y0 * (1f - p) + Y * p, deg, format);
                                    prev_insight = insight;

                                }
                            }
                        }
                        X0 = X;
                        Y0 = Y;
                        Z0 = Z;
                    }
                }
            }

            pen = penH;
            //北極軸
            {
                float X0 = ptArrV[0, 0].X;
                float Y0 = ptArrV[0, 0].Y;
                float Z0 = ptArrV[0, 0].Z;

                float X1 = (ptArrV[1, 0].X - X0) / 10;
                float Y1 = (ptArrV[1, 0].Y - Y0) / 10;
                float Z1 = ptArrV[1, 0].Z;
                if (Z0 > Zclip && Z1 > Zclip)
                {
                    g.DrawLine(pen, X0, Y0, (X0 + X1), (Y0 + Y1));
                    g.DrawLine(pen, X0, Y0, (X0 - X1), (Y0 - Y1));
                    g.DrawLine(pen, X0, Y0, (X0 + Y1), (Y0 - X1));
                    g.DrawLine(pen, X0, Y0, (X0 - Y1), (Y0 + X1));

                    g.DrawLine(pen, X0, Y0, ptArrV[1, 0].X, ptArrV[1, 0].Y);
                    g.DrawLine(pen, X0, Y0, ptArrV[1, RACNT / 4].X, ptArrV[1, RACNT / 4].Y);
                    g.DrawLine(pen, X0, Y0, ptArrV[1, RACNT / 2].X, ptArrV[1, RACNT / 2].Y);
                    g.DrawLine(pen, X0, Y0, ptArrV[1, RACNT / 4*3].X, ptArrV[1, RACNT / 4 * 3].Y);
                }
            }


            //描画処理　赤緯線
            st = 2;
            int step = 4;
            if (Magnitude > 30)//30-
            {
                st = 1;
                step = 1;
            }
            else if (Magnitude > 15)//15-30
            {
                st = 2;
                step = 1;
            }
            else if (Magnitude > 5)//5-15
            {
                st = 2;
                step = 2;
            }

            for (i=st; i < 37-st; i+=step)
            {
                if (i % 2 == 0)
                    pen = penH;
                else
                    pen = penHq;

                float X0 = ptArrV[i, RACNT - 1].X;
                float Y0 = ptArrV[i, RACNT - 1].Y;
                float Z0 = ptArrV[i, RACNT - 1].Z;
                bool prev_insight  = (X0 > -Width * 0.43f && X0 < Width * 0.42f && Y0 > -Height * 0.43f && Y0 < Height * 0.43f);
                for (int j = 0; j < RACNT; j++)
                {
                    float X = ptArrV[i, j].X;
                    float Y = ptArrV[i, j].Y;
                    float Z = ptArrV[i, j].Z;
                    if (Z > Zclip || Z0 > Zclip)
                    {
                        g.DrawLine(pen, X0 , Y0 , X , Y );

                        if (座標.Checked)
                        {
                            bool insight = (X > -Width * 0.43f && X < Width * 0.42f && Y > -Height * 0.43f && Y < Height * 0.43f);

                            if (prev_insight != insight)
                            {
                                int dec = 90 - i * 180 / (DECCNT - 1);
                                string std = (dec>0? "+":"" )+ dec.ToString() + "°";

                                float deg = (float)(Math.Atan2(Y0 - Y, X0 - X) * 180 / Math.PI);
                                float p = 0;

                                if (insight)
                                {   //X0 or Y0 is Outside 
                                    if (X0 < -Width * 0.43f)
                                    {
                                        p = (-Width * 0.43f - X0) / (X - X0);
                                    }
                                    else if (X0 > Width * 0.42f)
                                    {
                                        p = (Width * 0.42f - X0) / (X - X0);
                                    }
                                    else if (Y0 < -Height * 0.43f)
                                    {
                                        p = (-Height * 0.43f - Y0) / (Y - Y0);
                                    }
                                    else if (Y0 > Height * 0.43f)
                                    {
                                        p = (Height * 0.43f - Y0) / (Y - Y0);
                                    }
                                }
                                else
                                {   //X or Y is Outside 
                                    if (X < -Width * 0.43f)
                                    {
                                        p = (-Width * 0.43f - X0) / (X - X0);
                                    }
                                    else if (X > Width * 0.42f)
                                    {
                                        p = (Width * 0.42f - X0) / (X - X0);
                                    }
                                    else if (Y < -Height * 0.43f)
                                    {
                                        p = (-Height * 0.43f - Y0) / (Y - Y0);
                                    }
                                    else if (Y > Height * 0.43f)
                                    {
                                        p = (Height * 0.43f - Y0) / (Y - Y0);
                                    }
                                }
                                DrawString(g, std, font, Brushes.SkyBlue, X0 * (1f - p) + X * p, Y0 * (1f - p) + Y * p, deg, format);
                                prev_insight = insight;
                            }
                        }
                    }
                    X0 = X;
                    Y0 = Y;
                    Z0 = Z;
                }
            }

            {//　赤緯０度
                i = (DECCNT-1)/2;
                float X0 = ptArrV[i, RACNT - 1].X;
                float Y0 = ptArrV[i, RACNT - 1].Y;
                float Z0 = ptArrV[i, RACNT - 1].Z;
                for (int j = 0; j < RACNT; j++)
                {
                    float X = ptArrV[i, j].X;
                    float Y = ptArrV[i, j].Y;
                    float Z = ptArrV[i, j].Z;

                    if (Z > Zclip && Z0 > Zclip)
                    {
                        g.DrawLine(Pens.Red, X0 , Y0 , X , Y );
                    }
                    X0 = X;
                    Y0 = Y;
                    Z0 = Z;
                }
            }
            pen = penH;

            penH.Dispose();
            penHq.Dispose();
        }

        private void drawHrizon(Graphics g)
        {
            //地平線描画

            Quaternion Conj = Quaternion.Conjugate(horiq);
            //回転処理
            int h = (DECCNT - 1) / 2;
            if (ステレオ投影.Checked)
            {
                for (int j = 0; j < RACNT; j++)
                {
                    ptArrH[j] = horiq * ptArrQ[h, j] * Conj;
                    float dz = 2f / (1f + ptArrH[j].Z) * ScreenScale; //*= dz;
                    ptArrH[j].X *= dz;
                    ptArrH[j].Y *= dz;
                }
            }
            else
            {
                for (int j = 0; j < RACNT; j++)
                {
                    ptArrH[j] = horiq * ptArrQ[h, j] * Conj;
                    ptArrH[j].X *= ScreenScale;
                    ptArrH[j].Y *= ScreenScale;
                }
            }
            float X0 = ptArrH[RACNT - 1].X;
            float Y0 = ptArrH[RACNT - 1].Y;
            float Z0 = ptArrH[RACNT - 1].Z;

            float Zclip = getZclip(new Point(-Width / 3, -Height / 3));
            for (int j = 0; j < RACNT; j++)
            {
                float X = ptArrH[j].X;
                float Y = ptArrH[j].Y;
                float Z = ptArrH[j].Z;

                if (Z > Zclip || Z0 > Zclip)
                {
                    g.DrawLine(new Pen(Color.FromArgb(100, Color.LimeGreen), 10), X0, Y0, X, Y);
                }
                X0 = X;
                Y0 = Y;
                Z0 = Z;
            }

            Font font = new Font(FontFamily.GenericSansSerif, 10f * FontMagnitude);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Far;  // 上下方向は中心寄せ            //描画処理　赤経線

            string[] strazm = new string[] { "E", "NE", "N", "NW", "W", "SW", "S", "SE" };
            int stp = RACNT / 8;
            int i = 0;
            for (int ind = 0; ind < RACNT; ind += stp)
            {
                float X = ptArrH[ind].X;
                float Y = ptArrH[ind].Y;
                float Z = ptArrH[ind].Z;

                if (Y > Height / 2 - 10) Y = Height / 2 - 10;
                if (Y < -Height / 2 + 30) Y = -Height / 2 + 30;

                if (ptArrH[ind].Z > Zclip)
                {
                    g.DrawString(strazm[i], font, Brushes.Yellow, X, Y, format);
                }
                i++;
            }

            font.Dispose();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="eclipticVec"> NEPの赤道座標は（18h00m00.5s, +66°33’38.”6）</param>
        private void drawEcliptic(Graphics g, Quaternion eclipticVec = new Quaternion())
        {
            Quaternion nep = PointCel.ToQuat(18.0 + 0.5 / 3600, 66.0 + 33 / 60 + 38.6 / 3600);
            //  NEP == 黄道の北極を北黄極
            //      Vector3 NEP = new Vector3(nep.X , nep.Y , nep.Z);
            //
            //      Quaternion P = PointCel.ToQuat(0, 0);
            //      Quaternion Q = Quaternion.CreateFromAxisAngle(NEP, 2.0f * (float)(Math.PI / RACNT));
            //
            //      //配列作成用
            //      Quaternion Qx = Q;

            if (黄道.Checked)
            {
                //黄道傾斜
                Quaternion ecQ =Quaternion.CreateFromYawPitchRoll(0, -(float)wakusei.orbital_elements.ecl, 0);
                Quaternion ecQ2 = new Quaternion(0, 0, 1, 0) * Quaternion.Conjugate(nep);
                if (ecQ2 != ecQ) { }
                int h = (DECCNT - 1) / 2;//赤道リストインデックス
                for (int e = 0; e < RACNT; e++)
                {
                    ptArrEcliptic[e] = ecQ2 * ptArrQ[h, e] * Quaternion.Conjugate(ecQ);
                }

                //黄道線描画
                Quaternion Q = tenqu * ecQ;
                Quaternion Conj = Quaternion.Conjugate(Q);
                //回転処理
                if (ステレオ投影.Checked)
                {
                    for (int j = 0; j < RACNT; j++)
                    {
                        ptArrEcliptic[j] = Q * ptArrQ[h, j] * Conj;

                        float dz = 2f / (1f + ptArrEcliptic[j].Z) * ScreenScale; //*= dz;
                        ptArrEcliptic[j].X *= dz;
                        ptArrEcliptic[j].Y *= dz;
                    }
                }
                else
                {
                    for (int j = 0; j < RACNT; j++)
                    {
                        ptArrEcliptic[j] = Q * ptArrQ[h, j] * Conj;
                        ptArrEcliptic[j].X *= ScreenScale;
                        ptArrEcliptic[j].Y *= ScreenScale;
                    }
                }
                float X0 = ptArrEcliptic[RACNT - 1].X;
                float Y0 = ptArrEcliptic[RACNT - 1].Y;
                float Z0 = ptArrEcliptic[RACNT - 1].Z;

                float Zclip = getZclip(new Point(-Width / 3, -Height / 3));
                for (int j = 0; j < RACNT; j++)
                {
                    float X = ptArrEcliptic[j].X;
                    float Y = ptArrEcliptic[j].Y;
                    float Z = ptArrEcliptic[j].Z;

                    if (Z > Zclip || Z0 > Zclip)
                    {
                        g.DrawLine(new Pen(Color.FromArgb(100, Color.Yellow), 2), X0, Y0, X, Y);
                    }
                    X0 = X;
                    Y0 = Y;
                    Z0 = Z;
                }
            }
            if (太陽系.Checked)
            {
                //惑星アップデート

                /*
                float Zclip = getZclip(new Point(-Width / 3, -Height / 3));
                //太陽表示
                aInput.Latit = cel.Latitude;
                aInput.Longit = -cel.Longitude;
                aInput.RefLongit = 135;

                taiyo.SolPos(aInput, DateTime.Now.AddHours(hourOffset), ref aOutput);
                double t = (aOutput.hAngle) * Math.PI / 180;

                Quaternion taipos = new Quaternion(
                      (float)(-Math.Cos(t) * aOutput.CosD)
                    , (float)(-Math.Sin(t) * aOutput.CosD)
                    , (float)(-aOutput.SinD)
                    , 0f);

                //      Console.WriteLine("SinH " + aOutput.SinH.ToString("0.00") + " CosH " + aOutput.CosH.ToString("0.00"));
                //      Console.WriteLine("SinD " + aOutput.SinD.ToString("0.00") + " CosD " + aOutput.CosD.ToString("0.00"));
                //      Console.WriteLine("SinA " + aOutput.SinA.ToString("0.00") + " CosA " + aOutput.CosA.ToString("0.00"));

                Quaternion taiposnow = tenqu * taipos * Quaternion.Conjugate(tenqu);
                float sdz = ScreenScale;
                if (ucSwitch13.Checked)
                {
                    sdz *= 2f / (1f + taiposnow.Z);
                }
                taiposnow.X *= sdz;
                taiposnow.Y *= sdz;

                if (taiposnow.Z > Zclip)
                {
                    int re = 20;
                    float sz = 10 * re;
                    for (int s = 2; s < re; s++)
                    {
                        g.FillEllipse(new SolidBrush(Color.FromArgb(s * (20 / re), Color.Gold))
                              //    g.DrawEllipse(new Pen(Color.FromArgb(s * (200 / re), Color.Gold),11)
                              , (taiposnow.X  - sz / s)
                              , (taiposnow.Y  - sz / s)
                              , sz * 2 / s, sz * 2 / s);
                    }
                    //       g.FillEllipse(new SolidBrush(Color.FromArgb(80, Color.Orange))
                    //           , (taiposnow.X  - 50)
                    //           , (taiposnow.Y  - 50)
                    //           , 100, 100);
                }
                else
                {
                    g.DrawEllipse(new Pen(Color.Green, 10)
                        , (taiposnow.X  - 5)
                        , (taiposnow.Y  - 5)
                        , 10, 10);
                }
                */

                ///惑星表示/////////////////////////////////////////
                float sdz = ScreenScale;
                Quaternion Conj = Quaternion.Conjugate(tenqu);
                float Zclip = getZclip(new Point(-Width / 3, -Height / 3));

                if (eclipse.planets.Count > 0)
                {
                    foreach (wakusei.planets_item item in eclipse.planets)
                    {
                        Quaternion taipos = PointCel.ToQuat(item.elem.RA * 12 / Math.PI, item.elem.Dec * 180 / Math.PI);

                        Quaternion vp = tenqu * taipos * Conj;
                        if (ステレオ投影.Checked)
                        {
                            sdz = 2f / (1f + vp.Z) * ScreenScale;
                        }
                        else
                        {
                            sdz = ScreenScale;
                        }
                        vp.X *= sdz;
                        vp.Y *= sdz;
                        if (vp.Z > Zclip)
                        {
                            int re = item.size;
                            float sz = 2 * re;

                            sz = (float)item.elem.
                                d / 60f/15f * Magnitude;
                            if (sz < 10) sz = 8;
                            g.FillEllipse(new SolidBrush(Color.FromArgb(200, item.color))
                                          , (vp.X - sz / 2)
                                          , (vp.Y - sz / 2)
                                          , sz, sz);
                            string namen = item.Name; // クラス名取得
                            SizeF stsz  = g.MeasureString(namen, SystemFonts.DefaultFont);
                            g.DrawString(namen, SystemFonts.DefaultFont, Brushes.Goldenrod, vp.X - ( stsz.Width)*0.5f, vp.Y + stsz.Height + sz/2);

                    //        re = 0;
                    //       for (int s = 2; s < re; s++)
                    //       {
                    //           g.FillEllipse(new SolidBrush(Color.FromArgb(s * (200 / re), item.color))
                    //                 //    g.DrawEllipse(new Pen(Color.FromArgb(s * (200 / re), Color.Gold),11)
                    //                 , (vp.X - sz / s)
                    //                 , (vp.Y - sz / s)
                    //                 , sz * 2 / s, sz * 2 / s);
                    //           string name = item.Name; // クラス名取得
                    //           g.DrawString(name, SystemFonts.DefaultFont, Brushes.Goldenrod, vp.X, vp.Y + 10 + item.size * 1);
                    //       }
                        }
                        else
                        {
                            g.DrawEllipse(new Pen(Color.Red, 10)
                                , (vp.X - 5)
                                , (vp.Y - 5)
                                , 10, 10);
                        }
                    }
                }
            }
        }
        #endregion 

        //星を回転させる
        private void rotateStars(Quaternion Q, Quaternion[,] stars, float ScreenScale)//st_Star[] stars)
        {
            Quaternion CQ = Quaternion.Conjugate(Q);
            //     var gpu = Gpu.Default; // GPUインスタンス取得
            //     if (gpu != null)
            //     {
            //         gpu.For(0, stars.Length, i => rotatestar(i, Q, CQ, stars));
            //         // i => Console.Write("{0} ", i)); // 0～99までスペースで区切って出力
            //         //gpu.Synchronize(); // ここで同期する
            //     }
            //     else
            {
                if (ステレオ投影.Checked)
                {
                    for (int i = 0; i < stars.Length / 2; i++)
                    {
                        stars[i, 1] = Q * stars[i, 0] * CQ;
                        //stars[i].V = Q * stars[i].Pos.Quat * CQ ;
                        float dz = 2f / (1f + stars[i, 1].Z) * ScreenScale; //*= dz;
                        stars[i, 1].X *= dz;
                        stars[i, 1].Y *= dz;
                    }
                }
                else
                {
                    for (int i = 0; i < stars.Length / 2; i++)
                    {
                        stars[i, 1] = Q * stars[i, 0] * CQ * ScreenScale;
                    }

                }
            }
        }

        /// <summary>
        /// 星の等級によって表示のサイズを決定する
        /// </summary>
        /// <param name="mag"></param>
        /// <returns></returns>
        private float mag2size(float mag)
        {
            if (mag > 4) mag = 4.5f + (mag - 4)*0.5f;
            float size = 5f - mag * 0.7f + (Magnitude>90? 90: Magnitude) * 0.04f;
            if (size < 0.8f) size = 0;
            return size;
        }

        private void drawStars(Graphics g,  st_Star[] stars, Quaternion[,] starsQ)//)
        {
            Font font = new Font(FontFamily.GenericSansSerif, 8f * FontMagnitude);
            SolidBrush brush = new SolidBrush(Color.FromArgb(200, 250, 250, 250));
            //Quaternion
            float Zclip = getZclip();
            if (stars != null && stars.Length > 0)
            {
                for(int i=0;i< stars.Length;i++) 
                {
                    st_Star star = stars[i];
                    Quaternion s = starsQ[i,1];

                    if (s.Z > Zclip)
                    {
                        float x = s.X;
                        float y = s.Y;

                        float size = mag2size((float)stars[i].Magx100 / 100f);
                        if (size > 3)
                        {
                            g.FillEllipse(new SolidBrush(Color.FromArgb(100, star.color)), x - size, y - size, size * 2, size * 2);
                            g.FillEllipse(new SolidBrush(Color.FromArgb(100,200,200,200)), x - 3, y - 3, 6, 6);
                            //   g.DrawEllipse(Pens.White, x - s.Z, y - s.Z, s.Z * 2, s.Z * 2);
                            //恒星名
                            if (恒星名称.Checked && star.index > 0)
                            {
                                g.DrawString(reader.lstName[star.index], font, brush, x - size, y + size + 5);
                            }
                        }
                        else if (size > 0)
                        {
                            g.FillEllipse(new SolidBrush(star.color), x - size, y - size, size * 2, size * 2);
                            //   g.DrawEllipse(Pens.White, x - s.Z, y - s.Z, s.Z * 2, s.Z * 2);
                            //恒星名
                            if (恒星名称.Checked && star.index > 0)
                            {
                                g.DrawString(reader.lstName[star.index], font, brush, x - size, y + size + 5);
                            }
                        }
                        if (CntrlKey)
                        {
                            //マウスの位置と星の距離を測定し近いものを選択
                            float ox = (float)PointMouseDown.X - (float)this.Width / 2 - x;
                            float oy = (float)PointMouseDown.Y - (float)this.Height / 2 - y;

                            //double range = Math.Abs(RAMouse - stars[i].Pos.Ra) * 15.0 + Math.Abs(DECMouse - stars[i].Pos.Dec);
                            double range = Math.Abs(ox) + Math.Abs(oy);
                            if (MinRange > range)
                            {
                                MinRange = range;
                                StarMouse = i;
                            }
                        }
                    }
                }
            }
            font.Dispose();
            brush.Dispose();
        }

        /// <summary>
        /// 星座の描画
        /// </summary>
        /// <param name="g"></param>
        private void drawStarLines(Graphics g)
        {
            Pen pen = new Pen(Color.FromArgb(100, 125, 200, 250),2);
            Font font = new Font(FontFamily.GenericSansSerif, 10f * FontMagnitude);
            SolidBrush brush = new SolidBrush(Color.FromArgb(100, Color.SkyBlue));
            //Quaternion
            int cons = 0;
            float zx = 0;
            float zy = 0;
            int zc = 0;
            float Zclip = getZclip(new Point(-Width / 3, -Height / 3));

            if (reader.StarLines != null && reader.StarLines.Length > 0)
            {
                foreach (st_StarLines sls in reader.StarLines)
                {
                    Quaternion s = Hipstars[sls.Hipindex0, 1]; // reader.Hips[sls.Hipindex0].V;
                    Quaternion e = Hipstars[sls.Hipindex1, 1]; //reader.Hips[sls.Hipindex1].V;

                    if (s.Z > Zclip && e.Z > Zclip)
                    {
                        float sx = s.X ;
                        float sy = s.Y ;
                        float ex = e.X ;
                        float ey = e.Y ;

                        float tx = sx - ex;
                        float ty = sy - ey;

                        float L = 10f / (float)Math.Sqrt(tx * tx + ty * ty);
                        tx =  tx * L;
                        ty = ty * L;

                        g.DrawLine(pen, sx  - tx, sy  - ty, ex  + tx, ey  + ty);

                        if (cons != sls.const_num)
                        {
                            if (cons > 0)
                            {
                                zx = zx / zc;
                                zy = zy / zc;

                                string con_str = cel.constlation_name[cons - 1].ToString();
                                string[] con_strs = con_str.Split(',');
                                if (和名.Checked )
                                    g.DrawString(con_strs[2].ToString() +"座", font, brush, zx  - 50, zy );//, PointMouseDown.X + 20 - Width / 2, PointMouseDown.Y - 30 - Height / 2);
                                else
                                    g.DrawString(con_strs[1].ToString(), font, brush, zx  - 50, zy );//, PointMouseDown.X + 20 - Width / 2, PointMouseDown.Y - 30 - Height / 2);
                            }
                            cons = sls.const_num;
                            zx = sx + ex;
                            zy = sy + ey;
                            zc = 2;
                        }
                        else
                        {
                            zx += sx + ex;
                            zy += sy + ey;
                            zc+=2;
                        }
                    }
                }
            }
            pen.Dispose();
            font.Dispose();
            brush.Dispose();
        }


        private void rotateNgcs(Quaternion Q, ref st_NGC[] ngcs)
        {
            Quaternion CQ = Quaternion.Conjugate(Q);
            if (ngcs != null)
            {
                //NGC2000s;
                //   Quaternion[] nebulae = new Quaternion[ngcs.Length];
                if (ステレオ投影.Checked)
                {
                    for (int i = 0; i < ngcs.Length; i++)
                    {
                        ngcs[i].V = Q * ngcs[i].Pos.Quat * CQ;
                        float dz = 2f / (1f + ngcs[i].V.Z) * ScreenScale; //*= dz;
                        ngcs[i].V.X *= dz;
                        ngcs[i].V.Y *= dz;

                        if (ngcs[i].V.X == 0 && ngcs[i].V.Y == 0)
                        {
                            float tt = ngcs[i].V.X;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < ngcs.Length; i++)
                    {
                        ngcs[i].V = Q * ngcs[i].Pos.Quat * CQ * ScreenScale;
                    }
                }
            }
        }

        private int drawNgcs(Graphics g,ref st_NGC[] ngcs,string Catname)
        {
            int resindex = -1;
            Font font = new Font(FontFamily.GenericSansSerif, 12f * FontMagnitude);

            Pen pen = new Pen(Color.FromArgb(200, 250, 100, 0));
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;      // 左右方向は中心寄せ
            format.LineAlignment = StringAlignment.Center;  // 上下方向は中心寄せ            //描画処理
            float Zclip = getZclip();

            //Quaternion
            if (ngcs != null && ngcs.Length > 0)
            {
                for (int i=0;i< ngcs.Length;i++)
                {
                    Quaternion s = ngcs[i].V;
                    if (s.Z > Zclip)
                    {
                        float x = s.X;
                        float y = s.Y;
                        if (x == 0 && y == 0)
                        {
                            float tt=ngcs[i].V.X;
                        }

                        float size = (float)ngcs[i].MajorAngularSize * Magnitude * 0.0167f;
                      
                        g.DrawEllipse(pen, x  - size, y  - size, size * 2, size * 2);
                        if (NGC名称.Checked)
                        {
                            if (NGCNo.Checked && Catname != "")
                            {
                                g.DrawString(Catname + ngcs[i].NUM.ToString(), font, Brushes.Gray, x, y + size + 5, format);
                            }
                            else if (ngcs[i].index > 0)
                            {
                                g.DrawString(reader.lstName[ngcs[i].index], font, Brushes.Gray, x, y + size + 5, format);
                            }
                        }
                        if (CntrlKey)
                        {
                            float ox = (float)PointMouseDown.X - (float)this.Width / 2 - x;
                            float oy = (float)PointMouseDown.Y - (float)this.Height / 2 - y;
                            //      double range = Math.Abs(RAMouse - ngcs[i].Pos.Ra) * 15.0 + Math.Abs(DECMouse - ngcs[i].Pos.Dec);
                            double range = Math.Abs(ox) + Math.Abs(oy);
                            if (MinRange > range)
                            {
                                MinRange = range;
                                resindex = i;
                            }
                        }
                    }
                }
            }
            font.Dispose();
            pen.Dispose();
            format.Dispose();
            return resindex;

        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            float Zclip = getZclip();

            Graphics g = e.Graphics;
            g.TranslateTransform(Width / 2, Height / 2);
            //  g.ScaleTransform((float)Magnitude, (float)Magnitude);

            string info = "";

            //  g.FillRectangle(Brushes.DarkSlateBlue, -this.Width / 2, -this.Height / 2, this.Width, this.Height);
            Font font = new Font(FontFamily.GenericSansSerif, 8f * FontMagnitude);
            SolidBrush Brush = new SolidBrush(Color.FromArgb(200, 250, 250, 250));
            Pen pen = new Pen(Color.FromArgb(200, 250, 250, 100));

            //天の川描画
            if (天の川.Checked && MilkeyWay != null)
            {
                MilkeyWay.draw(g, Magnitude);
                info += @" Milky Way:Axel Mellinger http://astro.starfree.jp/commons/milkyway/  ";
            }
            //　天球経度緯度線描画
            if (座標線.Checked)
            {
                drawCeleLines(g);
                //         if (ucSwitch10.Checked) drawCeleUnit(g);
            }

            //恒星の描画
            if (恒星_BSC5.Checked)
            {
                drawStars(g, reader.Bsc5, Bscstars);
                info += @"  Bsc5";
            }
            MinRange = 100;
            if (CntrlKey)
            {
                StarMouse = -1;
                NBRMouse = -1;
                ngcdraw = "";
            }

            if (恒星_HIP.Checked)
            {
                info += @" AstroCommons http://astro.starfree.jp/commons/hip/";
                drawStars(g, reader.Hips, Hipstars);
            }

            //星座の描画
            if (星座.Checked) drawStarLines(g);

            //マウスポインタへの描画
            if (StarMouse >= 0)
            {
                st_Star star = reader.Hips[StarMouse];
                Quaternion s = Hipstars[StarMouse, 1];
                if (s.Z > Zclip)
                {
                    float difx = s.X - InfoStar.X;
                    float dify = s.Y - InfoStar.Y;

                    float diflen = (float)Math.Sqrt(difx * difx + dify * dify);

                    InfoStar.X += difx * 0.1f;// s.X
                    InfoStar.Y += dify * 0.1f;// s.X

                    float cx = (InfoStar.X - InfoNGC.X);
                    float cy = (InfoStar.Y - InfoNGC.Y);

                    //      GetClientContainState(rct);
                    diflen = (float)Math.Sqrt(cx * cx + cy * cy);
                    if (diflen < 30)
                    {
                        //         PointStarMouse.Y += cy / diflen;// s.X
                        //         PointNGCMouse.Y -= cy / diflen;// s.X
                    }

                    float sz = 5f + mag2size((float)(star.Magx100) / 100f);

                    //g.DrawEllipse(pen, x - z, y - z, z * 2, z * 2);
                    //draw open cross aim
                    g.DrawLine(pen, s.X - sz, s.Y, s.X - sz - 5, s.Y);
                    g.DrawLine(pen, s.X + sz, s.Y, s.X + sz + 5, s.Y);
                    g.DrawLine(pen, s.X, s.Y - sz, s.X, s.Y - sz - 5);
                    g.DrawLine(pen, s.X, s.Y + sz, s.X, s.Y + sz + 5);

                    //引き出し線
                    float x2 = InfoStar.X - 100 - s.X;
                    float y2 = InfoStar.Y - 100 - s.Y;
                    float l2 = sz / (float)Math.Sqrt(x2 * x2 + y2 * y2);
                    x2 *= l2;
                    y2 *= l2;
                    g.DrawLine(pen, s.X + x2, s.Y + y2, InfoStar.X - 100, InfoStar.Y - 100);

                    InfoStar.Size
                    = g.MeasureString("HIP" + star.NUM.ToString(), font);
                    g.DrawString("HIP" + star.NUM.ToString(), font, Brushes.Yellow, InfoStar.X - InfoStar.Width - 100, InfoStar.Y - InfoStar.Height / 2 - 100);
                    //, PointMouseDown.X + 20 - Width / 2, PointMouseDown.Y - 30 - Height / 2);
                }
            }

            //星雲　星団の描画
            MinRange = 100;

            int NGCMouse = -1;
            int ICMouse = -1;
            int NGC2000Mouse = -1;
            int IC2000Mouse = -1;

            if (NGC表示.Checked)
            {
                if (NGCtype.Checked)
                {// AstroCommons  http://astro.starfree.jp/commons/mix/ngc.html
                    info += @"  AstroCommons http://astro.starfree.jp/commons/mix/ngc.html";
                    NGCMouse = drawNgcs(g, ref reader.NGCs, "NGC ");
                    ICMouse = drawNgcs(g, ref reader.ICs, "IC ");
                    if (CntrlKey)
                    {
                        if (NGCMouse >= 0)
                        {
                            ngcs = reader.NGCs;
                            NBRMouse = NGCMouse;
                            ngcdraw = "IC_";
                        }
                        if (ICMouse >= 0)
                        {
                            ngcs = reader.ICs;
                            NBRMouse = ICMouse;
                            ngcdraw = "NGC_";
                        }
                    }
                }
                else
                {// VII/118  NGC 2000.0  (Sky Publishing, ed. Sinnott 1988)
                    info += @"  VII/118 NGC 2000.0(Sky Publishing, ed.Sinnott 1988)";
                    NGC2000Mouse = drawNgcs(g, ref reader.NGC2000s, "NGC_");
                    IC2000Mouse = drawNgcs(g, ref reader.IC2000s, "IC_");
                    if (CntrlKey)
                    {
                        if (NGC2000Mouse >= 0)
                        {
                            ngcs = reader.NGC2000s;
                            NBRMouse = NGC2000Mouse;
                            ngcdraw = "IC";
                        }
                        if (IC2000Mouse >= 0)
                        {
                            ngcs = reader.IC2000s;
                            NBRMouse = IC2000Mouse;
                            ngcdraw = "NGC";
                        }
                    }
                }
            }

            //マウスの選択
            if (NBRMouse >= 0)
            {
                string ngcstr = ngcdraw;
                ngc = ngcs[NBRMouse];
                Quaternion s = ngc.V;
                float difx = (s.X - InfoNGC.X);
                float dify = (s.Y - InfoNGC.Y);
                float diflen = (float)Math.Sqrt(difx * difx + dify * dify);

                if (s.Z > Zclip)
                {
                    InfoNGC.X += difx * 0.1f;// s.X
                    InfoNGC.Y += dify * 0.1f;// s.X
                    float sz = (float)ngc.MajorAngularSize * Magnitude * 0.0167f + 5f;
                    g.DrawEllipse(pen, s.X - sz, s.Y - sz, sz * 2, sz * 2);

                    ngcstr += ngc.NUM.ToString();

                    if (ngc.index > 0)
                    {
                        ngcstr += " " + reader.lstName[ngc.index];
                    }

                    ngcstr += "\r\n" + ngc.NGC_Types.ToString()
                       + " " + ngc.constellation.ToString();
                    ngcstr += "\r\n" + ngc.MajorAngularSize.ToString();

                    if (ngc.MinorAngularSize > 0)
                        ngcstr += "x" + ngc.MinorAngularSize.ToString();

                    //引き出し線
                    float x2 = InfoNGC.X + 100 - s.X;
                    float y2 = InfoNGC.Y - 100 - s.Y;
                    float l2 = sz / (float)Math.Sqrt(x2 * x2 + y2 * y2);
                    x2 *= l2;
                    y2 *= l2;
                    g.DrawLine(pen, s.X + x2, s.Y + y2, InfoNGC.X + 100, InfoNGC.Y - 100);

                    InfoNGC.Size
                    = g.MeasureString(ngcstr, font);
                    g.DrawString(ngcstr, font, Brushes.Yellow, InfoNGC.X + 100, InfoNGC.Y - InfoNGC.Height / 2 - 100);// PointMouseDown.X + 20 - Width / 2, PointMouseDown.Y - 50 - Height / 2);
                }
            }
            else
            {
                InfoNGC.X = PointMouseDown.X - Width / 2;
                InfoNGC.Y = PointMouseDown.Y - Height / 2;
            }

            //等級スケール表示　左上
            for (int i = 0; i < 11; i++)
            {
                float size = mag2size((float)i);
                if (size > 0)
                {
                    g.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.Yellow, i * 25 + 20 - Width / 2, 5 - Height / 2);
                    g.FillEllipse(Brush, i * 25 + 25 - size - Width / 2, 20 - Height / 2, size * 2, size * 2);
                }
            }

            if (地平線.Checked)
            {
                drawHrizon(g);
            }
            drawEcliptic(g);

            //下段　情報
            string Viewstate = "x" + Magnitude.ToString("0.0")
                + "   方位" + AZMITH.ToString("  0.000") + "°" + "高度" + ALTITUDE.ToString("  0.000") + "°   "
                + (RA).ToString("0.0000") + "h " + ((DEC >= 0) ? "+" : "") + (DEC).ToString("0.0000") + "°";

            if(Laps.Checked)
                Viewstate +=" hip"+ elapsHips.ToString() + " ngc" + elapsNGCs.ToString() + " mw" + elapsMilkeyWay.ToString();

            string Viewstatem = (RAMouse).ToString("0.0000") + "  " + (DECMouse).ToString("0.0000");
            g.DrawString(Viewstatem, font, Brushes.Yellow, PointMouseDown.X - Width / 2 - 30, PointMouseDown.Y - Height / 2 + 100);

            g.DrawString(Viewstate, font, Brushes.Yellow, -this.Width / 2, this.Height / 2 - 20);//elaps +
            g.DrawString(info, font, Brushes.Yellow, 0, Height / 2 - 15);

            font.Dispose();
            Brush.Dispose();
            pen.Dispose();

            TargetTime = DateTime.UtcNow.AddDays(dateOffset).AddHours(hourOffset);


            if (時間表示.Checked)
            {
                string strtime = "utc" + TargetTime.ToString("yy/MM/dd\r\nHH:mm:ss");
                Brush = new SolidBrush(Color.FromArgb(100, Color.Goldenrod));
                Font fontT = new Font(FontFamily.GenericSansSerif, 18f * FontMagnitude);

                if (LST表示.Checked)
                {
                    double ti = cel.LST(TargetTime) ;

                    int h = (int)ti;
                    ti = (ti - h) * 60.0;
                    int m = (int)ti;
                    ti = (ti - m) * 60.0;
                    int ss = (int)ti;
                    //    g.DrawString(, fontT, Brush, -this.Width / 2, this.Height / 3 - 20);
                    strtime += "\r\nLST" + h.ToString("00") + ":" + m.ToString("00") + ":" + ss.ToString("00");
                }
                g.DrawString(strtime, fontT, Brush, -this.Width / 2, this.Height / 3);


                fontT.Dispose();
                Brush.Dispose();
            }
            if (!mouseDown)
            {
                Quaternion vp = HorizontalToEquatorial(-AzmithFor, AltitudeFor);
                float sdz = ScreenScale;
                if (ステレオ投影.Checked)
                {
                    sdz *= 2f / (1f + vp.Z);
                }
                vp.X *= sdz;
                vp.Y *= sdz;
                if (vp.Z > Zclip)
                {
                    g.DrawLine(new Pen(Color.FromArgb(100, Color.Yellow), 3), vp.X - 50, vp.Y, vp.X + 50, vp.Y);
                    g.DrawLine(new Pen(Color.FromArgb(100, Color.Yellow), 3), vp.X, vp.Y - 50, vp.X, vp.Y + 50);
                }
            }
        }


        DateTime lastupdate = new DateTime();

        double refreshlap = 0;

        public bool needRefresh=true;
        public void tenqu_update()
        {
            needRefresh = true;
        }
        public void tenqu_refresh()
        {
            needRefresh = false;

            double pasttime = TargetTime.Subtract(lastupdate).TotalSeconds;
            if (Math.Abs(pasttime) > 90)
            {
                //惑星アップデート
                eclipse.set2Time(TargetTime);
                lastupdate = TargetTime;
            }


            tenqu = Quaternion.CreateFromYawPitchRoll(0f, (float)(viewAltitude / 180 * Math.PI), 0f)//
               * Quaternion.CreateFromYawPitchRoll((float)(viewAzmith / 180 * Math.PI), (float)(cel.Latitude / 180 * Math.PI), 0f)//(float)(cel.Latitude / 180 * Math.PI)
               * Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), (float)((cel.LST(TargetTime) - 6 ) / 12.0 * Math.PI));
            //地平線
            horiq = Quaternion.CreateFromYawPitchRoll(0f, (float)(viewAltitude / 180 * Math.PI), 0f)//
               * Quaternion.CreateFromYawPitchRoll((float)(viewAzmith / 180 * Math.PI), (float)(Math.PI / 2), 0f);


            //表示中心の天球座標
            Quaternion Vpos = new Quaternion(0, 0, 1, 0);
            Vpos = Quaternion.Conjugate(tenqu) * Vpos * tenqu;
            RA = Cel.Map24(-Math.Atan2(Vpos.X, Vpos.Y) * 12 / Math.PI - 6);
            double l = Math.Sqrt(Vpos.Z  * Vpos.Z);
            DEC = Math.Atan2(Vpos.Z,1.0- l) * 180 / Math.PI;

            if (座標線.Checked)rotateCeleLines(tenqu);
            swRefresh.Reset();
            swRefresh.Start();
            sw.Reset();
            sw.Start();
            if (NGCtype.Checked)
            {
                rotateNgcs(tenqu, ref reader.NGCs);
                rotateNgcs(tenqu, ref reader.ICs);
            }
            else
            {
                rotateNgcs(tenqu, ref reader.NGC2000s);
                rotateNgcs(tenqu, ref reader.IC2000s);
            }

            elapsNGCs = sw.ElapsedMilliseconds;

        //    if (ucSwitch2.Checked)
                rotateStars(tenqu, Bscstars, ScreenScale);// reader.Bsc5);

            sw.Restart();
         //   if (ucSwitch3.Checked)
            {
                rotateStars(tenqu, Hipstars, ScreenScale);// reader.Hips);
            }
            elapsHips  = sw.ElapsedMilliseconds;
            sw.Restart();

            if (天の川.Checked && MilkeyWay != null)
            {
                if (ステレオ投影.Checked)
                    MilkeyWay.rotate_stereo(tenqu, Magnitude);
                else
                    MilkeyWay.rotate(tenqu, Magnitude);

            }
            elapsMilkeyWay = sw.ElapsedMilliseconds;
            sw.Stop();
            
            this.Invalidate();
            swRefresh.Stop();
            refreshlap = swRefresh.Elapsed.TotalMilliseconds;
        }

        #region Mouse Events
        Point PointMouseDown = new Point(0, 0);
        private void this_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    if (FontMagnitude < 5) FontMagnitude *= 1.1f;
                }
                else
                {
                    if (FontMagnitude > 1) FontMagnitude /= 1.1f;
                }
            }
            else
            {
                if (e.Delta > 0)
                {
                    if (Magnitude < 200) Magnitude *= 1.1f;
                }
                else
                {
                    if (Magnitude > 1) Magnitude /= 1.1f;
                }
            }
            ScreenScale = 180f * Magnitude;
            tenqu_update();

        }

        bool trace = false;
        bool mouseDown = false;
        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point MM = e.Location;
            double xx = (MM.X - this.Width / 2) / (ScreenScale);
            double yy = (MM.Y - this.Height / 2) / (ScreenScale);
            double len = Math.Sqrt(1.0 - xx * xx - yy * yy);

            Quaternion Vpos = new Quaternion((float)xx, (float)yy, (float)len, 0);
            if (ステレオ投影.Checked)
            {
                xx *= 0.5f;
                yy *= 0.5f;
                len = (1.0 + xx * xx + yy * yy);
                double zz = (-1.0 + xx * xx + yy * yy);
                xx = 2 * xx / len;
                yy = 2 * yy / len;
                zz = zz / len;
                Vpos = new Quaternion((float)xx, (float)yy, -(float)zz, 0);
            }
            Quaternion Choriq = Quaternion.Conjugate(horiq);
            Mpos = (Choriq * Vpos * horiq);
            AltitudeFor = (Math.Atan2(Mpos.Z, Math.Sqrt(1.0 - Mpos.Z * Mpos.Z)) * 180.0 / Math.PI);
            AzmithFor = Cel.Map360(-Math.Atan2(Mpos.X, Mpos.Y) * 180.0 / Math.PI);

     //       AltitudeFor = ALTITUDE;
     //       AzmithFor = AZMITH;
            trace = true;
            this.Refresh();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            PointMouseDown = e.Location;
            mouseDown = true;
        }
        private void Tenqu_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Point MM = e.Location; 
            if (e.Button == MouseButtons.Left)
            {
                int dx = MM.X - PointMouseDown.X;
                int dy = MM.Y - PointMouseDown.Y;
                if (true)
                {
                    AzmithFor += (float)dx / (Magnitude * 3.0);
                    if (AzmithFor > 360.0) AzmithFor -= 360;
                    if (AzmithFor < 0f) AzmithFor += 360.0;
                    AltitudeFor += (float)dy / (Magnitude * 3.0);
                    if (AltitudeFor > 90.0) AltitudeFor = 90.0;
                    if (AltitudeFor < -90.0) AltitudeFor = -90.0;
                    trace = true;
             //       AltitudeFor = -viewAltitude;
             //       AzmithFor = viewAzmith;
                }
                else
                {//自由に回転
                    int ox = PointMouseDown.X - this.Width / 2;
                    int oy = PointMouseDown.Y - this.Height / 2;
                    float ln = (float)Math.Sqrt(ox * ox + oy * oy) / this.Width * 2;
                    ln = ln * ln;
                    double pr = Math.Atan2(oy, ox);
                    double r = Math.Atan2(MM.Y - this.Height / 2, MM.X - this.Width / 2);
                    r = r - pr;
                    tenqu = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)r * ln) * tenqu;
                    tenqu = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)dx / 200.0f / Magnitude * (1f - ln)) * tenqu;
                    tenqu = Quaternion.CreateFromAxisAngle(new Vector3(-1, 0, 0), (float)dy / 200.0f / Magnitude * (1f - ln)) * tenqu;
                }
                tenqu_update();
            }
            Quaternion Vpos;
            double xx =( MM.X - this.Width / 2) / (ScreenScale);
            double yy =( MM.Y - this.Height / 2) / (ScreenScale);
            double len = Math.Sqrt(1.0 - xx * xx - yy * yy);
            Vpos = new Quaternion((float)xx, (float)yy, (float)len, 0);
            if (ステレオ投影.Checked)
            {
                xx *= 0.5f;
                yy *= 0.5f;
                len = (1.0 + xx * xx + yy * yy);
                double zz = (-1.0 + xx * xx + yy * yy);
                xx = 2 * xx / len;
                yy = 2 * yy / len;
                zz = zz / len;
                Vpos = new Quaternion((float)xx, (float)yy, -(float)zz, 0);
            }
            Quaternion Choriq = Quaternion.Conjugate(horiq);
            Mpos = (Choriq * Vpos * horiq);
            ALTITUDE = (Math.Atan2(Mpos.Z, Math.Sqrt(1.0 - Mpos.Z * Mpos.Z)) * 180.0 / Math.PI);
            AZMITH = Cel.Map360(-Math.Atan2(Mpos.X, Mpos.Y) * 180.0 / Math.PI );

            Quaternion VposMouse = Quaternion.Conjugate(tenqu) * Vpos * tenqu;
            RAMouse = Cel.Map24(-Math.Atan2(VposMouse.X, VposMouse.Y) * 12.0 / Math.PI - 6.0);
            double l = Math.Sqrt(1-VposMouse.Z * VposMouse.Z);
            DECMouse = Math.Atan2(VposMouse.Z,  l) * 180.0 / Math.PI;

            this.Refresh();
            PointMouseDown = MM;

        }

        private Quaternion HorizontalToEquatorial(double azm,double alt)
        {
            float l = (float)Math.Cos((alt) * Math.PI / 180);
            Quaternion hPos = new Quaternion((float)Math.Sin(azm * Math.PI / 180) * l, (float)Math.Cos(azm * Math.PI / 180) * l, (float)Math.Sin((alt) * Math.PI / 180),0) ;
            Quaternion Conj = Quaternion.Conjugate(horiq);
            Quaternion vp = horiq * hPos * Conj;

            return vp;
        }


        #endregion

        #region Graphic
        /// <summary>
        /// 文字列の描画、回転、基準位置指定
        /// </summary>
        /// <param name="g">描画先のGraphicsオブジェクト</param>
        /// <param name="s">描画する文字列</param>
        /// <param name="f">文字のフォント</param>
        /// <param name="brush">描画用ブラシ</param>
        /// <param name="x">基準位置のX座標</param>
        /// <param name="y">基準位置のY座標</param>
        /// <param name="deg">回転角度（度数、時計周りが正）</param>
        /// <param name="format">基準位置をStringFormatクラスオブジェクトで指定します</param>
        public void DrawString(Graphics g, string s, Font f, Brush brush, float x, float y, float deg, StringFormat format)
        {
            using (var pathText = new System.Drawing.Drawing2D.GraphicsPath())  // パスの作成
            using (var mat = new System.Drawing.Drawing2D.Matrix())             // アフィン変換行列
            {
                // 描画用Format
                var formatTemp = (StringFormat)format.Clone();
                formatTemp.Alignment = StringAlignment.Near;        // 左寄せに修正
                formatTemp.LineAlignment = StringAlignment.Near;    // 上寄せに修正

                // 文字列の描画
                pathText.AddString(
                        s,
                        f.FontFamily,
                        (int)f.Style,
                        f.SizeInPoints,
                        new PointF(0, 0),
                        format);
                formatTemp.Dispose();

                // 文字の領域取得
                var rect = pathText.GetBounds();

                // 回転中心のX座標
                float px;
                switch (format.Alignment)
                {
                    case StringAlignment.Near:
                        px = rect.Left;
                        break;
                    case StringAlignment.Center:
                        px = rect.Left + rect.Width / 2f;
                        break;
                    case StringAlignment.Far:
                        px = rect.Right;
                        break;
                    default:
                        px = 0;
                        break;
                }
                // 回転中心のY座標
                float py;
                switch (format.LineAlignment)
                {
                    case StringAlignment.Near:
                        py = rect.Top;
                        break;
                    case StringAlignment.Center:
                        py = rect.Top + rect.Height / 2f;
                        break;
                    case StringAlignment.Far:
                        py = rect.Bottom;
                        break;
                    default:
                        py = 0;
                        break;
                }

                // 文字の回転中心座標を原点へ移動
                mat.Translate(-px, -py, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 文字の回転
                mat.Rotate(deg, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 表示位置まで移動
                mat.Translate(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);

                // パスをアフィン変換
                pathText.Transform(mat);

                // 描画
                g.FillPath(brush, pathText);
            }
        }
        #endregion


        private void Tenqu_Resize(object sender, EventArgs e)
        {
        //    trace = false;
            Center = new Vector2(Width / 2, Height / 2);
            Refresh();
        }


        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            //チェック状態を反転させる
            item.Checked = !item.Checked;

            bool b = item.Checked;
            座標線.Visible = b;
            恒星_BSC5.Visible = b;
            恒星_HIP.Visible = b;
            NGCtype.Visible = b;
            NGCNo.Visible = b;
            恒星名称.Visible = b;
            天の川.Visible = b;
            星座.Visible = b;
            地平線.Visible = b;
            座標.Visible = b;
            Laps.Visible = b;
            和名.Visible = b;
            ステレオ投影.Visible = b;
            LST表示.Visible = b;
            時間表示.Visible = b;
            黄道.Visible = b;
            太陽系.Visible = b;
            NGC表示.Visible = b;
            NGC名称.Visible = b;
            label1.Visible = b;
            label2.Visible = b;
            label3.Visible = b;
            label4.Visible = b;
            label5.Visible = b;
            label6.Visible = b;
            label7.Visible = b;
            label8.Visible = b;
            label9.Visible = b;
            label10.Visible = b;
            label11.Visible = b;
            label12.Visible = b;
            label13.Visible = b;
            label14.Visible = b;
            label15.Visible = b;
            label16.Visible = b;
            label17.Visible = b;
            label18.Visible = b;
            label19.Visible = b;

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                textBox1.Visible = false;
                return;
            }
            if (e.KeyChar < '0' || '9' < e.KeyChar || '.' == e.KeyChar || ' ' == e.KeyChar || '\b' == e.KeyChar)
            {
                //押されたキーが 0～9でない場合は、イベントをキャンセルする
                e.Handled = true;
            }
            double Latitude = cel.Latitude;
            double Longitude = cel.Longitude;
            if (Cel.RaDecStringToEquatorial(textBox1.Text, ref Latitude, ref Longitude))
            {
                cel.Latitude = Latitude;
                cel.Longitude = Longitude;
                textBox1.BackColor = Color.White;
                return;
            }
            textBox1.BackColor = Color.Yellow;

        }


        private void 観測位置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.Visible)
            {
                textBox1.Visible = false;
            }
            else
            {
                textBox1.Text = Cel.Deg2String(cel.Latitude) + " " + Cel.Deg2String(cel.Longitude);
                textBox1.BackColor = Color.White;
                textBox1.Visible = true;
            }
        }


        private void ucSwitch1_CheckedChanged(object sender, BoolEventArgs e)
        {
            tenqu_update();
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////
        




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



                Tenqu_savesetting();








                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region uc

        private ucSwitch 恒星_HIP;
        private ucSwitch 座標線;
        private ucSwitch 恒星_BSC5;
        private ucSwitch NGCtype;
        private ucSwitch NGCNo;
        private ucSwitch 恒星名称;
        private ucSwitch 天の川;
        private ucSwitch 星座;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private ucSwitch 地平線;
        private Label label10;
        private ucSwitch 座標;
        private Label label11;
        private ucSwitch Laps;
        private Label label12;
        private ucSwitch 和名;
        private Label label13;
        private ucSwitch ステレオ投影;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label label14;
        private ucSwitch LST表示;
        private Label label15;
        private ucSwitch 時間表示;
        private Label label16;
        private ucSwitch 黄道;
        private Label label17;
        private ucSwitch 太陽系;
        private Label label18;
        private ucSwitch NGC表示;
        private Label label19;
        private ucSwitch NGC名称;
        private Label label20;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem 観測位置ToolStripMenuItem;
        private ToolStripMenuItem 時差UTCToolStripMenuItem;
        private ToolStripMenuItem 設定ToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;

        #endregion

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.観測位置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.時差UTCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.NGCtype = new ucPgmac.ucSwitch();
            this.NGC名称 = new ucPgmac.ucSwitch();
            this.NGC表示 = new ucPgmac.ucSwitch();
            this.太陽系 = new ucPgmac.ucSwitch();
            this.黄道 = new ucPgmac.ucSwitch();
            this.時間表示 = new ucPgmac.ucSwitch();
            this.LST表示 = new ucPgmac.ucSwitch();
            this.ステレオ投影 = new ucPgmac.ucSwitch();
            this.和名 = new ucPgmac.ucSwitch();
            this.Laps = new ucPgmac.ucSwitch();
            this.座標 = new ucPgmac.ucSwitch();
            this.地平線 = new ucPgmac.ucSwitch();
            this.星座 = new ucPgmac.ucSwitch();
            this.天の川 = new ucPgmac.ucSwitch();
            this.恒星名称 = new ucPgmac.ucSwitch();
            this.NGCNo = new ucPgmac.ucSwitch();
            this.恒星_BSC5 = new ucPgmac.ucSwitch();
            this.座標線 = new ucPgmac.ucSwitch();
            this.恒星_HIP = new ucPgmac.ucSwitch();
            this.TimeZone = new System.Windows.Forms.NumericUpDown();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeZone)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.Color.Goldenrod;
            this.label1.Location = new System.Drawing.Point(586, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 14);
            this.label1.TabIndex = 15;
            this.label1.Text = "座標線";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.ForeColor = System.Drawing.Color.Goldenrod;
            this.label2.Location = new System.Drawing.Point(554, 222);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 14);
            this.label2.TabIndex = 16;
            this.label2.Text = "恒星 BSC5";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label2.Visible = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.ForeColor = System.Drawing.Color.Goldenrod;
            this.label3.Location = new System.Drawing.Point(568, 252);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 14);
            this.label3.TabIndex = 17;
            this.label3.Text = "恒星 HIP";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label3.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label4.ForeColor = System.Drawing.Color.Goldenrod;
            this.label4.Location = new System.Drawing.Point(597, 462);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 14);
            this.label4.TabIndex = 18;
            this.label4.Text = "NGC";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label4.Visible = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label5.ForeColor = System.Drawing.Color.Goldenrod;
            this.label5.Location = new System.Drawing.Point(577, 492);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 14);
            this.label5.TabIndex = 19;
            this.label5.Text = "NGCNo.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label5.Visible = false;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label6.ForeColor = System.Drawing.Color.Goldenrod;
            this.label6.Location = new System.Drawing.Point(564, 282);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 14);
            this.label6.TabIndex = 20;
            this.label6.Text = "恒星名称";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label6.Visible = false;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label7.ForeColor = System.Drawing.Color.Goldenrod;
            this.label7.Location = new System.Drawing.Point(584, 312);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 14);
            this.label7.TabIndex = 21;
            this.label7.Text = "天の川";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label7.Visible = false;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label8.ForeColor = System.Drawing.Color.Goldenrod;
            this.label8.Location = new System.Drawing.Point(597, 342);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 14);
            this.label8.TabIndex = 22;
            this.label8.Text = "星座";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label8.Visible = false;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label9.ForeColor = System.Drawing.Color.Goldenrod;
            this.label9.Location = new System.Drawing.Point(586, 102);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 14);
            this.label9.TabIndex = 24;
            this.label9.Text = "地平線";
            this.label9.Visible = false;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label10.ForeColor = System.Drawing.Color.Goldenrod;
            this.label10.Location = new System.Drawing.Point(597, 72);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 14);
            this.label10.TabIndex = 26;
            this.label10.Text = "座標";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label10.Visible = false;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label11.ForeColor = System.Drawing.Color.Goldenrod;
            this.label11.Location = new System.Drawing.Point(592, 582);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 14);
            this.label11.TabIndex = 28;
            this.label11.Text = "Laps";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label11.Visible = false;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label12.ForeColor = System.Drawing.Color.Goldenrod;
            this.label12.Location = new System.Drawing.Point(597, 372);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 14);
            this.label12.TabIndex = 30;
            this.label12.Text = "和名";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label12.Visible = false;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label13.ForeColor = System.Drawing.Color.Goldenrod;
            this.label13.Location = new System.Drawing.Point(551, 162);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 14);
            this.label13.TabIndex = 32;
            this.label13.Text = "ステレオ投影";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label13.Visible = false;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label14.ForeColor = System.Drawing.Color.Goldenrod;
            this.label14.Location = new System.Drawing.Point(569, 522);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(59, 14);
            this.label14.TabIndex = 34;
            this.label14.Text = "LST表示";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label14.Visible = false;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label15.ForeColor = System.Drawing.Color.Goldenrod;
            this.label15.Location = new System.Drawing.Point(563, 552);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(63, 14);
            this.label15.TabIndex = 36;
            this.label15.Text = "時間表示";
            this.label15.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label15.Visible = false;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label16.ForeColor = System.Drawing.Color.Goldenrod;
            this.label16.Location = new System.Drawing.Point(598, 132);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 14);
            this.label16.TabIndex = 44;
            this.label16.Text = "黄道";
            this.label16.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label16.Visible = false;
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label17.ForeColor = System.Drawing.Color.Goldenrod;
            this.label17.Location = new System.Drawing.Point(581, 192);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(49, 14);
            this.label17.TabIndex = 46;
            this.label17.Text = "太陽系";
            this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label17.Visible = false;
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label18.ForeColor = System.Drawing.Color.Goldenrod;
            this.label18.Location = new System.Drawing.Point(597, 402);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(35, 14);
            this.label18.TabIndex = 48;
            this.label18.Text = "NGC";
            this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label18.Visible = false;
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("MS UI Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label19.ForeColor = System.Drawing.Color.Goldenrod;
            this.label19.Location = new System.Drawing.Point(568, 432);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(63, 14);
            this.label19.TabIndex = 50;
            this.label19.Text = "NGC名称";
            this.label19.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label19.Visible = false;
            // 
            // label20
            // 
            this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Segoe UI Emoji", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Gold;
            this.label20.Location = new System.Drawing.Point(224, 10);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(392, 19);
            this.label20.TabIndex = 51;
            this.label20.Text = "viewstars Ver 0.00 test makoto kuwabara MMXXIV VIII / XXVII";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.観測位置ToolStripMenuItem,
            this.時差UTCToolStripMenuItem,
            this.設定ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(123, 70);
            // 
            // 観測位置ToolStripMenuItem
            // 
            this.観測位置ToolStripMenuItem.Name = "観測位置ToolStripMenuItem";
            this.観測位置ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.観測位置ToolStripMenuItem.Text = "観測位置";
            this.観測位置ToolStripMenuItem.Click += new System.EventHandler(this.観測位置ToolStripMenuItem_Click);
            // 
            // 時差UTCToolStripMenuItem
            // 
            this.時差UTCToolStripMenuItem.Name = "時差UTCToolStripMenuItem";
            this.時差UTCToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.時差UTCToolStripMenuItem.Text = "時差UTC";
            this.時差UTCToolStripMenuItem.Click += new System.EventHandler(this.時差UTCToolStripMenuItem_Click);
            // 
            // 設定ToolStripMenuItem
            // 
            this.設定ToolStripMenuItem.Name = "設定ToolStripMenuItem";
            this.設定ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.設定ToolStripMenuItem.Text = "設定";
            this.設定ToolStripMenuItem.Click += new System.EventHandler(this.設定ToolStripMenuItem_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 37);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(278, 19);
            this.textBox1.TabIndex = 52;
            this.textBox1.Visible = false;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // NGCtype
            // 
            this.NGCtype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NGCtype.Checked = true;
            this.NGCtype.ColorOff = System.Drawing.Color.Black;
            this.NGCtype.ColorOn = System.Drawing.Color.DarkBlue;
            this.NGCtype.Location = new System.Drawing.Point(645, 462);
            this.NGCtype.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.NGCtype.Name = "NGCtype";
            this.NGCtype.Size = new System.Drawing.Size(48, 24);
            this.NGCtype.TabIndex = 53;
            this.NGCtype.Visible = false;
            // 
            // NGC名称
            // 
            this.NGC名称.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NGC名称.Checked = true;
            this.NGC名称.ColorOff = System.Drawing.Color.Black;
            this.NGC名称.ColorOn = System.Drawing.Color.DarkBlue;
            this.NGC名称.Location = new System.Drawing.Point(646, 432);
            this.NGC名称.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.NGC名称.Name = "NGC名称";
            this.NGC名称.Size = new System.Drawing.Size(48, 24);
            this.NGC名称.TabIndex = 49;
            this.NGC名称.Visible = false;
            // 
            // NGC表示
            // 
            this.NGC表示.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NGC表示.Checked = true;
            this.NGC表示.ColorOff = System.Drawing.Color.Black;
            this.NGC表示.ColorOn = System.Drawing.Color.DarkBlue;
            this.NGC表示.Location = new System.Drawing.Point(646, 402);
            this.NGC表示.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.NGC表示.Name = "NGC表示";
            this.NGC表示.Size = new System.Drawing.Size(48, 24);
            this.NGC表示.TabIndex = 47;
            this.NGC表示.Visible = false;
            // 
            // 太陽系
            // 
            this.太陽系.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.太陽系.Checked = true;
            this.太陽系.ColorOff = System.Drawing.Color.Black;
            this.太陽系.ColorOn = System.Drawing.Color.DarkBlue;
            this.太陽系.Location = new System.Drawing.Point(646, 192);
            this.太陽系.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.太陽系.Name = "太陽系";
            this.太陽系.Size = new System.Drawing.Size(48, 24);
            this.太陽系.TabIndex = 45;
            this.太陽系.Visible = false;
            this.太陽系.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 黄道
            // 
            this.黄道.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.黄道.Checked = true;
            this.黄道.ColorOff = System.Drawing.Color.Black;
            this.黄道.ColorOn = System.Drawing.Color.DarkBlue;
            this.黄道.Location = new System.Drawing.Point(646, 132);
            this.黄道.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.黄道.Name = "黄道";
            this.黄道.Size = new System.Drawing.Size(48, 24);
            this.黄道.TabIndex = 43;
            this.黄道.Visible = false;
            this.黄道.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 時間表示
            // 
            this.時間表示.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.時間表示.Checked = true;
            this.時間表示.ColorOff = System.Drawing.Color.Black;
            this.時間表示.ColorOn = System.Drawing.Color.DarkBlue;
            this.時間表示.Location = new System.Drawing.Point(646, 552);
            this.時間表示.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.時間表示.Name = "時間表示";
            this.時間表示.Size = new System.Drawing.Size(48, 24);
            this.時間表示.TabIndex = 35;
            this.時間表示.Visible = false;
            this.時間表示.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // LST表示
            // 
            this.LST表示.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LST表示.Checked = true;
            this.LST表示.ColorOff = System.Drawing.Color.Black;
            this.LST表示.ColorOn = System.Drawing.Color.DarkBlue;
            this.LST表示.Location = new System.Drawing.Point(646, 522);
            this.LST表示.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LST表示.Name = "LST表示";
            this.LST表示.Size = new System.Drawing.Size(48, 24);
            this.LST表示.TabIndex = 33;
            this.LST表示.Visible = false;
            this.LST表示.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // ステレオ投影
            // 
            this.ステレオ投影.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ステレオ投影.Checked = true;
            this.ステレオ投影.ColorOff = System.Drawing.Color.Black;
            this.ステレオ投影.ColorOn = System.Drawing.Color.DarkBlue;
            this.ステレオ投影.Location = new System.Drawing.Point(646, 162);
            this.ステレオ投影.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ステレオ投影.Name = "ステレオ投影";
            this.ステレオ投影.Size = new System.Drawing.Size(48, 24);
            this.ステレオ投影.TabIndex = 31;
            this.ステレオ投影.Visible = false;
            this.ステレオ投影.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 和名
            // 
            this.和名.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.和名.Checked = true;
            this.和名.ColorOff = System.Drawing.Color.Black;
            this.和名.ColorOn = System.Drawing.Color.DarkBlue;
            this.和名.Location = new System.Drawing.Point(646, 372);
            this.和名.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.和名.Name = "和名";
            this.和名.Size = new System.Drawing.Size(48, 24);
            this.和名.TabIndex = 29;
            this.和名.Visible = false;
            this.和名.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // Laps
            // 
            this.Laps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Laps.Checked = false;
            this.Laps.ColorOff = System.Drawing.Color.Black;
            this.Laps.ColorOn = System.Drawing.Color.DarkBlue;
            this.Laps.Location = new System.Drawing.Point(646, 582);
            this.Laps.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Laps.Name = "Laps";
            this.Laps.Size = new System.Drawing.Size(48, 24);
            this.Laps.TabIndex = 27;
            this.Laps.Visible = false;
            this.Laps.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 座標
            // 
            this.座標.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.座標.Checked = true;
            this.座標.ColorOff = System.Drawing.Color.Black;
            this.座標.ColorOn = System.Drawing.Color.DarkBlue;
            this.座標.ForeColor = System.Drawing.SystemColors.ControlText;
            this.座標.Location = new System.Drawing.Point(646, 72);
            this.座標.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.座標.Name = "座標";
            this.座標.Size = new System.Drawing.Size(48, 24);
            this.座標.TabIndex = 25;
            this.座標.Visible = false;
            this.座標.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 地平線
            // 
            this.地平線.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.地平線.Checked = true;
            this.地平線.ColorOff = System.Drawing.Color.Black;
            this.地平線.ColorOn = System.Drawing.Color.DarkBlue;
            this.地平線.Location = new System.Drawing.Point(646, 102);
            this.地平線.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.地平線.Name = "地平線";
            this.地平線.Size = new System.Drawing.Size(48, 24);
            this.地平線.TabIndex = 23;
            this.地平線.Visible = false;
            this.地平線.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 星座
            // 
            this.星座.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.星座.Checked = true;
            this.星座.ColorOff = System.Drawing.Color.Black;
            this.星座.ColorOn = System.Drawing.Color.DarkBlue;
            this.星座.Location = new System.Drawing.Point(646, 342);
            this.星座.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.星座.Name = "星座";
            this.星座.Size = new System.Drawing.Size(48, 24);
            this.星座.TabIndex = 14;
            this.星座.Visible = false;
            this.星座.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 天の川
            // 
            this.天の川.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.天の川.Checked = false;
            this.天の川.ColorOff = System.Drawing.Color.Black;
            this.天の川.ColorOn = System.Drawing.Color.DarkBlue;
            this.天の川.Location = new System.Drawing.Point(646, 312);
            this.天の川.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.天の川.Name = "天の川";
            this.天の川.Size = new System.Drawing.Size(48, 24);
            this.天の川.TabIndex = 13;
            this.天の川.Visible = false;
            this.天の川.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 恒星名称
            // 
            this.恒星名称.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.恒星名称.Checked = true;
            this.恒星名称.ColorOff = System.Drawing.Color.Black;
            this.恒星名称.ColorOn = System.Drawing.Color.DarkBlue;
            this.恒星名称.Location = new System.Drawing.Point(646, 282);
            this.恒星名称.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.恒星名称.Name = "恒星名称";
            this.恒星名称.Size = new System.Drawing.Size(48, 24);
            this.恒星名称.TabIndex = 12;
            this.恒星名称.Visible = false;
            this.恒星名称.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // NGCNo
            // 
            this.NGCNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NGCNo.Checked = false;
            this.NGCNo.ColorOff = System.Drawing.Color.Black;
            this.NGCNo.ColorOn = System.Drawing.Color.DarkBlue;
            this.NGCNo.Location = new System.Drawing.Point(646, 492);
            this.NGCNo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.NGCNo.Name = "NGCNo";
            this.NGCNo.Size = new System.Drawing.Size(48, 24);
            this.NGCNo.TabIndex = 11;
            this.NGCNo.Visible = false;
            this.NGCNo.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 恒星_BSC5
            // 
            this.恒星_BSC5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.恒星_BSC5.Checked = false;
            this.恒星_BSC5.ColorOff = System.Drawing.Color.Black;
            this.恒星_BSC5.ColorOn = System.Drawing.Color.DarkBlue;
            this.恒星_BSC5.Location = new System.Drawing.Point(646, 222);
            this.恒星_BSC5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.恒星_BSC5.Name = "恒星_BSC5";
            this.恒星_BSC5.Size = new System.Drawing.Size(48, 24);
            this.恒星_BSC5.TabIndex = 9;
            this.恒星_BSC5.Visible = false;
            this.恒星_BSC5.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 座標線
            // 
            this.座標線.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.座標線.Checked = true;
            this.座標線.ColorOff = System.Drawing.Color.Black;
            this.座標線.ColorOn = System.Drawing.Color.DarkBlue;
            this.座標線.Location = new System.Drawing.Point(646, 42);
            this.座標線.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.座標線.Name = "座標線";
            this.座標線.Size = new System.Drawing.Size(48, 24);
            this.座標線.TabIndex = 8;
            this.座標線.Visible = false;
            this.座標線.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // 恒星_HIP
            // 
            this.恒星_HIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.恒星_HIP.Checked = true;
            this.恒星_HIP.ColorOff = System.Drawing.Color.Black;
            this.恒星_HIP.ColorOn = System.Drawing.Color.DarkBlue;
            this.恒星_HIP.Location = new System.Drawing.Point(646, 252);
            this.恒星_HIP.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.恒星_HIP.Name = "恒星_HIP";
            this.恒星_HIP.Size = new System.Drawing.Size(48, 24);
            this.恒星_HIP.TabIndex = 7;
            this.恒星_HIP.Visible = false;
            this.恒星_HIP.CheckedChanged += new ucPgmac.BoolEventHandler(this.ucSwitch1_CheckedChanged);
            // 
            // TimeZone
            // 
            this.TimeZone.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.TimeZone.Location = new System.Drawing.Point(13, 62);
            this.TimeZone.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.TimeZone.Minimum = new decimal(new int[] {
            12,
            0,
            0,
            -2147483648});
            this.TimeZone.Name = "TimeZone";
            this.TimeZone.Size = new System.Drawing.Size(46, 19);
            this.TimeZone.TabIndex = 55;
            this.TimeZone.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TimeZone.Value = new decimal(new int[] {
            115,
            0,
            0,
            65536});
            this.TimeZone.Visible = false;
            this.TimeZone.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TimeZone_KeyPress);
            // 
            // Tenqu
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(48)))));
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.TimeZone);
            this.Controls.Add(this.NGCtype);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.NGC名称);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.NGC表示);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.太陽系);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.黄道);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.時間表示);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.LST表示);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.ステレオ投影);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.和名);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.Laps);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.座標);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.地平線);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.星座);
            this.Controls.Add(this.天の川);
            this.Controls.Add(this.恒星名称);
            this.Controls.Add(this.NGCNo);
            this.Controls.Add(this.恒星_BSC5);
            this.Controls.Add(this.座標線);
            this.Controls.Add(this.恒星_HIP);
            this.Name = "Tenqu";
            this.Size = new System.Drawing.Size(709, 646);
            this.Load += new System.EventHandler(this.Tenqu_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Tenqu_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Tenqu_KeyUp);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Tenqu_MouseUp);
            this.Resize += new System.EventHandler(this.Tenqu_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TimeZone)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        bool CntrlKey = false;
        private void Tenqu_KeyDown(object sender, KeyEventArgs e)
        {
            CntrlKey = !CntrlKey;

        }

        private void Tenqu_KeyUp(object sender, KeyEventArgs e)
        {
        //    CntrlKey = false;
        }

        private void 時差UTCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeZone.Visible = true;
        }


        private void TimeZone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                TimeZone.Visible = false;
                return;
            }

        }
    }
}
