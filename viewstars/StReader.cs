using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Drawing;

//using Alea;
//using Alea.Parallel;

//Tenqu

namespace ucPgmac
{
    /*メシエに似た深宇宙のコレクションとカタログ
     * 
     * http://www.messier.seds.org/xtra/similar/similar.html
     
     */

    /*  サグアロ天文学クラブ (SAC) 
     *  BEST OBJECTS IN THE NEW GENERAL CATALOG
        by A.J. Crayon and Steve Coe
        Version 1.0, dated Thu 01-04-1990*/
    //http://www.messier.seds.org/xtra/similar/similar.html

    /*  https://fujisanastro.typepad.jp/sky/  */ 
    public struct st_Messier   //1-110
    {
        public int index;
        public int NUM;
        public Cel.ngc_types NGC_Types;
        public PointCel Pos;
        public Cel.constellation constellation;// 所属星座：0～87
        public float MajorAngularSize;//面積：平方度
        public float MinorAngularSize;//面積：平方度 //0の時MajorAngularSize
        public byte PAngle;//NCP 線とのなす角度(反時計回り)
        public int Magx100;

    }

    public struct st_NGC   //1-7840
    {
        public int index;
        public int NUM;
        public Cel.ngc_types NGC_Types;
        public PointCel Pos;
        public byte ngc_shutten;//Cel.ngc_shutten[num] 参照元：1～18
        public Cel.constellation constellation;// 所属星座：0～87
        public float MajorAngularSize;//面積：平方度
        public float MinorAngularSize;//面積：平方度 //0の時MajorAngularSize
        public byte PAngle;//NCP 線とのなす角度(反時計回り)
        public int Magx100;
        public bool MagB;//等級種別 0：視等級、1：写真等級
        public Quaternion V;
    }

    public struct st_Star   //bsc5 Hip
    {
        public int index; //proper Name list index  "List<string> lstName"
        public int NUM;
        //    public Cel.ngc_types NGC_Types;
        public PointCel Pos;
        public byte ngc_shutten;//Cel.ngc_shutten[num] 参照元：1～18
        public Cel.constellation constellation;// 所属星座：0～87
        public float pmRA;//RA 固有運動(ラジアン/年)
        public float pmDE;//Dec.固有運動(ラジアン/年)
        public Color color;
        public int Magx100;
        public bool MagB;//等級種別 0：視等級、1：写真等級
        public Quaternion V;

    }


    public struct st_StarPos
    {
        public Quaternion P;//point of tenqu
        public Quaternion V;//point of sight
    }

    public struct st_StarLines   //1-7840
    {
        public int index;
        public int const_num;// 所属星座：1～89     public Cel.constellation constellation;
        public int HipNUM0;
        public int HipNUM1;
        public int Hipindex0;
        public int Hipindex1;
    }


    /// <summary>
    /// http://astro.starfree.jp/commons/milkyway/
    /// 天の川を円集合の濃淡で表現したデータです
    /// PP3 Celestial Charts Generator
    /// http://sourceforge.net/projects/pp3/
    /// Milky Way : Axel Mellinger
    /// </summary>
    public struct st_milkyway
    {   //円の半径は角距離で0.212°です。
        public Quaternion Quaternion;
        public byte v;         //濃度：0～255 (値が大きいほど濃い)
 //       public Quaternion Quaternion
 //       {
 //           get { return Pos.Quat; }
 //       }
        public Quaternion Vec;

        public float X
        {
            get { return Vec.X; }
        }
        public float Y
        {
            get { return Vec.Y; }
        }
        public float Z
        {
            get { return Vec.Z; }
        }
    }

