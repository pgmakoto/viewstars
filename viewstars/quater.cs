using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;

namespace ucPgmac
{
    public class IntEventArgs
    {
        public IntEventArgs(int value) { Value = value; }
        public int Value { get; } // readonly
    }

    public delegate void IntEventHandler(object sender, IntEventArgs e);

    public class BoolEventArgs
    {
        public BoolEventArgs(bool value) { Value = value; }
        public bool Value { get; } // readonly
    }
    public delegate void BoolEventHandler(object sender, BoolEventArgs e);
    public class SampleEventArgs
    {
        public SampleEventArgs(string value) { Value = value; }
        public string Value { get; } // readonly
    }
    public delegate void SampleEventHandler(object sender, SampleEventArgs e);

    public class CelChangeEventArgs : EventArgs
    {
        //PointCelにしちゃったほうがいい？ ->しちゃった
        public PointCel Value { get; } // readonly
        public CelChangeEventArgs(PointCel value) { Value = value; }
        public CelChangeEventArgs(double _ra, double _dec) { Value = new PointCel(_ra, _dec); }
    }
    public delegate void CelEventHandler(object sender, CelChangeEventArgs e);


    /// <summary>
    /// Celestorial Utils
    /// </summary>
    public class Cel
    {
        public string[] ngc_shutten = new string[]{
            "0, no entry",
            "1,Archinal; Brent A.Version 4.0 of an unpublished list of errata to the RNGC; dated March 19; 1987. (110;0)",
            "2,Arp; H.; \"Atlas of Peculiar Galaxies\"; 1966ApJS...14....1A(1;2) (Catalog<VII/74>)",
            "3,Corwin; Harold G.; Jr.; A.de Vaucouleurs; and G.de Vaucouleurs; \"Southern Galaxy Catalogue\"; Austin; Texas: University of Texas Monographs in Astronomy No. 4; 1985. (152;564) (Catalog<VII/116>)",
            "4,Dreyer; J.L.E.; New General Catalogue of Nebulae and Clusters of Stars(1888); Index Catalogue(1895); Second Index Catalogue(1908). London: Royal Astronomical Society; 1953. (28;2157)",
            "5,Dreyer; J.L.E.; ibid.Errata on pages 237; 281-283; and 366-378. (158;28)",
            "6,Skiff; Brian; private communication of February 27; 1988.  (93;36)",
            "7,Holmberg; E.; \"A Study of Double and Multiple Galaxies\"; Lund Annals; 6; 1937. (13;2)",
            "8,Karachentsev; I.D.; \"A Catalogue of Isolated Pairs of Galaxies in the Northern Hemisphere\"; also; Karachentseva; V.E.; \"A Catalog of Isolated Galaxies.\" Astrofiz.Issled.Izv.Spetz.Astrofiz.; 7; 3; 1972; and 8; 3; 1973. (0;4) (Catalogs<VII/77>; <VII/82>; <VII/83>)",
            "9,Vorontsov-Velyaminov; B.A.; and V.P.Arhipova; \"Morphological Catalog of Galaxies\"; Parts I-V.Moscow: Moscow State University; 1962-74. (9;679) (Catalogs<VII/62> and <VII/100>)",
            "10,Reinmuth; K.; \"Photographische Positionsbestimmung von NebelRecken\" Veroff der Sternwarte zu Heidelberg; several papers; 1916-40. (0;4)",
            "11,Alter; G.; B.Balazs; and J.Ruprecht; Catalogue of Star Clusters and Associations; 2nd edition.Budapest: Akademiai Kiado; 1970. (5;0) (Catalogs<VII/5>; <VII/44> and<VII/101>)",
            "12,Sulentic; Jack W.; and William G.Tifft; \"The Revised New General Catalogue of Nonstellar Astronomical Objects (RNGC)\". Tucson; Arizona:University of Arizona Press; 1973. (4016;0) (Catalog<VII/1>)",
            "13,Hirshfeld; Alan; and Roger W.Sinnott; eds.; Sky Catalogue 2000.0; Vol.2; Cambridge; Massachusetts: Sky Publishing Corp.and Cambridge University Press; 1985. (3098;238)",
            "14,Tully; R.B.; \"Nearby Galaxies Catalog\". New York: Cambridge University Press; 1988. A preliminary version on magnetic tape(1981) was used here. (23;17) (Catalog<VII/145>)",
            "15,Nilson P.N.; Uppsala Ceneral Catalogue of Galaxies.Uppsala: Uppsala Astronomical Observatory; 1973. (15;543) (Catalog<VII/26>)",
            "16,de Vaucouleurs; G.; A.de Vaucouleurs; and H.C.Corvin; Jr.; Second Reference Catalogue of Bright Galaxies.Austin; Texas; University of Texas Press; 1976.(118;206) (Catalog<VII/112>)",
            "17,Dixon; R.S.; and George Sonneborn; \"A Master List of Nonstellar Optical Astronomical Objects (MOL)\".  Columbus; Ohio; Ohio State University Press; 1980. It should be noted that most of the information for codes a; h;k;m;n;o;u and z was extracted from the magnetic-tape version of this catalogue.The x code refers to IC objects identified in a literature search by these authors. (0;526)",
            "18,Zwicky; F.; E. Herzog; and P. Wild; \"Catalogue of Galaxies and Clusters of Galaxies\"; Vol.I. Pasadena; Calif.; California Institute of Technology; 1961. Also; successive volumes through 1968. (1;380) (Catalog <VII/49>)"

            };
        public string[] messier;

