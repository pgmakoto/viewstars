using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ucPgmac;


namespace ucPgmac
{
    //Astro Commons
    //https://note.com/toyoshimorioka/n/n20c844eb86e0

    //http://tdc-www.harvard.edu/catalogs/catalogsb.html
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct st_hipheaderinfo
    {

        public Int32 num;  //スター番号から減算してシーケンス番号を取得します
        public Int32 firstnum;  //ファイル内の最初の星の番号
        public Int32 count;  //ファイル内の星の数     負の場合、座標はB1950ではなくJ2000になります。
        public Int32 stnum; //星ID番号が存在しない場合は0
                  // 星のID番号がカタログファイルにある場合は1
                  // 星のID番号が地域nnnn（GSC）の場合は2
                  // 星のID番号が領域nnnnn（ティコ）の場合は3
		          // 4 星のID番号が整数*4 で実数*4 でない場合
                  // <0 ID番号はありませんが、オブジェクト名は-STNUM文字です
                  // エントリー終了時
        public Int32 mprop;// 固有運動が含まれていない場合は0  1 固有運動が含まれる場合 視線速度が含まれる場合は2
        public Int32 nmag;// 存在する等級の数(0-10) 負の場合、座標はB1950ではなくJ2000になります。
        public Int32 NBENT;//スターエントリあたりのバイト数
    };

   
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct st_hipinfo
    {
        public Int32 XNO;      //XNO 星のカタログ番号[オプション]
        public double SRAD;    // SRA0 B1950 赤経（ラジアン）
        public double SDECO;   //SDEC0 B1950 偏角（ラジアン）
        public char ISP0;      //ISPスペクトル型（2文字）
        public char ISP1;      //
        public Int16 MAG100;   //V 大きさ * 100 [0-10 が存在する可能性があります]
        public Single XRPM_RA;  //XRPM RA 固有運動(ラジアン/年) [オプション]
        public Single XDPM_Dec; //XDPM Dec.固有運動(ラジアン/年) [オプション]
        public double SVEL;    //SVEL 視線速度（キロメートル/秒）（オプション）
        /*  
        public readonly char b6;      //
        public readonly char b5;      //ISPスペクトル型（2文字）
        public readonly char b4;      //
        public readonly char b3;      //ISPスペクトル型（2文字）
        public readonly char b2;      //
        public readonly char b1;      //ISPスペクトル型（2文字）
        */
    };

    /*
    スペクトル型 O5  B5        A5       F5      G5      K5      M5
    恒星の色 青      青白      白       黄白    黄      橙      赤
    表面温度 45000K  15000K    8300K	6600K	5600K	4400K	3300K
    */

    public class HipStar
    {
        private FolderBrowserDialog folderBrowserDialog1;
        private string filepath = "";

        public st_hipheaderinfo fil = new st_hipheaderinfo();
        public List<Star> stars
        {
            get; set;
        }// = new List<Star>();

        public static short ToInt16(byte[] value, int startIndex)
        {
            byte[] sub = GetSubArray(value, startIndex, sizeof(Int16));
            return BitConverter.ToInt16(Reverse(sub), 0);
        }

        public static Int32 ToInt32(byte[] value, int startIndex)
        {
            byte[] sub = GetSubArray(value, startIndex, sizeof(Int32));
            return BitConverter.ToInt32(Reverse(sub), 0);

        }
        public static double ToDouble(byte[] value, int startIndex)
        {
            //   / *～エラーチェックとかのコード、省略～* /
            byte[] sub = GetSubArray(value, startIndex, sizeof(double));
            return BitConverter.ToDouble(Reverse(sub), 0);
        }
        private static byte[] GetSubArray(byte[] src, int startIndex, int count)
        {
            byte[] dst = new byte[count];
            Array.Copy(src, startIndex, dst, 0, count);
            return dst;
        }
        private static byte[] Reverse(byte[] bytes)
        {
            return bytes.Reverse().ToArray();
        }
        public static float ToSingle(byte[] value, int startIndex)
        {
            byte[] sub = GetSubArray(value, startIndex, sizeof(float));
            return BitConverter.ToSingle(Reverse(sub), 0);
        }
        /**/
        public void doset()
        {
            folderBrowserDialog1 = new FolderBrowserDialog();

            if (filepath == "") folderBrowserDialog1.SelectedPath = AppContext.BaseDirectory;
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                filepath = folderBrowserDialog1.SelectedPath ;
                Reg.AddUpdateAppSettings("HipPath", filepath);
            }
        }