    public class milkyway
    {
        st_milkyway[] datas;
        //milkyway.csv
        public milkyway(string path)
        {
            int num = 0;
            try
            {
                using (var reader = new StreamReader(path))
                {
                    string line = "";
                    List<st_milkyway> lst = new List<st_milkyway>();
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(',');
                        st_milkyway elem = new st_milkyway();

                        float ra, dec;
                        byte v;
                        float.TryParse(cells[0], out ra);
                        float.TryParse(cells[1], out dec);
                        byte.TryParse(cells[2], out v);
                        PointCel p = new PointCel(ra, dec);
                        elem.Quaternion = p.Quat;
                        elem.v = v;


                        if (v > 8)
                        {
                            lst.Add(elem);
                            num++;
                        }
                    }
                    datas = lst.ToArray();
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read milkyway:");
                Console.WriteLine(e.Message);
            }
        }
        public void rotate(Quaternion Q, float Magnitude)
        {
            if (datas != null && datas.Length > 0)
            {
                Quaternion CQ = Quaternion.Conjugate(Q);
                for (int i = 0; i < datas.Length; i++)
                {
                    datas[i].Vec = Q * datas[i].Quaternion * CQ * 180f * Magnitude;
                }
            }
        }
        public void rotate_stereo(Quaternion Q, float Magnitude)
        {
            if (datas != null && datas.Length > 0)
            {
                Quaternion CQ = Quaternion.Conjugate(Q);

        //      var gpu = Gpu.Default; // GPUインスタンス取得
        //      if (gpu != null)
        //      {
        //          gpu.For(0, datas.Length, i =>
        //          {
        //              datas[i].Vec = Q * datas[i].Quaternion * CQ;
        //              float dz = 2f / (1f + datas[i].Vec.Z) * 180f * Magnitude; //*= dz;
        //
        //              datas[i].Vec.X *= dz;
        //              datas[i].Vec.Y *= dz;
        //          });
        //          // i => Console.Write("{0} ", i)); // 0～99までスペースで区切って出力
        // //         gpu.Synchronize(); // ここで同期する
        //      }
        //      else
        //      {
                    for (int i = 0; i < datas.Length; i++)
                    {
                        datas[i].Vec = Q * datas[i].Quaternion * CQ;
                        float dz = 2f / (1f + datas[i].Vec.Z) * 180f * Magnitude; //*= dz;

                        datas[i].Vec.X *= dz;
                        datas[i].Vec.Y *= dz;
                    }

          //      }
            }
        }


        public void draw(Graphics g, float Magnitude)
        {
            if (datas != null && datas.Length > 0)
            {
                float size = 1.65f * Magnitude;// 0.212f *
                foreach (st_milkyway s in datas)
                {
                    if (s.Z > 0)
                    {
                        SolidBrush Brush = new SolidBrush(Color.FromArgb(s.v / 2, 200, 200, 150));
                        g.FillEllipse(Brush, s.X - size / 2, s.Y - size / 2, size, size);
                        g.FillEllipse(Brush, s.X - size *0.28f, s.Y - size *0.28f, size *0.56f, size *0.56f);
                    }
                }
            }
        }
    }

    public class StReader
    {
        /*
        T   text
        B   byte
        I   Int16
        L   Int32
        D   Double   
        S   Single  
        */

        public List<string> lstName = new List<string>();

        public string HipsNAME(st_Star star)
        {
            if (star.index < 0) return "";
            if (star.index > lstName.Count-1) return "";
            return lstName[star.index];

        }

        public st_NGC[] Messiers;
        public st_NGC[] NGCs;
        public st_NGC[] ICs;

        public st_NGC[] NGC2000s;
        public st_NGC[] IC2000s;
        public List<int> NGCs_index = new List<int>();
        public List<int> ICs_index = new List<int>();
        public List<int> NGC2000s_index = new List<int>();
        public List<int> IC2000s_index = new List<int>();

        public st_Star[] Hips;
        public List<int> Hips_index = new List<int>(); //HIPナンバーを入れる
        public st_Star[] Bsc5;
        public st_StarLines[] StarLines;
     //   public List<int> StarLines_index = new List<int>();

        #region nebula_Tool
        public Cel.ngc_types type2enum(string type)
        {
            if (type == " Gx")
            {
                return Cel.ngc_types.Galaxy;
            }
            else if (type == " OC")
            {
                return Cel.ngc_types.Open_star_cluster;
            }
            else if (type == " Gb")
            {
                return Cel.ngc_types.Globular_star_cluster;
            }
            else if (type == " Nb")
            {
                return Cel.ngc_types.Bright_emission_or_reflection_nebulav;
            }
            else if (type == " Pl")
            {
                return Cel.ngc_types.Planetary_nebula;
            }
            else if (type == "C+N")
            {
                return Cel.ngc_types.Cluster_associated_with_nebulosity;
            }
            else if (type == "Ast")
            {
                return Cel.ngc_types.Asterism_or_group_of_a_few_stars;
            }
            else if (type == " Kt")
            {
                return Cel.ngc_types.Knot_or_nebulous_region_in_an_external_galaxy;
            }
            else if (type == "***")
            {
                return Cel.ngc_types.Triple_star;
            }
            else if (type == " D*")
            {
                return Cel.ngc_types.Double_star;
            }
            else if (type == "  *")
            {
                return Cel.ngc_types.Single_star;
            }
            else if (type == "  ?")
            {
                return Cel.ngc_types.Uncertain_type_or_may_not_exist;
            }
            else if (type == "   ")
            {
                return Cel.ngc_types.Unidentified_at_the_place_given_or_type_unknown;
            }
            else if (type == "  -")
            {
                return Cel.ngc_types.Object_called_nonexistent_in_the_RNGC;
            }
            else if (type == " PD")
            {
                return Cel.ngc_types.Photographic_plate_defect;
            }
            else
            {
                return Cel.ngc_types.Other;
            }


        }
        public byte code2shutten(char type)
        {
            string codes = "AacdDFhkmnorstuvxz";
            int p = codes.IndexOf(type);
            return (byte)(p + 1);

        }