        public enum messier_types
        {
            銀河 = 1,
            星団 = 2,
            星雲 = 3,
            その他 = 0,//M40_二重星,M73_星群
        }
        public enum ngc_types
        {
            Other = 0,
            Galaxy = 1,
            Open_star_cluster = 2,
            Globular_star_cluster = 3,
            Bright_emission_or_reflection_nebulav = 4,
            Planetary_nebula = 5,
            Cluster_associated_with_nebulosity = 6,
            Asterism_or_group_of_a_few_stars = 7,
            Knot_or_nebulous_region_in_an_external_galaxy = 8,
            Triple_star = 9,
            Double_star = 10,
            Single_star = 11,
            Uncertain_type_or_may_not_exist = 12,
            Unidentified_at_the_place_given_or_type_unknown = 13,
            Object_called_nonexistent_in_the_RNGC = 14,
            Photographic_plate_defect = 15,
        };
        public enum constellation
        {
            And = 0, // "And,Andromeda,アンドロメダ",
            Ant = 1, // "Ant,Antlia,ポンプ",
            Aps = 2, // "Aps,Apus,ふうちょう",
            Aql = 3, // "Aql,Aquila,わし",
            Aqr = 4, // "Aqr,Aquarius,みずがめ",
            Ara = 5, // "Ara,Ara,さいだん",
            Ari = 6, // "Ari,Aries,おひつじ",
            Aur = 7, // "Aur,Auriga,ぎょしゃ",
            Boo = 8, // "Boo,Bootes,うしかい",
            Cae = 9, // "Cae,Caelum,ちょうこくぐ",
            Cam = 10,// "Cam,Camelopardalis,きりん",
            Cap = 11,// "Cap,Capricornus,やぎ",
            Car = 12,// "Car,Carina,りゅうこつ",
            Cas = 13,// "Cas,Cassiopeia,カシオペヤ",
            Cen = 14,// "Cen,Centaurus,ケンタウルス",
            Cep = 15,// "Cep,Cepheus,ケフェウス",
            Cet = 16,// "Cet,Cetus,くじら",
            Cha = 17,// "Cha,Chamaeleon,カメレオン",
            Cir = 18,// "Cir,Circinus,コンパス",
            CMa = 19,// "CMa,Canis Major,おおいぬ",
            CMi = 20,// "CMi,Canis Minor,こいぬ",
            Cnc = 21,// "Cnc,Cancer,かに",
            Col = 22,// "Col,Columba,はと",
            Com = 23,// "Com,Coma Berenices,かみのけ",
            CrA = 24,// "CrA,Corona Australis,みなみのかんむり",
            CrB = 25,// "CrB,Corona Borealis,かんむり",
            Crt = 26,// "Crt,Crater,コップ",
            Cru = 27,// "Cru,Crux,みなみじゅうじ",
            Crv = 28,// "Crv,Corvus,からす",
            CVn = 29,// "CVn,Canes Venatici,りょうけん",
            Cyg = 30,// "Cyg,Cygnus,はくちょう",
            Del = 31,// "Del,Delphinus,いるか",
            Dor = 32,// "Dor,Dorado,かじき",
            Dra = 33,// "Dra,Draco,りゅう",
            Equ = 34,// "Equ,Equuleus,こうま",
            Eri = 35,// "Eri,Eridanus,エリダヌス",
            For = 36,// "For,Fornax,ろ",
            Gem = 37,// "Gem,Gemini,ふたご",
            Gru = 38,// "Gru,Grus,つる",
            Her = 39,// "Her,Hercules,ヘルクレス",
            Hor = 40,// "Hor,Horologium,とけい",
            Hya = 41,// "Hya,Hydra,うみへび",
            Hyi = 42,// "Hyi,Hydrus,みずへび",
            Ind = 43,// "Ind,Indus,インディアン",
            Lac = 44,// "Lac,Lacerta,とかげ",
            Leo = 45,// "Leo,Leo,しし",
            Lep = 46,// "Lep,Lepus,うさぎ",
            Lib = 47,// "Lib,Libra,てんびん",
            LMi = 48,// "LMi,Leo Minor,こじし",
            Lup = 49,// "Lup,Lupus,おおかみ",
            Lyn = 50,// "Lyn,Lynx,やまねこ",
            Lyr = 51,// "Lyr,Lyra,こと",
            Men = 52,// "Men,Mensa,テーブルさん",
            Mic = 53,// "Mic,Microscopium,けんびきょう",
            Mon = 54,// "Mon,Monoceros,いっかくじゅう",
            Mus = 55,// "Mus,Musca,はえ",
            Nor = 56,// "Nor,Norma,じょうぎ",
            Oct = 57,// "Oct,Octans,はちぶんぎ",
            Oph = 58,// "Oph,Ophiuchus,へびつかい",
            Ori = 59,// "Ori,Orion,オリオン",
            Pav = 60,// "Pav,Pavo,くじゃく",
            Peg = 61,// "Peg,Pegasus,ペガスス",
            Per = 62,// "Per,Perseus,ペルセウス",
            Phe = 63,// "Phe,Phoenix,ほうおう",
            Pic = 64,// "Pic,Pictor,がか",
            PsA = 65,// "PsA,Piscis Austrinus,みなみのうお",
            Psc = 66,// "Psc,Pisces,うお",
            Pup = 67,// "Pup,Puppis,とも",
            Pyx = 68,// "Pyx,Pyxis,らしんばん",
            Ret = 69,// "Ret,Reticulum,レチクル",
            Scl = 70,// "Scl,Sculptor,ちょうこくしつ",
            Sco = 71,// "Sco,Scorpius,さそり",
            Sct = 72,// "Sct,Scutum,たて",
            Ser = 73,// "Ser,Serpens(Caput),へび(頭)",
                    // "Ser,Serpens(Cauda),へび(尾)",
            Sex = 74,// "Sex,Sextans,ろくぶんぎ",
            Sge = 75,// "Sge,Sagitta,や",
            Sgr = 76,// "Sgr,Sagittarius,いて",
            Tau = 77,// "Tau,Taurus,おうし",
            Tel = 78,// "Tel,Telescopium,ぼうえんきょう",
            TrA = 79,// "TrA,Triangulum Australe,みなみのさんかく",
            Tri = 80,// "Tri,Triangulum,さんかく",
            Tuc = 81,// "Tuc,Tucana,きょしちょう",
            UMa = 82,// "UMa,Ursa Major,おおぐま",
            UMi = 83,// "UMi,Ursa Minor,こぐま",
            Vel = 84,// "Vel,Vela,ほ",
            Vir = 85,// "Vir,Virgo,おとめ",
            Vol = 86,// "Vol,Volans,とびうお",
            Vul = 87,         // "Vul,Vulpecula,こぎつね",

        }
        
