using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using ucPgmac;
using System.Drawing.Imaging;
using System.Drawing;

namespace ucPgmac
{
    public class Star
    {
        public long hip = -1;
        string line = "";
        double _RA = 0;
        double _DEC = 0;
        float _GLON = 0; //Galactic longitude
        float _GLAT = 0; //Galactic latitude

        float _Vmag = 0; //Visual magnitude
        float B_V = 0;  //UBV system
        float U_B = 0;
        float R_I = 0;

        double pmRA = 0;
        double pmDE = 0;

        public Color color;

        Quaternion _Quaternion;
        public Star(st_hipinfo star)
        {
            try
            {
                hip = star.XNO;
                _RA = star.SRAD * 12 / Math.PI;
                _DEC = star.SDECO * 180 / Math.PI;

                _Vmag = Math.Abs(star.MAG100) / 100f;

                pmRA = star.XRPM_RA * 180 / Math.PI;
                pmDE = star.XDPM_Dec * 180 / Math.PI;
                //           string spi = "" + star.ISP0 + star.ISP1;

                /*
                スペクトル型 O5  B5        A5       F5      G5      K5      M5
                恒星の色 青      青白      白       黄白    黄      橙      赤
                表面温度 45000K  15000K    8300K	6600K	5600K	4400K	3300K
                */

                switch (star.ISP0)
                {
                    case 'O':
                        color = Color.FromArgb(255,200, 200, 255);// w = (float)(0x44f);// 45000;
                        break;
                    case 'B':
                        color = Color.FromArgb(255, 150, 150, 250);// w = (float)(0x88f);// 15000;
                        break;
                    case 'A':
                        color = Color.FromArgb(255, 200, 200, 200);// w = (float)(0xfff);// 83000;
                        break;
                    case 'F':
                        color = Color.FromArgb(255, 220, 220, 100);// w = (float)(0xff8);// 6600;
                        break;
                    case 'G':
                        color = Color.FromArgb(255, 250, 250, 0);// w = (float)(0x880);// 5600;
                        break;
                    case 'K':
                        color = Color.FromArgb(255, 250, 200, 0);// w = (float)(0xC80);// 4400;
                        break;
                    case 'M':
                        color = Color.FromArgb(255, 255, 0, 0);// w = (float)(0xF44);// 3300;
                        break;
                    default:
                        color = Color.FromArgb(255, 120, 120, 120);// w = (float)(0x888);// 0;
                        break;

                }

                //天の北極を+Zとする春分点を+Yとする
                float sin = (float)Math.Sin(_RA / 12.0 * Math.PI);
                float cos = (float)Math.Cos(_RA / 12.0 * Math.PI);
                //x east ra = 18h
                //y ra = 0h
                //z north pole
                float l = (float)Math.Sin((DEC - 90) / 180.0 * Math.PI);
                float z = (float)Math.Cos((DEC - 90) / 180.0 * Math.PI);
                _Quaternion = new Quaternion(cos * l, sin * l, z, 0);
            }
            catch (Exception e)
            {

            }

        }