        #endregion

        /// <summary>
        /// AstroCommons  http://astro.starfree.jp/commons/mix/ngc.html
        /// ウィリアム・ハーシェルとその息子のジョン・ハーシェルが作ったジェネラルカタログを、
        /// ジョン・ドレイヤーが追補して1888年に発表した、連星・星雲・星団・銀河を収載した天体カタログ
        /// 、New General Catalogueに掲載されている天体データです。
        /// ngc.csv
        /// 
        ///列 内容
        ///1	NGC番号：1～7840
        ///2	天体種別：1～15
        ///3	赤経：時 1時=15°
        ///4	赤経：分（整数）1分=15*1/60°
        ///5	赤経：分（小数第一位）
        ///6	赤緯：符号 0：＋、1：－
        ///7	赤緯：度 0°～90°
        ///8	赤緯：分 1分=1/60°
        ///9	参照元：1～18
        ///10	所属星座：0～87
        ///11	面積：平方度（整数） 0：データなし
        ///12	面積：平方度（小数第一位）
        ///13	等級（整数） 0：データなし
        ///14	等級（小数第一位）
        ///15	等級種別 0：視等級、1：写真等級
        /// </summary>
        public st_NGC[] NgcRead(string path, char sep = ',')
        {
            List<st_NGC> lstNgc = new List<st_NGC>();
            if (!File.Exists(path))
            {
            }
            else
            {
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(sep);

                        st_NGC n = new st_NGC();
                        n.NUM = int.Parse(cells[0]);
                        n.NGC_Types = (Cel.ngc_types)int.Parse(cells[1]);
                        double ra = int.Parse(cells[2]);
                        ra += (float)(int.Parse(cells[3]) / 60f);
                        ra += (float)(int.Parse(cells[4]) / 600f);
                        double dec = (float)(int.Parse(cells[6]));
                        dec += (float)(int.Parse(cells[7]) / 60f);
                        dec *= cells[5] == "1" ? -1 : 1;
                        n.Pos = new PointCel(ra, dec);
                        n.ngc_shutten = Byte.Parse(cells[8]);
                        n.constellation = (Cel.constellation)Byte.Parse(cells[9]);
                        int i= int.Parse(cells[10]);
                        if (i == 0)
                        {
                            n.MajorAngularSize =  4f;
                        }
                        else
                        {
                            float SquareDeg = (float)int.Parse(cells[10]);
                            SquareDeg += (float)int.Parse(cells[11]) / 10f;
                            n.MajorAngularSize = (float)Math.Sqrt(SquareDeg) * 8f;
                        }
                        if (int.Parse(cells[12]) == 0)
                        {
                            n.Magx100 = 1500;
                        }
                        else { 
                            n.Magx100 = int.Parse(cells[12]) * 100;
                            n.Magx100 += int.Parse(cells[13]) * 10;
                        }
                        n.MagB = (cells[14] == "1" ? true : false);

                        lstNgc.Add(n);
                        NGCs_index.Add(n.NUM);
                  //      Console.WriteLine(line);
                    }
                }
            }
            NGCs = lstNgc.ToArray();
            return NGCs;
        }

        //https://heasarc.gsfc.nasa.gov/W3Browse/all/ngc2000.html
        //https://cdsarc.cds.unistra.fr/ftp/cats/VII/118/
        /*
            VII/118               NGC 2000.0            (Sky Publishing, ed. Sinnott 1988)
            The following files can be converted to FITS (extension .fit or fit.gz)
	        ngc2000.dat names.dat
        I1529      0 05.2  -11 30 d  Cet              F, S, R, biN, r                                   *
        */
        public void Ngc2000Read(string path, char sep = '\t', string linefeed = "\r\n")
        {
            List<string> lsttype = new List<string>();

            if (!File.Exists(path))
            {

            }
            else
            {
                List<st_NGC> lstNgc = new List<st_NGC>();
                List<st_NGC> lstIc = new List<st_NGC>();
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        //    string[] cells = line.Split(sep);
                        st_NGC n = new st_NGC();

                        //  1 - 5  A5-- - Name     NGC or IC designation(preceded by I)
                        //       line[0] == 'I' ?
                        int.TryParse(line.Substring(1, 4), out n.NUM);

                        //  7 - 9  A3-- - Type     Object classification(1)
                        string type = line.Substring(6, 3);
                        if (!lsttype.Contains(type)) lsttype.Add(type);
                        n.NGC_Types = type2enum(type);
                        // 11 - 12  I2 h       RAh Right Ascension B2000(hours)
                        // 14 - 17  F4.1   min RAm      Right Ascension B2000(minutes)
                        int r = 0;
                        int.TryParse(line.Substring(10, 2), out r);
                        float rf;
                        float.TryParse(line.Substring(13, 4), out rf);
                        float ra = (float)r + rf / 60f;
                        //     20  A1-- - DE - Declination B2000(sign)
                        // 21 - 22  I2 deg     DEd Declination B2000(degrees) 
                        // 24 - 25  I2 arcmin  DEm Declination B2000(minutes)

                        int.TryParse(line.Substring(20, 2), out r);
                        float dec = (float)r;
                        int.TryParse(line.Substring(23, 2), out r);
                        dec += (float)r / 60f;
                        if (line[19] == '-') dec = -dec;
                        n.Pos = new PointCel(ra, dec);
                        //     27  A1-- - Source   Source of entry(2)
                        n.ngc_shutten = code2shutten(line[26]);
                        // 30 - 32  A3-- - Const    Constellation
                        //文字列からENUM
                        n.constellation = (Cel.constellation)Enum.Parse(typeof(Cel.constellation), line.Substring(29, 3));
                        //     33  A1-- - l_size[<] Limit on Size

                        // 34 - 38  F5.1   arcmin size     ? Largest dimension
                        float.TryParse(line.Substring(33, 5), out n.MajorAngularSize);
                        if (n.MajorAngularSize == 0) n.MajorAngularSize = 4f;
                        // 41 - 44  F4.1   mag mag      ? Integrated magnitude, visual or photographic
                        //                                  (see n_mag)
                        float.TryParse(line.Substring(33, 5), out rf);
                        n.Magx100 = (int)(rf * 100);
                        if (n.Magx100 == 0) n.Magx100 = 1500;
                        //     45  A1-- - n_mag[p] 'p' if mag is photographic(blue)
                        n.MagB = (line[44] == 'p');
                        // 47 - 99  A53-- - Desc     Description of the object(3)

                        if (line[0] == 'I')
                        {
                            lstIc.Add(n);
                            IC2000s_index.Add(n.NUM);
                        }
                        else
                        {
                            lstNgc.Add(n);
                            NGC2000s_index.Add(n.NUM);
                        }
                        Console.WriteLine(line);
                    }
                    NGC2000s = lstNgc.ToArray();
                    IC2000s = lstIc.ToArray();
                }
            }
        }

   // Byte-by-byte Description of file: names.dat
   // --------------------------------------------------------------------------------
   //    Bytes Format Units Label     Explanations
   // --------------------------------------------------------------------------------
   //    1- 35  A35   ---     Object Common name(including Messier numbers)
   //   37- 41  A5    ---     Name* NGC or IC name, as in ngc2000.dat
   //   43- 70  A28   ---     Comment Text of comment, if any


        public void NgcNameRead(string path)
        {
            // names.dat
            List<st_StarLines> lst = new List<st_StarLines>();
            if (!File.Exists(path))
            {
            }
            else
            {
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length < 37) continue;

                        //    string[] cells = line.Split(sep);

                        //  1 - 5  A5-- - Name     NGC or IC designation(preceded by I)
                        //       line[0] == 'I' ?

                        //  7 - 9  A3-- - Type     Object classification(1)
                        string name = line.Substring(0, 35);

                        name = name.Trim();

                        int ngcnum = 0;
                        int.TryParse(line.Substring(37, 4), out ngcnum);

                        if (ngcnum > 0)
                        {
                            string disc = "";
                            if (line.Length > 42) disc = line.Substring(42).Trim();
                            if (disc.Length > 0) name = name + "\r\n" + disc;
                            //Hip  regist name to hip
                            if (line[36] == 'I')
                            {
                                //IC
                                int index = ICs_index.IndexOf(ngcnum);
                                int index2000 = IC2000s_index.IndexOf(ngcnum);
                                if (index >= 0)
                                {
                                    ICs[index].index = lstName.Count;
                                }
                                if (index2000 >= 0)
                                {
                                    IC2000s[index2000].index = lstName.Count;
                                }
                                if (index >= 0 || index2000 >= 0)
                                {
                                    lstName.Add(name);
                                }
                            }
                            else
                            {
                                int index = NGCs_index.IndexOf(ngcnum);
                                int index2000 = NGC2000s_index.IndexOf(ngcnum);

                                if (index >= 0)
                                {
                                    NGCs[index].index = lstName.Count;
                                }
                                if (index2000 >= 0)
                                {
                                    NGC2000s[index2000].index = lstName.Count;
                                }
                                if (index >= 0 || index2000 >= 0)
                                {
                                    NGCs[index].index = lstName.Count;
                                    lstName.Add(name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void MessierRead(string path, char sep = '\t', string linefeed = "\r\n")
        {
            if (!File.Exists(path))
            {

            }
            else
            {
                List<st_NGC> lstMessier = new List<st_NGC>();
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(sep);
                        st_NGC m = new st_NGC();
                        try
                        {
                            m.NUM = int.Parse(cells[0]);
                            double szmaj = 0;
                            double szmin = 0;
                            byte pa = 0;

                            if (Cel.SizeStringToval(cells[1], ref szmaj, ref szmin, ref pa))
                            {
                                m.MajorAngularSize = (float)szmaj;
                                m.MinorAngularSize = (float)szmin;
                            }

                            double ra = 0;
                            double de = 0;
                            if (Cel.RaDecStringToEquatorial(cells[2], ref ra, ref de))
                            {
                                m.Pos = new PointCel(ra, de);
                            }

                            //  m.NGC_Types = (Cel.ngc_types)int.Parse(cells[1]);

                        }
                        catch (Exception e)
                        {

                        }
                        lstMessier.Add(m);
                        Console.WriteLine(line);
                    }
                    Messiers = lstMessier.ToArray();
                }
            }
        }

        #region HipRead Binary tools
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
        public static float ToSingle(byte[] value, int startIndex)
        {
            byte[] sub = GetSubArray(value, startIndex, sizeof(float));
            return BitConverter.ToSingle(Reverse(sub), 0);
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
        #endregion tools

        public void HipLineRead(string path)
        {
            // hip_constellation_line.csv
            List<st_StarLines> lst = new List<st_StarLines>();
            if (!File.Exists(path))
            {      }
            else
            {
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(',');

                        st_StarLines n = new st_StarLines();
                        Cel.constellation cons = (Cel.constellation)Enum.Parse(typeof(Cel.constellation), cells[0]);
                        n.const_num = (int)cons+1;
                        if (n.const_num > 74) n.const_num++;
                        //n.const_num = int.Parse(cells[0]);
                        n.HipNUM0 = int.Parse(cells[1]);
                        n.HipNUM1 = int.Parse(cells[2]);
                        n.Hipindex0 = Hips_index.IndexOf(n.HipNUM0);
                        n.Hipindex1 = Hips_index.IndexOf(n.HipNUM1);

                        if (n.Hipindex0 > 0 && n.Hipindex1 > 0)
                            lst.Add(n);
                    }
                }
                StarLines = lst.ToArray();
            }
        }

        /// <summary>
        /// hip_proper_name.csv
        /// </summary>
        /// <param name="path"></param>
        public void HipNameRead(string path)
        {
            // hip_proper_name.csv
            if (!File.Exists(path))
            {
            }
            else
            {
                using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(',');
                        int hipnum = int.Parse(cells[0]);

                        int index = Hips_index.IndexOf(hipnum);
                        //Hip  regist name to hip
                        if (hipnum > 0 && index >= 0)
                        {
                            Hips[index].index = lstName.Count;
                            lstName.Add(cells[1]);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// hip_100.csv
        /// 1	HIP番号
        /// 2	赤経：時（整数）
        /// 3	赤経：分（整数）
        /// 4	赤経：秒（小数）
        /// 5	赤緯：符号（0：- 1：+）
        /// 6	赤緯：度（整数）
        /// 7	赤緯：分（整数）
        /// 8	赤緯：秒（小数）
        /// 9	視等級：等級（小数）
        /// 10	年周視差（ミリ秒）
        /// 11	赤経方向固有運動（ミリ秒）
        /// 12	赤緯方向固有運動（ミリ秒）
        /// 13	B-V色指数
        /// 14	スペクトル分類
        /// 15	星座：略符
        /// 16	バイエル符号
        /// 17	固有名（英語）
        /// 18	固有名（日本語）
        /// </summary>
        /// <param name="path"></param>
        public void Hip100Read(string path)
        {
            //名称付きのデータ(詳細）

            // http://astro.starfree.jp/commons/hip/
            // hip_100.csv
            if (!File.Exists(path))
            {
            }
            else
            {
                //  0   1   2  3    4 5   6  7    8     9    10     11     12     13   14  15   16     17
                //59803,12,15,48.47,0,17,32,31.1,2.59,19.78,-159.58,22.31,-0.107,B8III,Crv,γ,Gienah,からす座ガンマ星
                List<st_Star> lstHip = new List<st_Star>();

              //  using (var reader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                using (var reader = new StreamReader(path, Encoding.GetEncoding("shift-jis")))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] cells = line.Split(',');
                        st_Star s = new st_Star();
                        s.index = -1;
                        s.NUM = int.Parse(cells[0]);

                        //s.NGC_Types = (Cel.ngc_types)int.Parse(cells[1]);

                        double ra = int.Parse(cells[1]);
                        ra += (float)(int.Parse(cells[2]) / 60f);
                        ra += float.Parse(cells[3]) / 3600f;

                        double dec = (float)(int.Parse(cells[5]));
                        dec += (float)(int.Parse(cells[6]) / 60f);
                        dec += (float.Parse(cells[7]) / 3600f);
                        dec *= cells[4] == "0" ? -1 : 1;

                        s.Pos = new PointCel(ra, dec);

                        s.Magx100 = (int)(float.Parse(cells[8])*100);

                        s.pmRA = float.Parse(cells[10]);//赤経方向固有運動（ミリ秒）
                        s.pmDE = float.Parse(cells[11]);//赤緯方向固有運動（ミリ秒）

                        int b_V =(int)( float.Parse(cells[12]) * 50);//B-V色指数
                        int r = 200 + b_V*3;
                        int g = 200 - b_V*1;
                        int b =(int)(200 - b_V * 2);
                        if (r < 0) r = 0; else if (r > 255) r = 255;
                        if (g < 0) g = 0; else if (g > 255) g = 255;
                        if (b < 0) b = 0; else if (b > 255) b = 255;
                        s.color = Color.FromArgb(200,r,g,b);

                        //(cells[13]); //スペクトル分類
                        //文字列からENUM
                        s.constellation = (Cel.constellation)Enum.Parse(typeof(Cel.constellation),cells[14]);//星座：略符
                        string constellationCode = cells[15];//バイエル符号
                        string name = cells[16];//固有名（英語）
                        string namae = cells[17];//固有名（日本語）

                        //Hip  regist name to hip
                        if (s.NUM > 0 )
                        {
                            s.index = lstName.Count;
                            lstName.Add(name+"/"+namae);
                        }     
                        
                        lstHip.Add(s);
                        Hips_index.Add(s.NUM);

                    }
                    Hips = lstHip.ToArray();
                }
            }
        }


        /// <summary>
        /// hipparcos binary datas
        /// </summary>
        /// <param name="path"></param>
        public void HipRead(string path)
        {
            if (!File.Exists(path)){}
            else
            {
         //       Hips_index.Clear();
                List<st_Star> lstHip = new List<st_Star>();
                if(Hips.Length>0)
                    lstHip.AddRange(Hips);

                using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
                {
                    // https://zenn.dev/flipflap/articles/a72a3fc40605f7
                    byte[] hdr = new byte[28];
                    hdr = reader.ReadBytes(28);
                    st_hipheaderinfo fil = new st_hipheaderinfo();
                    int p = 0;
                    fil.num = ToInt32(hdr, p); p += 4;
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
                    while (ent < fil.count)
                    {
                        try
                        {
                            Byte[] buff = new Byte[size];
                            buff = reader.ReadBytes(size);
                            if (buff.Length < size)
                                break;

                            st_hipinfo hip = new st_hipinfo();
                            //Console.WriteLine("st_hipinfo" + (hip).ToString());
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
                            hip.SVEL = (Int16)(buff[0] << 8 | (buff[1]));

                            st_Star s = new st_Star();

                            s.index = -1;
                            s.NUM = hip.XNO;
                            s.Pos = new PointCel(hip.SRAD*12.0/Math.PI, hip.SDECO*180.0/Math.PI);
                            s.pmRA = hip.XRPM_RA;
                            s.pmDE = hip.XDPM_Dec;
                            s.Magx100 = hip.MAG100;
                            switch (hip.ISP0)
                            {
                                case 'O':
                                    s.color = Color.FromArgb(200, 200, 200, 255);// w = (float)(0x44f);// 45000;
                                    break;
                                case 'B':
                                    s.color = Color.FromArgb(200, 150, 150, 250);// w = (float)(0x88f);// 15000;
                                    break;
                                case 'A':
                                    s.color = Color.FromArgb(200, 200, 200, 200);// w = (float)(0xfff);// 83000;
                                    break;
                                case 'F':
                                    s.color = Color.FromArgb(200, 220, 220, 100);// w = (float)(0xff8);// 6600;
                                    break;
                                case 'G':
                                    s.color = Color.FromArgb(200, 250, 250, 0);// w = (float)(0x880);// 5600;
                                    break;
                                case 'K':
                                    s.color = Color.FromArgb(200, 250, 200, 0);// w = (float)(0xC80);// 4400;
                                    break;
                                case 'M':
                                    s.color = Color.FromArgb(200, 255, 0, 0);// w = (float)(0xF44);// 3300;
                                    break;
                                default:
                                    s.color = Color.FromArgb(200, 120, 120, 120);// w = (float)(0x888);// 0;
                                    break;

                            }

                            int index = Hips_index.IndexOf(s.NUM);

                            if (index >= 0)
                            {
                                //すでに存在
                            }
                            else if(hip.MAG100 == 0)
                            {
                                Console.WriteLine(errcnt.ToString() + " " + ent.ToString() + " " + hip.XNO + " " + hip.SDECO.ToString("0.00")
                                + " " + hip.ISP0.ToString() + hip.ISP1.ToString()
                                + " " + hip.MAG100.ToString()
                                + " " + hip.XRPM_RA.ToString() + " " + hip.XDPM_Dec.ToString() + " " + hip.SVEL.ToString());
                                errcnt++;
                            }
                            else if (hip.SRAD == 0.0 )
                            {
                                Console.WriteLine(errcnt.ToString() + " " + ent.ToString() + " " + hip.XNO + " " + hip.SDECO.ToString("0.00")
                                + " " + hip.ISP0.ToString() + hip.ISP1.ToString()
                                + " " + hip.MAG100.ToString()
                                + " " + hip.XRPM_RA.ToString() + " " + hip.XDPM_Dec.ToString() + " " + hip.SVEL.ToString());
                                errcnt++;
                            }
                            else
                            {
                                lstHip.Add(s);
                                Hips_index.Add(s.NUM);
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
                        catch (ArgumentOutOfRangeException e)
                        {
                            Console.WriteLine(" error : " + ent.ToString());
                            break;
                        }
                        ent++;
                    }
                    Hips = lstHip.ToArray();

                }
            }
        }

        //Create bsc5 data datafile = @"bsc5.dat";
        public void Bsc5Read(string path)
        {
            //存在確認
            if (File.Exists(path))
            {
                // filePathのファイルは存在する
                List<st_Star> lstBsc5 = new List<st_Star>();

                // ファイルを開く＆文字化け防止
                // コードページ エンコーディング プロバイダーを登録
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (StreamReader file = new StreamReader(path, Encoding.GetEncoding("shift-jis")))
                {
                    // 行末まで１行ずつ読み込む
                    while (file.Peek() != -1)
                    {
                        string line = file.ReadLine();
                        if (line != null)
                        {
                            st_Star s = new st_Star();
                            try
                            {
                                s.index = -1;

                                string RAh = line.Substring(75, 2);      // 76- 77  I2     h       RAh      ?Hours RA, equinox J2000, epoch 2000.0 (1)
                                string RAm = line.Substring(77, 2);      // 78- 79  I2     min     RAm      ?Minutes RA, equinox J2000, epoch 2000.0 (1)
                                string RAs = line.Substring(79, 4);      // 80- 83  F4.1   s       RAs      ?Seconds RA, equinox J2000, epoch 2000.0 (1)
                                string DE_ = line.Substring(83, 1);      //     84  A1     ---     DE-      ?Sign Dec, equinox J2000, epoch 2000.0 (1)
                                string DEd = line.Substring(84, 2);      // 85- 86  I2     deg     DEd      ?Degrees Dec, equinox J2000, epoch 2000.0 (1)
                                string DEm = line.Substring(86, 2);      // 87- 88  I2     arcmin  DEm      ?Minutes Dec, equinox J2000, epoch 2000.0 (1)
                                string DEs = line.Substring(88, 2);      // 89- 90  I2     arcsec  DEs      ?Seconds Dec, equinox J2000, epoch 2000.0 (1)
                                double _RA = double.Parse(RAh) + double.Parse(RAm) / 60 + double.Parse(RAs) / 3600;
                                double _DEC = (DE_ == "-" ? -1 : 1) * (double.Parse(DEd) + double.Parse(DEm) / 60 + double.Parse(DEs) / 3600);

                                s.Pos = new PointCel(_RA, _DEC);

                                //_GLON = float.Parse(line.Substring(90, 6));    // 91- 96  F6.2   deg     GLON     ?Galactic longitude (1)
                                //_GLAT = float.Parse(line.Substring(96, 6));    // 97-102  F6.2   deg     GLAT     ?Galactic latitude (1)
                                s.Magx100 = (int)(float.Parse(line.Substring(102, 5)) * 100);   //103-107  F5.2   mag     Vmag     ?Visual magnitude (1)
                                //n_Vmag = line.Substring(107, 1);             //    108  A1     ---   n_Vmag    *[ HR] Visual magnitude code
                                //string u_Vmag = line.Substring(108, 1);      //    109  A1     ---   u_Vmag     [ :?] Uncertainty flag on V
                                float B_V, U_B, R_I;
                                float.TryParse(line.Substring(109, 5), out B_V);    //110-114  F5.2   mag     B-V      ? B-V color in the UBV system
                                                                                    //string u_B_V = line.Substring(114, 1);              //    115  A1     ---   u_B-V      [ :?] Uncertainty flag on B-V
                                float.TryParse(line.Substring(115, 5), out U_B);    //116-120  F5.2   mag     U-B      ? U-B color in the UBV system
                                                                                    //string u_U_B = line.Substring(120, 1);              //    121  A1     ---   u_U-B      [ :?] Uncertainty flag on U-B
                                float.TryParse(line.Substring(121, 5), out R_I);    //122-126  F5.2   mag     R-I      ? R-I   in system specified by n_R-I
                                                                                    //string n_R_I = line.Substring(126, 1);              //    127  A1     ---   n_R-I      [CE:?D] Code for R - I system(Cousin, Eggen)
                                if (B_V != 0)
                                {
                                    int b_V = (int)(B_V);//B-V色指数
                                    int r = 200 + b_V * 3;
                                    int g = 200 - b_V * 1;
                                    int b = (int)(200 - b_V * 2);
                                    if (r < 0) r = 0; else if (r > 255) r = 255;
                                    if (g < 0) g = 0; else if (g > 255) g = 255;
                                    if (b < 0) b = 0; else if (b > 255) b = 255;
                                    s.color = Color.FromArgb(200, r, g, b);
                                }
                                else
                                {
                                    s.color = Color.FromArgb(200, 200,200,200);
                                }
                                //Annual proper motion (arcsec/yr)
                                s.pmRA = float.Parse(line.Substring(148, 6));         //149-154  F6.3 arcsec / yr pmRA *? Annual proper motion in RA J2000, FK5 system
                                s.pmDE = float.Parse(line.Substring(154, 6));         //155-160  F6.3 arcsec / yr pmDE ? Annual proper motion in Dec J2000, FK5 system
                                if (s.Pos.Ra != 0.0 && s.Pos.Dec != 0.0) lstBsc5.Add(s);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Err " + ex.ToString());
                            }
                            //Console.WriteLine(line);
                        }
                        Bsc5 = lstBsc5.ToArray();
                    }
                }

            }
        }
    }
}