        public string[] constlation_name = new string[]
        {
            "And,Andromeda,アンドロメダ",
            "Ant,Antlia,ポンプ",
            "Aps,Apus,ふうちょう",
            "Aql,Aquila,わし",
            "Aqr,Aquarius,みずがめ",
            "Ara,Ara,さいだん",
            "Ari,Aries,おひつじ",
            "Aur,Auriga,ぎょしゃ",
            "Boo,Bootes,うしかい",
            "Cae,Caelum,ちょうこくぐ",
            "Cam,Camelopardalis,きりん",
            "Cap,Capricornus,やぎ",
            "Car,Carina,りゅうこつ",
            "Cas,Cassiopeia,カシオペヤ",
            "Cen,Centaurus,ケンタウルス",
            "Cep,Cepheus,ケフェウス",
            "Cet,Cetus,くじら",
            "Cha,Chamaeleon,カメレオン",
            "Cir,Circinus,コンパス",
            "CMa,Canis Major,おおいぬ",
            "CMi,Canis Minor,こいぬ",
            "Cnc,Cancer,かに",
            "Col,Columba,はと",
            "Com,Coma Berenices,かみのけ",
            "CrA,Corona Australis,みなみのかんむり",
            "CrB,Corona Borealis,かんむり",
            "Crt,Crater,コップ",
            "Cru,Crux,みなみじゅうじ",
            "Crv,Corvus,からす",
            "CVn,Canes Venatici,りょうけん",
            "Cyg,Cygnus,はくちょう",
            "Del,Delphinus,いるか",
            "Dor,Dorado,かじき",
            "Dra,Draco,りゅう",
            "Equ,Equuleus,こうま",
            "Eri,Eridanus,エリダヌス",
            "For,Fornax,ろ",
            "Gem,Gemini,ふたご",
            "Gru,Grus,つる",
            "Her,Hercules,ヘルクレス",
            "Hor,Horologium,とけい",
            "Hya,Hydra,うみへび",
            "Hyi,Hydrus,みずへび",
            "Ind,Indus,インディアン",
            "Lac,Lacerta,とかげ",
            "Leo,Leo,しし",
            "Lep,Lepus,うさぎ",
            "Lib,Libra,てんびん",
            "LMi,Leo Minor,こじし",
            "Lup,Lupus,おおかみ",
            "Lyn,Lynx,やまねこ",
            "Lyr,Lyra,こと",
            "Men,Mensa,テーブルさん",
            "Mic,Microscopium,けんびきょう",
            "Mon,Monoceros,いっかくじゅう",
            "Mus,Musca,はえ",
            "Nor,Norma,じょうぎ",
            "Oct,Octans,はちぶんぎ",
            "Oph,Ophiuchus,へびつかい",
            "Ori,Orion,オリオン",
            "Pav,Pavo,くじゃく",
            "Peg,Pegasus,ペガスス",
            "Per,Perseus,ペルセウス",
            "Phe,Phoenix,ほうおう",
            "Pic,Pictor,がか",
            "PsA,Piscis Austrinus,みなみのうお",
            "Psc,Pisces,うお",
            "Pup,Puppis,とも",
            "Pyx,Pyxis,らしんばん",
            "Ret,Reticulum,レチクル",
            "Scl,Sculptor,ちょうこくしつ",
            "Sco,Scorpius,さそり",
            "Sct,Scutum,たて",
            "Ser,Serpens(Caput),へび(頭)",
            "Ser,Serpens(Cauda),へび(尾)",
            "Sex,Sextans,ろくぶんぎ",
            "Sge,Sagitta,や",
            "Sgr,Sagittarius,いて",
            "Tau,Taurus,おうし",
            "Tel,Telescopium,ぼうえんきょう",
            "TrA,Triangulum Australe,みなみのさんかく",
            "Tri,Triangulum,さんかく",
            "Tuc,Tucana,きょしちょう",
            "UMa,Ursa Major,おおぐま",
            "UMi,Ursa Minor,こぐま",
            "Vel,Vela,ほ",
            "Vir,Virgo,おとめ",
            "Vol,Volans,とびうお",
            "Vul,Vulpecula,こぎつね",
        };