            //Create bsc5 data
        public Star(string _line)
        {
            line = _line;
            try
            {
                string RAh = line.Substring(75, 2);                 // 76- 77  I2     h       RAh      ?Hours RA, equinox J2000, epoch 2000.0 (1)
                string RAm = line.Substring(77, 2);                 // 78- 79  I2     min     RAm      ?Minutes RA, equinox J2000, epoch 2000.0 (1)
                string RAs = line.Substring(79, 4);                 // 80- 83  F4.1   s       RAs      ?Seconds RA, equinox J2000, epoch 2000.0 (1)
                string DE_ = line.Substring(83, 1);                 //     84  A1     ---     DE-      ?Sign Dec, equinox J2000, epoch 2000.0 (1)
                string DEd = line.Substring(84, 2);                 // 85- 86  I2     deg     DEd      ?Degrees Dec, equinox J2000, epoch 2000.0 (1)
                string DEm = line.Substring(86, 2);                 // 87- 88  I2     arcmin  DEm      ?Minutes Dec, equinox J2000, epoch 2000.0 (1)
                string DEs = line.Substring(88, 2);                 // 89- 90  I2     arcsec  DEs      ?Seconds Dec, equinox J2000, epoch 2000.0 (1)
                _RA = double.Parse(RAh) + double.Parse(RAm)/60 + double.Parse(RAs)/3600;
                _DEC = (DE_ == "-" ? -1 : 1) * (double.Parse(DEd) + double.Parse(DEm)/60 + double.Parse(DEs)/3600);

                _GLON = float.Parse(line.Substring(90, 6));    // 91- 96  F6.2   deg     GLON     ?Galactic longitude (1)
                _GLAT = float.Parse(line.Substring(96, 6));    // 97-102  F6.2   deg     GLAT     ?Galactic latitude (1)
                _Vmag = float.Parse(line.Substring(102, 5));   //103-107  F5.2   mag     Vmag     ?Visual magnitude (1)
                                                               //n_Vmag = line.Substring(107, 1);             //    108  A1     ---   n_Vmag    *[ HR] Visual magnitude code
                                                               //string u_Vmag = line.Substring(108, 1);             //    109  A1     ---   u_Vmag     [ :?] Uncertainty flag on V
                float.TryParse(line.Substring(109, 5), out B_V);    //110-114  F5.2   mag     B-V      ? B-V color in the UBV system
                                                                    //string u_B_V = line.Substring(114, 1);              //    115  A1     ---   u_B-V      [ :?] Uncertainty flag on B-V
                float.TryParse(line.Substring(115, 5), out U_B);    //116-120  F5.2   mag     U-B      ? U-B color in the UBV system
                                                                    //string u_U_B = line.Substring(120, 1);              //    121  A1     ---   u_U-B      [ :?] Uncertainty flag on U-B
                float.TryParse(line.Substring(121, 5), out R_I);    //122-126  F5.2   mag     R-I      ? R-I   in system specified by n_R-I
                                                                    //string n_R_I = line.Substring(126, 1);              //    127  A1     ---   n_R-I      [CE:?D] Code for R - I system(Cousin, Eggen)

                //Annual proper motion (arcsec/yr)
                pmRA = double.Parse(line.Substring(148, 6));         //149-154  F6.3 arcsec / yr pmRA *? Annual proper motion in RA J2000, FK5 system
                pmDE = double.Parse(line.Substring(154, 6));         //155-160  F6.3 arcsec / yr pmDE ? Annual proper motion in Dec J2000, FK5 system

                float sin = (float)Math.Sin(_RA / 12.0 * Math.PI);
                float cos = (float)Math.Cos(_RA / 12.0 * Math.PI);
                //x east ra = 18h
                //y ra = 0h
                //z north pole
                float l = (float)Math.Sin((DEC-90) / 180.0 * Math.PI);
                float z = (float)Math.Cos((DEC-90) / 180.0 * Math.PI);
                _Quaternion = new Quaternion(cos * l, sin * l, z, 0);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Err " + ex.ToString());
            }
        }

        /// <summary>
        /// Z軸＋ north　Z軸- South
        /// </summary>
        public Quaternion Quaternion
        {
            get
            {
                return _Quaternion;
            }
        }

        public double Vmag
        {
            get { return _Vmag; }
        }
        
        public double RA
        {
            get { return _RA; }
        }

        public double DEC
        {
            get { return _DEC; }
        }

        //Annual proper motion (arcsec/yr)
        public double PmRA
        {
            get { return pmRA; }
        }

        //Annual proper motion (arcsec/yr)
        public double PmDEC
        {
            get { return pmDE; }
        }

        //Galactic longitude
        public float GLON
        {
            get { return _GLON; }
        }
        //Galactic latitude
        public float GLAT
        {
            get { return _GLAT; }
        }

        public string SpectralType
        {
            get
            {
                char n_SpType = line[147];//[evt]
                return line.Substring(127, 20);
            }
        }
        public string Name
        {
            get { return line.Substring(4, 10); }
        }