        public HipStar( List<Star> st)
        {
            stars = st;
            filepath = Reg.ReadSetting("HipPath") ;
            string datafile = filepath + @"\hipparcos";
            if (!File.Exists(datafile))
            {
                 doset();
            }

            if (File.Exists(datafile))
            {
                // filePathのファイルは存在する
                using (var reader = new BinaryReader(new FileStream(datafile, FileMode.Open)))
                {
                    // https://zenn.dev/flipflap/articles/a72a3fc40605f7
                    byte[] hdr = new byte[28];
                    hdr = reader.ReadBytes(28);
                    st_hipheaderinfo fil = new st_hipheaderinfo();
                    int p = 0;

                    fil.num = ToInt32(hdr, p);p += 4;
                    fil.firstnum = ToInt32(hdr, p); p += 4;
                    fil.count = ToInt32(hdr, p); p += 4;
                    fil.stnum = ToInt32(hdr, p); p += 4;

                    fil.mprop = ToInt32(hdr, p); p += 4;
                    fil.nmag = ToInt32(hdr, p); p += 4;
                    fil.NBENT = ToInt32(hdr, p); p += 4;

                    int size = fil.NBENT;
                    //https://qiita.com/kob58im/items/99b45df5bc2d75ccf58a

                    int errcnt = 0;
                    int ent = 0;
                    while (ent< fil.count) {
                        try
                        {
                            Byte[] buff = new Byte[size];
                            buff = reader.ReadBytes(size);
                            if (buff.Length < size)
                                break;

                            st_hipinfo hip = new st_hipinfo();
                            Console.WriteLine("st_hipinfo" + (hip ).ToString());
                            p = 0;
                           // hip.XNO = (Int32)(buff[p] | (buff[p + 1] << 8) | (buff[p + 2] << 16) | (buff[p + 3] << 24));
                            hip.XNO = ToInt32(buff, p);
                            p += 4;
                            hip.SRAD = ToDouble(buff, p);
                            p += 8;
                            hip.SDECO = ToDouble(buff, p);
                            p += 8;
                            hip.ISP0 = (char)buff[p]; p++;
                            hip.ISP1 = (char)buff[p]; p++;
                            hip.MAG100 = ToInt16(buff, p);
                            p += 2;
                            hip.XRPM_RA = ToSingle(buff, p);
                            p += 4;
                            hip.XDPM_Dec = ToSingle(buff, p);
                            p += 4;
                            hip.SVEL = (Int16)(buff[0]<< 8 | (buff[1] ));
                            if(hip.SRAD==0.0)
                            {
                                errcnt++;
                            //}
                            //{ 
                                Debug.WriteLine(errcnt.ToString() + " "+ ent.ToString() + " " + hip.XNO + " " + hip.SDECO.ToString("0.00") 
                                    + " " + hip.ISP0.ToString() +  hip.ISP1.ToString() 
                                    + " " + hip.MAG100.ToString()
                                    + " " + hip.XRPM_RA.ToString() + " " + hip.XDPM_Dec.ToString() +" "+ hip.SVEL.ToString());

                            }
                            else
                            {

                                Star particle = new Star(hip);
                                stars.Add(particle);
                            }
                            /*
                            hip.SRAD = (double)(buff[p] | (buff[p + 1] << 8) | (buff[p + 2] << 16) | (buff[p + 3] << 24) | (buff[p + 4] << 32) | (buff[p + 5] << 40) | (buff[p + 6] << 48) | (buff[p + 7] << 56));
                            p += 8;
                            hip.SDECO = (double)(buff[p] | (buff[p + 1] << 8) | (buff[p + 2] << 16) | (buff[p + 3] << 24) | (buff[p + 4] << 32) | (buff[p + 5] << 40) | (buff[p + 6] << 48) | (buff[p + 7] << 56));
                            p += 8;
                            hip.ISP0 = (char)buff[p]; p++;
                            hip.ISP1 = (char)buff[p]; p++;
                            hip.MAG100 = (Int16)(buff[0] | (buff[1] << 8));
                            p += 2;
                            hip.XRPM_RA = (float)(buff[p] | (buff[p + 1] << 8) | (buff[p + 2] << 16) | (buff[p + 3] << 24));
                            p += 4;
                            hip.XDPM_Dec = (float)(buff[p] | (buff[p + 1] << 8) | (buff[p + 2] << 16) | (buff[p + 3] << 24));
                            p += 4;
                            hip.SVEL = (Int16)(buff[0] | (buff[1] << 8));
                            */
                        }
                        catch(ArgumentOutOfRangeException ex)
                        {
                            Console.WriteLine(" error : " + ent.ToString());
                            break;
                        }
                        ent++;
                    }
                }

        //        // ファイルを開く＆文字化け防止
        //        // コードページ エンコーディング プロバイダーを登録
        //        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //        using (StreamReader file = new StreamReader(datafile, Encoding.GetEncoding("shift-jis")))
        //        {
        //            // 行末まで１行ずつ読み込む
        //            while (file.Peek() != -1)
        //            {
        //                string line = file.ReadLine();
        //                if (line != null)
        //                {
        //                    Star particle = new Star(line);
        //                    stars.Add(particle);
        //                    Console.WriteLine(line);
        //                }
        //            }
        //        }

            }

        }


    }
}