        public double Latitude
        {
            get; set;
        }

        public double Longitude
        {
            get; set;
        }

        /*  Compute the Julian Day for the given date */
        /*  Julian Date is the number of days since noon of Jan 1 4713 B.C. */
        public static double CalcJD(int ny, int nm, int nd, double ut)
        {
            double A, B, C, D, jd, day;

            day = nd + ut / 24.0;
            if ((nm == 1) || (nm == 2))
            {
                ny = ny - 1;
                nm = nm + 12;
            }

            if (((double)ny + nm / 12.0 + day / 365.25) >=
              (1582.0 + 10.0 / 12.0 + 15.0 / 365.25))
            {
                A = ((int)(ny / 100.0));
                B = 2.0 - A + (int)(A / 4.0);
            }
            else
            {
                B = 0.0;
            }

            if (ny < 0.0)
            {
                C = (int)((365.25 * (double)ny) - 0.75);
            }
            else
            {
                C = (int)(365.25 * (double)ny);
            }

            D = (int)(30.6001 * (double)(nm + 1));
            jd = B + C + D + day + 1720994.5;
            return (jd);
        }


        /* Calculate the Julian date with millisecond resolution */

        public static double JDNow()
        {
            double ut, jd;

            /* Use gettimeofday and timeval to capture microseconds */

            /* Copy seconds part to convert to UTC */

            /* Save microseconds part to handle milliseconds of UTC */

            /* Convert unix time now to utc */
            /* Copy values pointed to by g to variables we will use later */

            DateTime t = DateTime.UtcNow;

            /* Convert the microseconds part to milliseconds */
            //milliseconds = usecs/1000;

            /* Calculate floating point ut in hours */
            ut = ((double)t.Millisecond) / 3600000.0 + ((double)t.Second) / 3600.0 + ((double)t.Minute) / 60.0 + ((double)t.Hour);
            jd = CalcJD(t.Year, t.Month, t.Day, ut);

            /* To test for specific jd change this value and uncomment */
            /* jd = 2462088.69;  */

            return (jd);
        }

        /* Local sidereal time */
        /*  Compute Greenwich Mean Sidereal Time (gmst) */
        /*  TU is number of Julian centuries since 2000 January 1.5 */
        /*  Expression for gmst from the Astronomical Almanac Supplement */