        //https://en.wikipedia.org/wiki/Dynamical_parallax
        //https://en.wikipedia.org/wiki/Stellar_parallax
        public double Parallax
        {
            get
            {
                char d = line[160];   //    161  A1-- - n_Parallax[D] D indicates a dynamical parallax,  otherwise a trigonometric parallax

                if (d == 'D')
                { //dynamical parallax
                    return double.Parse( line.Substring(161, 5));     //162-166  F5.3   arcsec  Parallax ? Trigonometric parallax(unless n_Parallax)
                }
                else
                {//trigonometric parallax
                    return double.Parse(line.Substring(161, 5));
                }

            }

        }

    }
        /*
            //          1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16        17        18        19        20        21        
            //0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            //  15 21Alp AndBD+28    4    358 73765   1I     94  Alp And  000313.0+283218000823.3+290526111.73-32.84 2.06  -0.11 -0.46 -0.10   B8IVpMnHg         v+0.136-0.163 +.032-012SBO    56  8.5  81.5      *
            //                                               --------------------------------------------------------------------------------
            //                                               Bytes Format Units   Label Explanations
            //                                               --------------------------------------------------------------------------------
            string HR = line.Substring(0, 4);             //  1 - 4  I4-- - HR[1 / 9110] + Harvard Revised Number = Bright Star Number
            string Name = line.Substring(4, 10);          //  5- 14  A10-- - Name     Name, generally Bayer and / or Flamsteed name
            string DM = line.Substring(14, 11);           // 15- 25  A11-- - DM       Durchmusterung Identification(zone in bytes 17 - 19)
            string HD = line.Substring(25, 6);            // 26- 31  I6-- - HD[1 / 225300] ? Henry Draper Catalog Number
            string SAO = line.Substring(31, 6);           // 32- 37  I6-- - SAO[1 / 258997] ? SAO Catalog Number
            string FK5 = line.Substring(37, 4);           // 38- 41  I4-- - FK5 ? FK5 star Number
            string IRflag = line.Substring(41, 1);        //     42  A1-- - IRflag[I] I if infrared source
            string r_IRflag = line.Substring(42, 1);      //     43  A1-- - r_IRflag * [ ':] Coded reference for infrared source
            string Mltpl = line.Substring(43, 1);         //     44  A1     ---    Multiple *[         ] Double or multiple-star code
            string ADS = line.Substring(44, 5);           // 45- 49  A5     ---     ADS      Aitken's Double Star Catalog (ADS) designation
            string ADScomp = line.Substring(49, 2);       // 50- 51  A2     ---     ADScomp  ADS number components
            string VarID = line.Substring(51, 9);         // 52- 60  A9     ---     VarID    Variable star identification
            //string RAh1900 = line.Substring(60, 2);       // 61- 62  I2     h       RAh1900  ?Hours RA, equinox B1900, epoch 1900.0 (1)
            //string RAm1900 = line.Substring(32, 2);       // 63- 64  I2     min     RAm1900  ?Minutes RA, equinox B1900, epoch 1900.0 (1)
            //string RAs1900 = line.Substring(64, 4);       // 65- 68  F4.1   s       RAs1900  ?Seconds RA, equinox B1900, epoch 1900.0 (1)
            //string DE_1900 = line.Substring(68, 1);       //     69  A1     ---     DE-1900  ?Sign Dec, equinox B1900, epoch 1900.0 (1)
            //string DEd1900 = line.Substring(69, 2);       // 70- 71  I2     deg     DEd1900  ?Degrees Dec, equinox B1900, epoch 1900.0 (1)
            //string DEm1900 = line.Substring(71, 2);       // 72- 73  I2     arcmin  DEm1900  ?Minutes Dec, equinox B1900, epoch 1900.0 (1)
            //string DEs1900 = line.Substring(73, 2);       // 74- 75  I2     arcsec  DEs1900  ?Seconds Dec, equinox B1900, epoch 1900.0 (1)
            string RAh = line.Substring(75, 2);           // 76- 77  I2     h       RAh      ?Hours RA, equinox J2000, epoch 2000.0 (1)
            string RAm = line.Substring(77, 2);           // 78- 79  I2     min     RAm      ?Minutes RA, equinox J2000, epoch 2000.0 (1)
            string RAs = line.Substring(79, 4);           // 80- 83  F4.1   s       RAs      ?Seconds RA, equinox J2000, epoch 2000.0 (1)
            string DE_ = line.Substring(83, 1);           //     84  A1     ---     DE-      ?Sign Dec, equinox J2000, epoch 2000.0 (1)
            string DEd = line.Substring(84, 2);           // 85- 86  I2     deg     DEd      ?Degrees Dec, equinox J2000, epoch 2000.0 (1)
            string DEm = line.Substring(86, 2);           // 87- 88  I2     arcmin  DEm      ?Minutes Dec, equinox J2000, epoch 2000.0 (1)
            string DEs = line.Substring(88, 2);           // 89- 90  I2     arcsec  DEs      ?Seconds Dec, equinox J2000, epoch 2000.0 (1)
            string GLON = line.Substring(90, 6);          // 91- 96  F6.2   deg     GLON     ?Galactic longitude (1)
            string GLAT = line.Substring(96, 6);          // 97-102  F6.2   deg     GLAT     ?Galactic latitude (1)
            string Vmag = line.Substring(102, 5);         //103-107  F5.2   mag     Vmag     ?Visual magnitude (1)
            string n_Vmag = line.Substring(107, 1);       //    108  A1     ---   n_Vmag    *[ HR] Visual magnitude code
            string u_Vmag = line.Substring(108, 1);       //    109  A1     ---   u_Vmag     [ :?] Uncertainty flag on V
            string B_V = line.Substring(109, 5);          //110-114  F5.2   mag     B-V      ? B-V color in the UBV system
            string u_B_V = line.Substring(114, 1);        //    115  A1     ---   u_B-V      [ :?] Uncertainty flag on B-V
            string U_B = line.Substring(115, 5);          //116-120  F5.2   mag     U-B      ? U-B color in the UBV system
            string u_U_B = line.Substring(120, 1);        //    121  A1     ---   u_U-B      [ :?] Uncertainty flag on U-B
            string R_I = line.Substring(121, 5);          //122-126  F5.2   mag     R-I      ? R-I   in system specified by n_R-I
            string n_R_I = line.Substring(126, 1);        //    127  A1     ---   n_R-I      [CE:?D] Code for R - I system(Cousin, Eggen)
            string SpType = line.Substring(127, 20);      //128-147  A20-- - SpType   Spectral type
            string n_SpType = line.Substring(147, 1);     //    148  A1-- - n_SpType[evt] Spectral type code
            string pmRA = line.Substring(148, 6);         //149-154  F6.3 arcsec / yr pmRA *? Annual proper motion in RA J2000, FK5 system
            string pmDE = line.Substring(154, 6);         //155-160  F6.3 arcsec / yr pmDE ? Annual proper motion in Dec J2000, FK5 system
            string n_Parallax = line.Substring(160, 1);   //    161  A1-- - n_Parallax[D] D indicates a dynamical parallax,  otherwise a trigonometric parallax
            string Parallax = line.Substring(161, 5);     //162-166  F5.3   arcsec  Parallax ? Trigonometric parallax(unless n_Parallax)
            string RadVel = line.Substring(166, 4);       //167-170  I4     km / s    RadVel ? Heliocentric Radial Velocity
            string n_RadVel = line.Substring(170, 4);     //171-174  A4-- - n_RadVel * [V ? SB123O ] Radial velocity comments
            string I_RotVel = line.Substring(174, 2);     //175-176  A2-- - l_RotVel[<=> ] Rotational velocity limit characters
            string RotVel = line.Substring(176, 3);       //177-179  I3     km / s    RotVel ? Rotational velocity, v sin i
            string u_RotVel = line.Substring(179, 1);     //    180  A1-- - u_RotVel[ :v] uncertainty and variability flag on  RotVel
            string Dmag = line.Substring(180, 4);         //181-184  F4.1   mag     Dmag ? Magnitude difference of double,  or brightest multiple
            string Sep = line.Substring(184, 6);          //185-190  F6.1   arcsec  Sep ? Separation of components in Dmag  if occultation binary.
            string MultID = line.Substring(190, 4);       //191-194  A4-- - MultID   Identifications of components in Dmag
            string MultCnt = line.Substring(194, 2);      //195-196  I2-- - MultCnt ? Number of components assigned to a multiple
            string NoteFlag = line.Substring(196, 1);     //    197  A1-- - NoteFlag[*] a star indicates that there is a note   (see file notes)

         */

    
}