        public static double CalcLST(int year, int month, int day, double ut, double glong)
        {
            double TU, TU2, TU3, T0;
            double gmst, lmst;

            TU = (CalcJD(year, month, day, 0.0) - 2451545.0) / 36525.0;
            TU2 = TU * TU;
            TU3 = TU2 * TU;
            T0 =
                (24110.54841 / 3600.0) +
                8640184.812866 / 3600.0 * TU + 0.093104 / 3600.0 * TU2 -
                6.2e-6 / 3600.0 * TU3;
            T0 = Map24(T0);

            gmst = Map24(T0 + ut * 1.002737909);
            lmst = 24.0 * frac((gmst - glong / 15.0) / 24.0);
            return (lmst);
        }
        public double LST(DateTime t)
        {
            double ut, lst;

            ut = ((double)t.Millisecond) / 3600000.0 + ((double)t.Second) / 3600.0 + ((double)t.Minute) / 60.0 + ((double)t.Hour);
            double glong = Longitude;
            lst = CalcLST(t.Year, t.Month, t.Day, ut, glong);
            return (lst);
        }

        /* Calculate the local sidereal time with millisecond resolution */
        /// <summary>
        /// 24 hour format Need Longitude
        /// </summary>
        /// <returns></returns>

        public double LSTNow()
        {
            double ut, lst;
            DateTime t = DateTime.UtcNow;

            ut = ((double)t.Millisecond) / 3600000.0 + ((double)t.Second) / 3600.0 + ((double)t.Minute) / 60.0 + ((double)t.Hour);
            double glong = Longitude;
            lst = CalcLST(t.Year, t.Month, t.Day, ut, glong);
            return (lst);
        }

        public static double LSTNow(double siteLongitude)
        {
            double ut, lst;
            DateTime t = DateTime.UtcNow;

            ut = ((double)t.Millisecond) / 3600000.0 + ((double)t.Second) / 3600.0 + ((double)t.Minute) / 60.0 + ((double)t.Hour);
            lst = CalcLST(t.Year, t.Month, t.Day, ut, siteLongitude);
            return (lst);
        }

        public static bool SizeStringToval(string str, ref double ra, ref double dec,ref byte pa)
        {
            //transrate 
            ra = 0; dec = 0;pa = 0;


            if (str == null) return false;

            float unit = 1f / 60f;
;           if (str.IndexOf('\'') >= 0)
            {
                unit = 1f / 60f;//deg
                str.Replace('\'', ' ');
            }
            else if (str.IndexOf('"') >= 0)
            {
                unit = 1f / 3600f;//deg
                str.Replace('"', ' ');
            }

            string[] wh = str.Split('x');

            for(int i = 0; i < wh.Length; i++)
            {
                wh[i].Trim();
            }
            if (wh.Length ==1)
            {
               double.TryParse( wh[0],out ra);
                dec = ra* unit;
                ra = ra * unit;
            }
            if (wh.Length > 1)
            { //h 有り
                double.TryParse(wh[0], out ra);
                string[] hh = wh[1].Split(' ');

                double.TryParse(hh[0], out dec);
                if (hh.Length > 1)
                {
                    for(int i=1;i<hh.Length;i++)
                    {
                        byte.TryParse(hh[i], out pa);
                    }
                    if (pa > 0) Console.WriteLine("size extra num is " + pa.ToString());
                }

                dec = dec * unit;
                ra = ra * unit;

            }

            return true;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ra"></param>
        /// <param name="dec"></param>
        /// <returns></returns>
        public static bool RaDecStringToEquatorial(string str, ref double ra, ref double dec)
        {
            //transrate 
            if (str == null) return false;

            string[] ss = str.Split(' ');
            if (ss.Length < 6) return false;

            int i = 0;
            bool succ = false;
            while (i + 2 < ss.Length)
            {
                double h, m, s;
                if (!double.TryParse(ss[i++], out h)) continue;
                if (!double.TryParse(ss[i++], out m)) continue;
                if (!double.TryParse(ss[i++], out s)) continue;
                ra = h + m / 60 + s / 3600;
                succ = true;
                break;
            }
            if (succ)
                while (i + 2 < ss.Length)
                {
                    double d, m, s;
                    if (!double.TryParse(ss[i++], out d)) continue;
                    if (!double.TryParse(ss[i++], out m)) continue;
                    if (!double.TryParse(ss[i++], out s)) continue;
                    if (d < 0) dec = d - m / 60 - s / 3600;
                    else dec = d + m / 60 + s / 3600;
                    //success
                    return true;
                }
            return false;
        }

        /// <summary>
        /// need Latitude of Site
        /// </summary>
        /// <param name="ha"></param>
        /// <param name="dec"></param>
        /// <param name="az"></param>
        /// <param name="alt"></param>
        public void EquatorialToHorizontal(double ha, double dec, ref double az, ref double alt)
        {
            double phi, altitude, azimuth;

            ha = ha * Math.PI / 12.0;
            phi = Latitude * Math.PI / 180.0;
            dec = dec * Math.PI / 180.0;
            altitude = Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(ha));
            altitude = altitude * 180.0 / Math.PI;
            azimuth = Math.Atan2(-Math.Cos(dec) * Math.Sin(ha),
              Math.Sin(dec) * Math.Cos(phi) - Math.Sin(phi) * Math.Cos(dec) * Math.Cos(ha));
            azimuth = azimuth * 180.0 / Math.PI;

            azimuth = Map360(azimuth);

            alt = altitude;
            az = azimuth;
        }
        /* Fractional part */
        public static double frac(double x)
        {
            x -= (int)x;
            return ((x < 0) ? x + 1.0 : x);
        }
        /* Map a time in hours to the range  0  to 24 */
        public static double Map24(double hour)
        {
            int n;

            if (hour < 0.0)
            {
                n = (int)(hour / 24.0) - 1;
                return (hour - n * 24.0);
            }
            else if (hour >= 24.0)
            {
                n = (int)(hour / 24.0);
                return (hour - n * 24.0);
            }
            else
            {
                return (hour);
            }
        }
        /* Map an hourangle in hours to  -12 <= ha < +12 */
        public static double Map12(double hour)
        {
            double hour24;

            hour24 = Map24(hour);

            if (hour24 >= 12.0)
            {
                return (hour24 - 24.0);
            }
            else
            {
                return (hour24);
            }
        }

        /* Map an angle in degrees to  0 <= angle < 360 */
        public static double Map360(double angle)
        {
            int n;

            if (angle < 0.0)
            {
                n = (int)(angle / 360.0) - 1;
                return (angle - n * 360.0);
            }
            else if (angle >= 360.0)
            {
                n = (int)(angle / 360.0);
                return (angle - n * 360.0);
            }
            else
            {
                return (angle);
            }
        }

        /* Map an angle in degrees to -180 <= angle < 180 */
        public static double Map180(double angle)
        {
            double angle360;
            angle360 = Map360(angle);

            if (angle360 >= 180.0)
            {
                return (angle360 - 360.0);
            }
            else
            {
                return (angle360);
            }
        }

        /// "00 24 05.2 -72 04 51" if need turn "*" string add 
        /// </summary>
        /// for DEC?
        /// <returns></returns>
        public static string Deg2String(double deg)
        {

            string res = "";

                           //   d = Map360(deg);
       //     bool sign = deg < 0 ;

            double d = Math.Abs(deg);//  
            int dd = (int)(d);
            d -= dd;
            d *= 60.0;
            int dm = (int)(d);
            d -= dm;
            d *= 60.0;
            int ds = (int)(d + 0.5);
            res += deg < 0 ? '-' : '+';
            res += string.Format(@"{0:00} {1:00} {2:00}", dd, dm, ds);
            return res;
        }

        //for RA?
        public static string Deg2HString(double deg)
        {
            string res = "";
            double a = Map24(deg / 15);//  / 15.0;
            int h = (int)(a);
            a -= h;
            a *= 60.0;
            int m = (int)(a);
            a -= m;
            a *= 60.0;
            int s = (int)(a);
            a -= s;
            a *= 100;

            res = string.Format(@"{0:00} {1:00} {2:00}.{3:00} ", h, m, s, (int)a);

            return res;
        }
    }


    /// <summary>
    /// need Cel lib
    /// </summary>
    public class PointCel
    {
        private double ra, dec;
        public Quaternion Quat;

        public static Quaternion ToQuat ( double ra ,double dec)
        {
            float sin = (float)Math.Sin(ra / 12.0 * Math.PI);
            float cos = (float)Math.Cos(ra / 12.0 * Math.PI);
            //x east ra = 18h
            //y ra = 0h
            //z north pole
            float l = (float)Math.Sin((dec - 90) / 180.0 * Math.PI);
            float z = (float)Math.Cos((dec - 90) / 180.0 * Math.PI);
            return  new Quaternion(cos * l, sin * l, z, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raHour">double 24h format</param>
        /// <param name="decDegree">double 360deg format</param>
        public PointCel(double raHour, double decDegree)
        {
            ra = Cel.Map24(raHour);
            dec = Cel.Map360(decDegree);

            Quat = ToQuat(ra, dec);
        }

        /// <summary>
        /// Ra Dec String "HH MM SS.dd +DD mm ss" to PointCel
        /// </summary>
        /// <param name="str">"00 24 05.2 -72 04 51"  </param>
        /// <param name="ra"> hour format 0-23.9999</param>
        /// <param name="dec">degree format 0-359.999</param>
        /// <returns></returns>
        public PointCel(string str)
        {
            //transrate 
            double rh = 0;
            double de = 0;
            if (Cel.RaDecStringToEquatorial(str, ref rh, ref de))
            {//success
                /*
                double altitude2, azimuth2;
                double ha = ra * 15 * Math.PI / 180.0;
                double phi = latitude * Math.PI / 180.0;
                double dec = de * Math.PI / 180.0;

                altitude2 = Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(ha));
                altitude2 = altitude2 * 180.0 / Math.PI;
                azimuth2 = Math.Atan2(-Math.Cos(dec) * Math.Sin(ha),
                                Math.Sin(dec) * Math.Cos(phi) - Math.Sin(phi) * Math.Cos(dec) * Math.Cos(ha));
                azimuth2 = azimuth2 * 180.0 / Math.PI;
                azimuth2 = Map360(azimuth2);

                if (altitude2 > 0)
                {
                    textRaDec.BackColor = Color.White;
                }
                else
                    textRaDec.BackColor = Color.Gray;
                twTarRA.Value = (int)(ra * 3600);
                twTarDec.Value = (int)(de * 3600);
                */
            }
            else
            {
                //Need Throw ?? 
                throw new ArgumentException(
                   "あかん。", "ra dec");
            }

            ra = Cel.Map24(rh);
            dec = Cel.Map360(de);
            Quat = ToQuat(ra, dec);
        }

        public bool Equal(PointCel dst)
        {
            if (Math.Abs(Cel.Map24(ra - dst.Ra)) > 0.000003) return false;
            if (Math.Abs(Cel.Map360(dec - dst.Dec)) > 0.00003) return false;
            return true;
        }

        /// <summary>
        /// Set couple data 
        /// </summary>
        /// <param name="_ra"></param>
        /// <param name="_de"></param>
        public void Set(double _ra, double _de)
        {
            ra = _ra;
            dec = _de;
            Quat = ToQuat(ra, dec);

        }
        public void Set(PointCel pt)
        {
            ra = pt.ra;
            dec = pt.dec;
            Quat = ToQuat(ra, dec);


        }

        /// <summary>
        /// Right Ascension in HourFormat
        /// 0.0000 ... 23.9999
        /// </summary>
        public double Ra
        {
            get
            {
                return ra;
            }
            set
            {
                if (ra != value)
                {
                    ra = Cel.Map24(value);
                    Quat = ToQuat(ra, dec);


                }
            }
        }
        public double RaRad
        {
            get
            {
                return ra / 12.0 * Math.PI;
            }

        }
        /// <summary>
        /// Declination  in degree Format
        /// 0.0000 ... 359.9999
        /// </summary>
        public double Dec
        {
            get
            {
                return dec;
            }
            set
            {
                if (dec != value)
                {
                    dec = Cel.Map360(value);
                    Quat = ToQuat(ra, dec);


                }
            }
        }

        public double RaDeg
        {
            get
            {
                return ra * 15.0;
            }
            set
            {
                if (ra * 15.0 != value)
                {
                    ra = Cel.Map24(value / 15.0);
                    Quat = ToQuat(ra, dec);

                }
            }
        }

        /// <summary>
        /// Same as Dec  Declination  in degree Format
        /// </summary>
        public double DecDeg
        {
            get
            {
                return dec;
            }
            set
            {
                if (dec != value)
                {
                    dec = Cel.Map360(value);
                    Quat = ToQuat(ra, dec);


                }
            }
        }
        public double DecRad
        {
            get
            {
                return dec / 180.0 * Math.PI;
            }

        }

        /// <summary>
        /// "00 24 05.2 -72 04 51" if need turn "*" string add 
        /// </summary>
        /// <returns></returns>
        new public string ToString()
        {
            bool turn = false;
            bool minus = false;
            string res = "";

            double d;//  / 15.0;
            if (dec > 180) d = dec - 360;
            else d = dec;

            if (d < 0)
            {
                d = -d;
                minus = true;
            }
            if (d > 90)
            {
                d = 180 - d;
                turn = true;
            }

            int dd = (int)(d);
            d -= dd;
            d *= 60.0;
            int dm = (int)(d);
            d -= dm;
            d *= 60.0;
            int ds = (int)(d + 0.5);

            double a = ra;//  / 15.0;
            if (turn)
            {
                a += 12.0;
                if (a > 24.0) a -= 24.0;
            }
            int h = (int)(a);
            a -= h;
            a *= 60.0;
            int m = (int)(a);
            a -= m;
            a *= 60.0;
            int s = (int)(a);
            a -= s;
            a *= 100;

            res = string.Format(@"{0:00} {1:00} {2:00}.{3:00} ", h, m, s, (int)a);
            res += minus ? '-' : '+';
            res += string.Format(@"{0:00} {1:00} {2:00}", dd, dm, ds);
            if (turn) res += " *";

            return res;
        }

    }


    /// <summary>
    /// HSV (HSB) カラーを表す
    /// </summary>
    public class HsvColor
    {
        private float _h;
        /// <summary>
        /// 色相 (Hue)
        /// </summary>
        public float H
        {
            get { return this._h; }
        }

        private float _s;
        /// <summary>
        /// 彩度 (Saturation)
        /// </summary>
        public float S
        {
            get { return this._s; }
        }

        private float _v;
        /// <summary>
        /// 明度 (Value, Brightness)
        /// </summary>
        public float V
        {
            get { return this._v; }
            set { this._v = value; }
        }

        private HsvColor(float hue, float saturation, float brightness)
        {
            if (hue < 0f || 360f <= hue)
            {
                throw new ArgumentException(
                    "hueは0以上360未満の値です。", "hue");
            }
            if (saturation < 0f || 1f < saturation)
            {
                throw new ArgumentException(
                    "saturationは0以上1以下の値です。", "saturation");
            }
            if (brightness < 0f || 1f < brightness)
            {
                throw new ArgumentException(
                    "brightnessは0以上1以下の値です。", "brightness");
            }

            this._h = hue;
            this._s = saturation;
            this._v = brightness;
        }

        /// <summary>
        /// 指定したColorからHsvColorを作成する
        /// </summary>
        /// <param name="rgb">Color</param>
        /// <returns>HsvColor</returns>
        public static HsvColor FromRgb(Color rgb)
        {
            float r = (float)rgb.R / 255f;
            float g = (float)rgb.G / 255f;
            float b = (float)rgb.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            float brightness = max;

            float hue, saturation;
            if (max == min)
            {
                //undefined
                hue = 0f;
                saturation = 0f;
            }
            else
            {
                float c = max - min;

                if (max == r)
                {
                    hue = (g - b) / c;
                }
                else if (max == g)
                {
                    hue = (b - r) / c + 2f;
                }
                else
                {
                    hue = (r - g) / c + 4f;
                }
                hue *= 60f;
                if (hue < 0f)
                {
                    hue += 360f;
                }

                saturation = c / max;
            }

            return new HsvColor(hue, saturation, brightness);
        }

        /// <summary>
        /// 指定したHsvColorからColorを作成する
        /// </summary>
        /// <param name="hsv">HsvColor</param>
        /// <returns>Color</returns>
        public static Color ToRgb(HsvColor hsv)
        {
            float v = hsv.V;
            float s = hsv.S;

            float r, g, b;
            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                float h = hsv.H / 60f;
                int i = (int)Math.Floor(h);
                float f = h - i;
                float p = v * (1f - s);
                float q;
                if (i % 2 == 0)
                {
                    //t
                    q = v * (1f - (1f - f) * s);
                }
                else
                {
                    q = v * (1f - f * s);
                }

                switch (i)
                {
                    case 0:
                        r = v;
                        g = q;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = q;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = q;
                        g = p;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                    default:
                        throw new ArgumentException(
                            "色相の値が不正です。", "hsv");
                }
            }

            return Color.FromArgb(
                (int)Math.Round(r * 255f),
                (int)Math.Round(g * 255f),
                (int)Math.Round(b * 255f));
        }

        public static Color cvt(Color c, float mag, float gloss = 0.0f)
        {
            HsvColor hsv = HsvColor.FromRgb(c);
            float b = hsv.V;

            if (mag > -0.5 && mag < 0.5)
            {
                b += mag;
                if (b > 1f) b = 1f;
                if (b < 0) b = 0f;
            }
            else if (mag == 0.5f)
            {
                if (b > 1f) b -= 1f;
                if (b < 0) b += 1f;
            }

            else b = mag;

            b += gloss;
            if (b > 1.0f) b = 1f;
            if (b < 0.0f) b = 0f;
            hsv.V = b;
            return HsvColor.ToRgb(hsv);
        }

    }



    public class Reg
    {
        public static void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Debug.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Debug.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Debug.WriteLine("Error reading app settings");
            }
        }

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                Debug.WriteLine(result);
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                Debug.WriteLine("Error reading app settings");
            }
            return "";
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Debug.WriteLine("Error writing app settings");
            }
        }





    }
}
