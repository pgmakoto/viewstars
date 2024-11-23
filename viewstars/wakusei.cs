using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ucPgmac
{
    // http://stjarnhimlen.se/comp/ppcomp.html#0
    //を参考に作成
    //  makoto kuwabara

    //惑星　パラメーター保存用構造
    public struct orbital_params
    {

        double N_param;
        double N_paramD;
        double i_param;
        double i_paramD;
        double w_param;
        double w_paramD;
        double a_param;
        double a_paramD;
        double e_param;
        double e_paramD;
        double M_param;
        double M_paramD;
        double d0_param;
        double d0_paramD;

        public orbital_params(double N
                              , double N_D
                              , double i
                              , double i_D
                              , double w
                              , double w_D
                              , double a
                              , double a_D
                              , double e
                              , double e_D
                              , double M
                              , double M_D
                              , double d0
                              , double d0_D

            )
        {
            N_param = N;
            N_paramD = N_D;
            i_param = i;
            i_paramD = i_D;
            w_param = w;
            w_paramD = w_D;
            a_param = a;
            a_paramD = a_D;
            e_param = e;
            e_paramD = e_D;
            M_param = M;
            M_paramD = M_D;
            d0_param = d0;
            d0_paramD = d0_D;
        }

        public double N(double jd)
        {
            return (N_param + N_paramD * jd) * Math.PI / 180;
        }
        public double i(double jd)
        {
            return (i_param + i_paramD * jd)*Math.PI / 180;
        }
        public double w(double jd)
        {
            return (w_param + w_paramD * jd) * Math.PI / 180;
        }
        public double a(double jd)
        {
            return a_param + a_paramD * jd;
        }
        public double e(double jd)
        {
            return e_param + e_paramD * jd;
        }
        public double M(double jd)
        {
            return (M_param + M_paramD * jd) * Math.PI / 180;
        }
        public double d0()
        {
            return d0_param;
        }
        public double d0p()
        {
            return d0_paramD;
        }

    }


    public class wakusei
    {
        #region params
        static orbital_params Sun_param = new orbital_params(
            0.0, 0
           , 0.0, 0
           , 282.9404, 4.70935E-5
           , 1.000000, 0
           , 0.016709, -1.151E-9
           , 356.0470, 0.9856002585
            , 1919.26, 1919.26
        );
        static orbital_params Moon_param = new orbital_params(
            125.1228, -0.0529538083
            , 5.1454, 0
            , 318.0634, 0.1643573223
            , 60.2666, 0 //(Earth radii)
            , 0.054900, 0
            , 115.3654, 13.0649929509
             , 1873.7, 1873.7
        );
        static orbital_params Mercury_param = new orbital_params(
             48.3313, 3.24587E-5
            , 7.0047, 5.00E-8
            , 29.1241, 1.01444E-5
            , 0.387098, 0//(AU)
            , 0.205635, 5.59E-10
            , 168.6562, 4.0923344368
             , 6.74, 6.74
           );
        static orbital_params Venus_param = new orbital_params(
             76.6799, 2.46590E-5
            , 3.3946, 2.75E-8
            , 54.8910, 1.38374E-5
            , 0.723330, 0//(AU)
            , 0.006773, -1.302E-9
            , 48.0052, 1.6021302244
             , 16.92, 16.92
           );
        static orbital_params Mars_param = new orbital_params(
              49.5574, 2.11081E-5
            , 1.8497, -1.78E-8
            , 286.5016, 2.92961E-5
            , 1.523688, 0//(AU)
            , 0.093405, 2.516E-9
            , 18.6021, 0.5240207766
             , 9.36, 9.28
           );
        static orbital_params Jupiter_param = new orbital_params(
             100.4542, 2.76854E-5
            , 1.3030, -1.557E-7
            , 273.8777, 1.64505E-5
            , 5.20256, 0//(AU)
            , 0.048498, 4.469E-9
            , 19.8950, 0.0830853001
             , 196.94, 185.08
           );
        static orbital_params Saturn_param = new orbital_params(
              113.6634, 2.38980E-5
            , 2.4886, -1.081E-7
            , 339.3939, 2.97661E-5
            , 9.55475, 0//(AU)
            , 0.055546, -9.499E-9
            , 316.9670, 0.0334442282
             , 165.6, 150.8
           );
        static orbital_params Uranus_param = new orbital_params(
               74.0005, 1.3978E-5
            , 0.7733, 1.9E-8
            , 96.6612, 3.0565E-5
            , 19.18171, -1.55E-8
            , 0.047318, 7.45E-9
            , 142.5905, 0.011725806
             , 65.8, 62.1
        );
        static orbital_params Neptune_param = new orbital_params(
             131.7806, 3.0173E-5
            , 1.7700, -2.55E-7
            , 272.8461, -6.027E-6
            , 30.05826, 3.313E-8
            , 0.008606, 2.15E-9
            , 260.2471, 0.005995147
             , 62.2, 60.9
           );
        #endregion

        //原文　   http://stjarnhimlen.se/comp/ppcomp.html#0
        //    0. 序文
        //       以下は、一連の軌道要素から太陽と月、主要な惑星、および彗星と小惑星の位置を計算する方法の説明です。
        //
        //       アルゴリズムは可能な限り簡略化されていますが、それでもかなりの精度が維持されています。計算された位置の精度は、太陽と内惑星の場合は
        //       1 分角未満、外惑星の場合は約 1 分角、月の場合は 1 ～ 2 分角です。精度の要求をこのレベルに制限すると、たとえば平均位置、真位置、
        //       見かけ位置の差を無視するなどして、さらに簡略化できます。
        //
        //       以下で計算された位置は「その日の春分」のもので、日の出/日の入り時刻を計算するのに適していますが、固定された時代について描かれた星図
        //       に位置をプロットするには適していません。後者の場合、歳差運動の補正を適用する必要があり、これは黄道に沿った回転として最も簡単に実行されます。
        //
        //       これらのアルゴリズムは、1980 年に Astrophysical Journal Supplement Series に掲載された T.van Flandern と K.Pulkkinen の論文
        //       「惑星の位置に関する低精度の公式」のプレプリントに基づいて、1979 年に私が開発しました。これは基本的に、妥当な精度を保ちながらこれら
        //       のアルゴリズムを簡略化したものです。これらは 1979 年に HP-41C のプログラム可能なポケット計算機に初めて実装され、
        //       2 KB 未満の RAM で実行されました。もちろん、今日では、より精度の高いアルゴリズムや、より強力なコンピューターが利用可能です。
        //       それでも、私はこれらのアルゴリズムを、1 ～ 2 分の精度で太陽/月の位置を計算する最も簡単な方法だと考えて保持しています。
        //
        //     1. はじめに
        //       以下のテキストでは、太陽、月、海王星までの主要な惑星の天空での位置を計算する方法について説明します。冥王星のアルゴリズムは、JPL
        //       で数値積分によって計算された冥王星の位置に対するフーリエ フィットから取得されます。軌道要素が利用できる場合は、他の天体 (つまり、
        //       彗星や小惑星) の位置も計算できます。
        //
        //       これらの式は複雑に思えるかもしれませんが、これは約 1 分角(= 1/60 度) というかなり良好な精度で惑星の位置を計算する最も簡単な方法
        //       だと思います。さらに単純化すると精度は低下しますが、もちろんアプリケーションによっては問題ない場合もあります。
        //
        //     2. 正確性について
        //       精度要件は控えめで、最終位置の誤差は 1 ～ 2 分角(1 分角 = 1 / 60 度) 以内です。この精度は、ある意味では極めて最適です。つまり、
        //       多くの簡略化を行える一方で、目指せる最高の精度です。ここで行われる簡略化は次のとおりです。1
        //
        //       : 章動と行角は両方とも無視されます。2
        //       : 惑星行角(光の移動時間) は無視されます。3
        //       : 地球時刻/エフェメリス時刻(TT/ET) と世界時刻(UT) の差は無視されます。4
        //       : 歳差は、黄道経度への単純な加算という簡略化された方法で計算されます。5
        //       : 惑星軌道要素の高次項は無視されます。これにより、1000 年後には最大 2 分角の誤差が生じます。月の場合、誤差はさらに大きくなり
        //       、1000 年後には 7 分角になります。この誤差は、現在からの時間の 2 乗に比例して大きくなります。
        //       6: 惑星の摂動のほとんどは無視されます。月、木星、土星、天王星の主な摂動項のみが含まれます。さらに低い精度が許容される場合は、
        //       これらの摂動も無視できます。
        //       7: 最大の天王星-海王星摂動は、これらの惑星の軌道要素で考慮されます。したがって、天王星と海王星の軌道要素は、特に遠い過去と
        //       未来では精度が低くなります。したがって、これらの惑星の要素は、せいぜい過去と未来の数世紀にのみ使用する必要があります。
        //
        //      3. 時間スケール
        //       これらの数式における時間スケールは日数で計算されます。時間、分、秒は 1 日の小数として表されます。
        //       0.0 日目は 2000 年 1 月 0.0 UT(または 1999 年 12 月 31 日 0:00 UT) です。この「日数」d は次のように計算されます
        //       (y= 年、m= 月、D= 日、UT= 時間 + 小数での UT):
        //           d = 367*y - 7 * (y + (m+9)/12 ) / 4 + 275*m/9 + D - 730530


        //       上記の式は 1900 年 3 月から 2100 年 2 月までのみ有効であることに注意してください。
        //       以下は、グレゴリオ暦全体に有効な別の式です。
        //           d = 367*y - 7 * (y + (m+9)/12 ) / 4 - 3 * ((y + (m-9)/7 ) / 100 + 1 ) / 4 + 275*m/9 + D - 730515

        public double calc_jd(DateTime gd)
        {
            int y = gd.Year;
            int m = gd.Month;
            int D = gd.Day;
            DateTime utc = gd.AddHours(-9);

            double h = utc.TimeOfDay.TotalDays;
            return h + 367 * y - 7 * (y + (m + 9) / 12) / 4 - 3 * ((y + (m - 9) / 7) / 100 + 1) / 4 + 275 * m / 9 + D - 730515;
        }

        //       ここでの除算はすべて整数除算である必要があることに注意してください。Pascal では、「/」の代わりに「div」を使用し、MS-Basic では、
        //       「/」の代わりに「\」を使用します。Fortran、C、C++ では、y と m の両方が整数の場合、「/」を使用できます。最後に、次のコードを追加して
        //       時刻を含めます。
        //           d = d + UT/24.0         (これは浮動小数点除算です)

        //     4. 軌道要素
        //       主要な軌道要素は次のように表されます。
        public class orbital_elements
        {
            static double  d2rad = Math.PI / 180.0;
            public static double jd;

            public static double xs, ys,rs;
            public static double ecl;

            orbital_params param;

            public double r;    //太陽までの距離 r[AU] 　地球ー太陽はrs
            public double lonsun;

            public double xh, yh, zh; //惑星の座標(太陽中心)
            public double xg, yg, zg , rg; //地心位置 地心距離

            public double d;//見かけの直径

            double xe;//直交赤道座標
            double ye;
            double ze;

            public double RA, Dec;//[Rad]

            public orbital_elements(orbital_params _param)
            {
                this.param = _param;
            }
            public double N;//= longitude of the ascending node  昇交点の経度
            public double i;//= inclination to the ecliptic(plane of the Earth's orbit)      黄道（地球の軌道面）に対する傾斜角
            public double w;//= argument of perihelion                                       近日点引数
            public double a;//= semi - major axis, or mean distance from Sun                 半長径、または太陽からの平均距離
            public double e;//= eccentricity(0 = circle, 0 - 1 = ellipse, 1 = parabola)      離心率(0 = 円、0 - 1 = 楕円、1 = 放物線)
            public double M;//= mean anomaly (0 at perihelion; increases uniformly with time)平均日点角（近日点では 0、時間とともに均一に増加

            //       関連する軌道要素は次のとおりです。
            public double w1;   // = N + w   = longitude of perihelion                                      //近日点の経度
            public double L;    // = M + w1  = mean longitude                                               //平均経度
            public double q;    // = a* (1-e) = perihelion distance                                         //近日点距離
            public double Q;    // = a* (1+e) = aphelion distance                                           //遠日点距離
            public double P;    // = a ^ 1.5 = orbital period(years if a is in AU, astronomical units)      //軌道周期（a が AU の場合は年、天文単位
            public double T;    // = Epoch_of_M - (M(deg)/360_deg) / P  = time of perihelion                //Epoch_of_M - (M (deg) /360_deg) / P = 近日点の時間
            public double v;    //= true anomaly(angle between position and perihelion)                     //真近点角（位置と近日点の間の角度）
            public double E;    // = eccentric anomaly                                                      //離心近点角

            public void update()
            {
                N = param.N(jd);//昇交点の経度
                i = param.i(jd);//黄道（地球の軌道面）に対する傾斜角
                w = param.w(jd);//近日点引数
                a = param.a(jd);//(AU)
                e = param.e(jd);//離心率(0 = 円、0 - 1 = 楕円、1 = 放物線)
                M = param.M(jd);//平均日点角（近日点では 0、時間とともに均一に増加

                w1 = N + M;//近日点の経度
                L = M + w1;//平均経度
                q = a * (1 - e);//近日点距離
                Q = a * (1 + e);//遠日点距離
                P = Math.Sqrt(a * a * a);////軌道周期（a が AU の場合は年、天文単位
                T = 2000 - (M / Math.PI / 2) / P;//????????????????????????????????????
                
            }

            /// <summary>
            /// eccentric anomaly // E  離心近点角
            /// </summary>
            public double 離心近点角
            {
                get
                {
                    //e is rad.          if degree * (180.0 / Math.PI)
                    return M + e * Math.Sin(M) * (1.0 + e * Math.Cos(M));
                }
            }

            //7.ichi
            public void Vector()//out double xh, out double yh, out double zh)
            {
                //       3次元空間における惑星の位置を計算します。
                xh = r * (Math.Cos(N) * Math.Cos(v + w) - Math.Sin(N) * Math.Sin(v + w) * Math.Cos(i));
                yh = r * (Math.Sin(N) * Math.Cos(v + w) + Math.Cos(N) * Math.Sin(v + w) * Math.Cos(i));
                zh = r * (Math.Sin(v + w) * Math.Sin(i));
            }

            public void sunlocation()
            {
                E = 離心近点角;
                double xv = Math.Cos(E) - e;                       //= a * Math.Cos(v)
                double yv = Math.Sqrt(1.0 - e * e) * Math.Sin(E);  //= a * Math.Sin(v)

                v = Math.Atan2(yv, xv);             //真の近点角 v
                r = Math.Sqrt(xv * xv + yv * yv);   //太陽までの距離 r rsとして使用
                rs = r;
                // 次に、太陽の真の経度を計算します。
                double lonsun = v + w;
                xs = r * Math.Cos(lonsun);//xs、ys は黄道面の座標系における太陽の位置
                ys = r * Math.Sin(lonsun);

                //赤道直角地心座標変換
                double xe = xs;
                double ye = ys * Math.Cos(ecl);
                double ze = ys * Math.Sin(ecl);

                RA = Math.Atan2(ye, xe);
                Dec = Math.Atan2(ze, Math.Sqrt(xe * xe + ye * ye));
                //////////////////////// Taiyo
                
                d  = param.d0() / r;

            }
            public void planetlocation()
            {
                //    6. 月と惑星の位置
                //       ケプラーの方程式を解かなければなりません
                //           M = e * sin(E) - E
                //       ここで、平均日点角 M と離心率 e がわかっており、離心近点角 E を求めます。
                //       まず、E の第一近似値を計算してみましょう。
                //           E = M + e * sin(M) * ( 1.0 + e * cos(M) )

                //       ここで、E と M はラジアン単位です。E と M を度単位で表したい場合は、次のように 180/pi の係数を挿入する必要があります。
                //           E = M + e*(180/pi) * sin(M) * ( 1.0 + e * cos(M) )

                E = 離心近点角;
                if (e > 0.05)
                {
                    //       離心率 e が約 0.05 ～ 0.06 未満の場合、この近似値は十分に正確です。離心率がこれより大きい場合は、E0 = E に設定し、
                    //       次の反復式を使用します (E と M は度単位)。
                    //           E1 = E0 - ( E0 - e*(180/pi) * sin(E0) - M ) / ( 1 - e * cos(E0) )
                    //       または（EとMはラジアン単位）:
                    //           E1 = E0 - ( E0 - e * sin(E0) - M ) / ( 1 - e * cos(E0) )
                    double E0 = E;
                    double E1 = E;
                    for (int cnt = 0; cnt < 5; cnt++)
                    {
                        E1 = E0 - (E0 - e * Math.Sin(E0) - M) / (1 - e * Math.Cos(E0));
                        if (Math.Abs(E1 - E0) < 0.001) break;
                    }
                    if (Math.Abs(E1 - E0) < 0.001)
                    { //近放物線軌道または放物線 軌道 の式を使用する必要があります
                        Console.WriteLine("planet error " + (Math.Abs(E1 - E0)).ToString());
                    }
                    E = E1;
                    // 新しい反復ごとに、E0 を E1 に置き換えます。E0 と E1 が十分に近くなるまで (約 0.001 度) 反復します。離心率が 1 に近い彗星の軌道の場合、
                    // 1E-4 度または 1E-5 度未満の差が必要です。この反復式が収束しない場合は、離心率が 1 に近すぎる可能性があります。その場合は、代わりに
                    // 近放物線軌道または放物線 軌道 の式を使用する必要があります。
                }

                // 次に、惑星の距離と真の近点角を計算します。
                //     xv = r * cos(v) = a * ( cos(E) - e )
                //     yv = r * sin(v) = a * ( sqrt(1.0 - e*e) * sin(E) )
                //
                //     v = atan2(yv,xv) 
                //     r = sqrt( xv*xv + yv*yv )

                double xv = a*(Math.Cos(E) - e);                       //= r * Math.Cos(v)
                double yv = a*(Math.Sqrt(1.0 - e * e) * Math.Sin(E));  //= r * Math.Sin(v)

                v = Math.Atan2(yv, xv);             //真の近点角 v
                r = Math.Sqrt(xv * xv + yv * yv);   //惑星の距離 r rsとして使用
                //////////////// planets
                // 7. 空間における位置
                // 3次元空間における惑星の位置を計算します。
                //    xh = r * ( cos(N) * cos(v+w) - sin(N) * sin(v+w) * cos(i) )
                //    yh = r * ( sin(N) * cos(v+w) + cos(N) * sin(v+w) * cos(i) )
                //    zh = r * ( sin(v+w) * sin(i) )
                xh = r * (Math.Cos(N) * Math.Cos(v + w) - Math.Sin(N) * Math.Sin(v + w) * Math.Cos(i));
                yh = r * (Math.Sin(N) * Math.Cos(v + w) + Math.Cos(N) * Math.Sin(v + w) * Math.Cos(i));
                zh = r * (Math.Sin(v + w) * Math.Sin(i));

                //（摂動を補正する場合、または位置を標準の時代に合わせて歳差移動させる場合は、これを行う必要があります）。
                //  lonecl = atan2( yh, xh )
                //  latecl = atan2(zh, sqrt(xh * xh + yh * yh))
                double lonecl = Math.Atan2(yh, xh);
                double latecl = Math.Atan2(zh, Math.Sqrt(xh * xh + yh * yh));

                //   double ye = ys * Math.Cos(ecl);
                //   double ze = ys * Math.Sin(ecl);

                //       確認として、sqrt(xh*xh+yh*yh+zh*zh) を計算することができます。もちろん、これは r と等しくなるはずです (小さな丸め誤差を除く)。
                double R = Math.Sqrt(xh * xh + yh * yh + zh * zh);
                if (Math.Abs(r - R) > 0.001)
                {
                    Console.Write("kevlar:" + r.ToString() + " " + R.ToString());
                }

                ///////////////////////////////////////////////

                //     8. Precession 歳差運動
                //        1950.0 や 2000.0 などの標準的な時代の惑星の位置を計算したい場合 (たとえば、星図に位置をプロットできるようにするため)、lonecl に以下の補正を追加する
                //        必要があります。月ではなく惑星の位置を計算する場合は、同じ補正を太陽の経度 lonsun にも追加する必要があります。必要な時代は、分数を含む年として表されます。
                //            lon_corr = 3.82394E-5 * ( 365.2422 * ( Epoch - 2000.0 ) - d )
                double Epoch = 2000;
                double lon_corr = 3.82394E-5 * (365.2422 * (Epoch - 2000.0) - jd) * d2rad;
                //        今日のエポックの位置を知りたい場合（日の出/日の入りの時間などを計算するときに便利です）、修正する必要はありません。

            }

            public void EarthCenter(double lon =0,double lat =0)
            {
                //     11. 地球中心座標
                //        これで、惑星の太陽中心座標が計算され、最も重要な摂動が組み込まれました。次に、地球中心座標を計算します。
                //        摂動された lonecl、latecl、r を  (摂動された)xh、yh、zh に変換する必要があります。
                //            xh = r * cos(lonecl) * cos(latecl)
                //            yh = r * sin(lonecl) * cos(latecl)
                //            zh = r * sin(latecl)

                //        月の位置を計算する場合、これはすでに地心位置なので、xg=xh、yg=yh、zg=zh と設定するだけです。
                //        まだの場合は、太陽の位置も計算する必要があります。
                //        lonsun、rs (rs はSunlocationで計算された r です) を xs、ys に変換します。

                //            xs = rs * cos(lonsun)//Sunlocationですでに計算済み
                //            ys = rs * sin(lonsun)

                //        (もちろん、 xh、yh、zh および xs、ys に変換する前に、 loneclおよびlonsunに歳差補正を追加する必要があります)。
                //        次に、太陽中心の位置から地心位置に変換します。
                //            xg = xh + xs
                //            yg = yh + ys
                //            zg = zh
                double lonecl = lon + Math.Atan2(yh, xh); //<<惑星毎の摂動を変換している？　//太陽の摂動を??
                double latecl = lat + Math.Atan2(zh, Math.Sqrt(xh * xh + yh * yh));

                double _xh = r * Math.Cos(lonecl) * Math.Cos(latecl);
                double _yh = r * Math.Sin(lonecl) * Math.Cos(latecl);
                double _zh = r * Math.Sin(latecl);

                xg = _xh + xs;
                yg = _yh + ys;
                zg = _zh;
                //    これで、地球を中心とする惑星の地心位置が、直交黄道座標でわかるようになりました。
                //
                // 12. 赤道座標
                //    直交黄道座標を直交赤道座標に変換してみましょう。yz 平面を黄道傾斜角 ecl だけ回転させるだけです。
                //        xe = xg
                //        ye = yg * cos(ecl) - zg * sin(ecl)
                //        ze = yg * sin(ecl) + zg * cos(ecl)

                xe = xg;
                ye = yg * Math.Cos(ecl) - zg * Math.Sin(ecl);
                ze = yg * Math.Sin(ecl) + zg * Math.Cos(ecl);

                RA = Math.Atan2(ye, xe);
                Dec = Math.Atan2(ze, Math.Sqrt(xe * xe + ye * ye));
                //    最後に、惑星の赤経 (RA) と赤緯 (Dec) を計算します。
                //        RA = atan2( ye, xe )
                //        Dec = atan2( ze, sqrt(xe*xe+ye*ye) )

                //    地心距離を計算します。
                //        rg = sqrt(xg*xg+yg*yg+zg*zg) = sqrt(xe*xe+ye*ye+ze*ze)
                //    これで赤道座標の計算が完了します。
                rg =  Math.Sqrt(xg * xg + yg * yg + zg * zg);


                //15.惑星の離角と物理的な暦
                //       まずは惑星の見かけの直径を計算してみましょう。
                //            d = d0 / R
                d = param.d0() / rg;
                //        1天文単位での太陽の見かけの直径は1919.26" です。月の見かけの直径は次のとおりです。
                //            d = 1873.7" * 60 / r

                //        ここで、r は地球の半径における月の距離です。

            }

            public void LocForMoon(double lon = 0, double lat = 0)
            {
                //     11. 地球中心座標
                //        これで、惑星の太陽中心座標が計算され、最も重要な摂動が組み込まれました。次に、地球中心座標を計算します。
                //        摂動された lonecl、latecl、r を  (摂動された)xh、yh、zh に変換する必要があります。
                //            xh = r * cos(lonecl) * cos(latecl)
                //            yh = r * sin(lonecl) * cos(latecl)
                //            zh = r * sin(latecl)
                double lonecl = lon + Math.Atan2(yh, xh); //<<惑星毎の摂動を変換している？　//太陽の摂動を??
                double latecl = lat + Math.Atan2(zh, Math.Sqrt(xh * xh + yh * yh));

                double _xh =xg= r * Math.Cos(lonecl) * Math.Cos(latecl);
                double _yh =yg = r * Math.Sin(lonecl) * Math.Cos(latecl);
                double _zh =zg = r * Math.Sin(latecl);

                //        月の位置を計算する場合、これはすでに地心位置なので、xg=xh、yg=yh、zg=zh と設定するだけです。
                //        まだの場合は、太陽の位置も計算する必要があります。
                //        lonsun、rs (rs はSunlocationで計算された r です) を xs、ys に変換します。

                //            xs = rs * cos(lonsun)//Sunlocationですでに計算済み
                //            ys = rs * sin(lonsun)

                //        (もちろん、 xh、yh、zh および xs、ys に変換する前に、 loneclおよびlonsunに歳差補正を追加する必要があります)。
                //        次に、太陽中心の位置から地心位置に変換します。
                //            xg = xh + xs
                //            yg = yh + ys
                //            zg = zh
                xe =  xh;
                ye =  yh * Math.Cos(ecl) - zh * Math.Sin(ecl);
                ze =  yh * Math.Sin(ecl) + zh * Math.Cos(ecl);

                RA = Math.Atan2(ye, xe);
                Dec = Math.Atan2(ze, Math.Sqrt(xe * xe + ye * ye));

                //    地心距離を計算します。
                //        rg = sqrt(xg*xg+yg*yg+zg*zg) = sqrt(xe*xe+ye*ye+ze*ze)
                //    これで赤道座標の計算が完了します。
                rg = Math.Sqrt(xh * xh + yh * yh + zh * zh);


                //15.惑星の離角と物理的な暦
                //       まずは惑星の見かけの直径を計算してみましょう。
                //            d = d0 / R
                d = param.d0()  *60/ rg;
                //        1天文単位での太陽の見かけの直径は1919.26" です。月の見かけの直径は次のとおりです。
                //            d = 1873.7" * 60 / r

                //        ここで、r は地球の半径における月の距離です。

            }
        }


        //4続き---
        //       1天文単位（AU）は、地球から太陽までの平均距離で、1億4,960万kmです。惑星は、太陽に最も近いとき 近日点にあり、
        //       太陽から最も遠いとき遠日点にあります 。月、人工衛星、または地球を周回するその他の天体については、
        //       地球から最も遠い軌道上の点を近地点、 最も遠い軌道上の点を遠地点と呼びます 。 perigee and apogee instead
        //
        //       軌道上の位置を表すために、平均近点角、真近点角、および離心率近点角という3つの角度を使用します。
        //                                  Mean Anomaly, True Anomaly, and Eccentric Anomaly
        //       惑星が近日点にあるときは、 これらはすべてゼロになります。
        //       平均近点角（M）：この角度は時間の経過とともに均一に増加し、1公転周期につき360度ずつ増加します。近日点ではゼロです。
        //       公転周期と前回の近日点からの時間から簡単に計算できます。
        //       真近点角(v) : これは、中心天体(この場合は太陽) から見た、惑星と近点間の実際の角度です。時間とともに不均一に増加し、
        //       近点で最も急速に変化します。
        //       偏心近点角(E) : これは、平均近点角と軌道偏心率から真近点角を計算するときに、ケプラーの方程式で使用される補助角度です。
        //       円軌道(偏心率 = 0) の場合、これら 3 つの角度はすべて互いに等しいことに注意してください。
        //
        //       必要なもう 1 つの量は、ecl、つまり黄道の傾斜、つまり地球の回転軸の「傾き」です(現在は 23.4 度で、徐々に減少しています)。まず、関心の
        //       ある瞬間の「d」を計算します(セクション 3 )。次に、黄道の傾斜を計算します。
        //           ecl = 23.4393 - 3.563E-7 * d

        //       次に、対象の惑星の軌道要素を計算します。太陽または月の位置が必要な場合は、太陽または月の軌道要素を計算するだけで済みます。
        //       他の惑星の位置が必要な場合は、その惑星と太陽の軌道要素を計算する必要があります(もちろん、太陽の軌道要素は実際には地球の軌道要素ですが、
        //       ここでは太陽が地球を周回していると仮定するのが通例です)。これは、惑星の地心位置を計算するために必要です。
        //
        //       半長軸 a は、月の場合は地球半径で与えられますが、太陽とすべての惑星の場合は天文単位で与えられることに注意してください。M を
        //       計算する場合 (および月の場合は N と w を計算する場合)、360 度より大きい結果、つまり負の結果 (ここではすべての角度が度で計算されます)
        //       が頻繁に得られます。負の場合は、正になるまで 360 度を加算します。 360 度より大きい場合は、値が 360 度未満になるまで
        //       360 度を減算します。ほとんどのプログラミング言語では、これらの角度を pi/180 で乗算してラジアンに変換してから、
        //       それらの正弦または余弦を計算する必要があることに注意してください。

        //        Orbital elements of the Sun: //       太陽の軌道要素:
        //            N = 0.0
        //            i = 0.0
        //            w = 282.9404 + 4.70935E-5 * d
        //            a = 1.000000(AU)
        //            e = 0.016709 - 1.151E-9 * d
        //            M = 356.0470 + 0.9856002585 * d
        //        Orbital elements of the Moon://       月の軌道要素:
        //            N = 125.1228 - 0.0529538083 * d
        //            i = 5.1454
        //            w = 318.0634 + 0.1643573223 * d
        //            a = 60.2666(Earth radii)
        //            e = 0.054900
        //            M = 115.3654 + 13.0649929509 * d
        //        Orbital elements of Mercury://       水星の軌道要素:
        //            N =  48.3313 + 3.24587E-5 * d
        //            i = 7.0047 + 5.00E-8 * d
        //            w =  29.1241 + 1.01444E-5 * d
        //            a = 0.387098(AU)
        //            e = 0.205635 + 5.59E-10 * d
        //            M = 168.6562 + 4.0923344368 * d
        //        Orbital elements of Venus://       金星の軌道要素:
        //            N =  76.6799 + 2.46590E-5 * d
        //            i = 3.3946 + 2.75E-8 * d
        //            w =  54.8910 + 1.38374E-5 * d
        //            a = 0.723330(AU)
        //            e = 0.006773 - 1.302E-9 * d
        //            M = 48.0052 + 1.6021302244 * d
        //        Orbital elements of Mars://       火星の軌道要素:
        //            N =  49.5574 + 2.11081E-5 * d
        //            i = 1.8497 - 1.78E-8 * d
        //            w = 286.5016 + 2.92961E-5 * d
        //            a = 1.523688(AU)
        //            e = 0.093405 + 2.516E-9 * d
        //            M = 18.6021 + 0.5240207766 * d
        //        Orbital elements of Jupiter://       木星の軌道要素:
        //            N = 100.4542 + 2.76854E-5 * d
        //            i = 1.3030 - 1.557E-7 * d
        //            w = 273.8777 + 1.64505E-5 * d
        //            a = 5.20256(AU)
        //            e = 0.048498 + 4.469E-9 * d
        //            M = 19.8950 + 0.0830853001 * d
        //        Orbital elements of Saturn://       土星の軌道要素:
        //            N = 113.6634 + 2.38980E-5 * d
        //            i = 2.4886 - 1.081E-7 * d
        //            w = 339.3939 + 2.97661E-5 * d
        //            a = 9.55475(AU)
        //            e = 0.055546 - 9.499E-9 * d
        //            M = 316.9670 + 0.0334442282 * d
        //        Orbital elements of Uranus://       天王星の軌道要素:
        //            N =  74.0005 + 1.3978E-5 * d
        //            i = 0.7733 + 1.9E-8 * d
        //            w =  96.6612 + 3.0565E-5 * d
        //            a = 19.18171 - 1.55E-8 * d(AU)
        //            e = 0.047318 + 7.45E-9 * d
        //            M = 142.5905 + 0.011725806 * d
        //        Orbital elements of Neptune://       海王星の軌道要素:
        //            N = 131.7806 + 3.0173E-5 * d
        //            i = 1.7700 - 2.55E-7 * d
        //            w = 272.8461 - 6.027E-6 * d
        //            a = 30.05826 + 3.313E-8 * d(AU)
        //            e = 0.008606 + 2.15E-9 * d
        //            M = 260.2471 + 0.005995147 * d  
        //       
        //       ここで示されている天王星と海王星の軌道要素は、多少正確性に欠けることに注意してください。
        //       これには、天王星と海王星の間の長周期摂動が含まれます。
        //       摂動の周期は約 4200 年です。したがって、これらの要素では、過去および未来の数世紀以上にわたって、
        //       記載されている精度の範囲内で結果が返されることは期待できません。
        //

        public orbital_elements Sun = new orbital_elements(Sun_param);
        public orbital_elements Moon = new orbital_elements(Moon_param);

        public orbital_elements Mercury = new orbital_elements(Mercury_param);
        public orbital_elements Venus = new orbital_elements(Venus_param);
        public orbital_elements Mars = new orbital_elements(Mars_param);
        public orbital_elements Jupiter = new orbital_elements(Jupiter_param);
        public orbital_elements Saturn = new orbital_elements(Saturn_param);
        public orbital_elements Uranus = new orbital_elements(Uranus_param);
        public orbital_elements Neptune = new orbital_elements(Neptune_param);


        //描画用
        public struct planets_item
        {
            public orbital_elements elem;
            public Color color;
            public int size;
            public string Name;
            public planets_item(string name,orbital_elements _elem, Color _color,int _size)
            {
                Name = name;
                elem = _elem;
                color = _color;
                size = _size;
            }
        }

        public List<planets_item> planets = new List<planets_item>();


        public wakusei()
        {

            planets.Add(new planets_item("太陽",Sun, Color.Gold,10));
            planets.Add(new planets_item("月",Moon, Color.Silver,8));
            planets.Add(new planets_item("水星",Mercury, Color.Gray,5));
            planets.Add(new planets_item("金星",Venus, Color.LightGoldenrodYellow,5));
            planets.Add(new planets_item("火星",Mars, Color.OrangeRed, 5));
            planets.Add(new planets_item("木星",Jupiter, Color.SaddleBrown, 5));
            planets.Add(new planets_item("土星",Saturn, Color.RosyBrown, 5));
            planets.Add(new planets_item("天王星",Uranus, Color.FromArgb(200,180,250,220), 5));
            planets.Add(new planets_item("海王星", Neptune, Color.Blue, 5));

            jd = calc_jd(DateTime.Now);

        }

        double d2rad = Math.PI / 180.0;
        double _jd = 0;
        public double jd
        {
            get { return _jd; }
            set
            {
             //   if (_jd != value)
                {
                    _jd = value;
                    orbital_elements.jd = _jd;
                    orbital_elements.ecl = (23.4393 - 3.563E-7 * jd) * d2rad;
                    Update();
                }
            }
        }

        public void set2Now()
        {
            jd = calc_jd(DateTime.Now);
        }
        public void set2Time(DateTime dt)
        {
            jd = calc_jd(dt);
        }

        //    5. 太陽の位置
        //       太陽の位置は他の惑星の位置と同じように計算されますが、太陽は常に黄道上を移動しており、軌道の離心率は非常に小さいため、いくつかの簡略化を行うことができます。
        //       したがって、太陽については別のプレゼンテーションが提供されます。
        //
        //       もちろん、ここでは太陽の周りの軌道上の地球の位置を計算していますが、地球中心の視点から空を見ているため、太陽が地球の周りの軌道上にあると仮定します。
        //
        //       まず、平均離心率 M と離心率 e から離心率 E を計算します (E と M の単位は度)。
        //           E = M + e*(180/pi) * sin(M) * ( 1.0 + e * cos(M) ) ;//Mの単位はラジアン！！？？
        //       または（E と M がラジアンで表現されている場合）
        //           E = M + e * sin(M) * ( 1.0 + e * cos(M) )

        //       E を計算するための式は正確ではないことに注意してください。ただし、ここでは十分に正確です。
        //
        //       次に、太陽までの距離 r と真の近点角 v を次のように計算します。
        //           xv = r * cos(v) = cos(E) - e
        //           yv = r * sin(v) = sqrt(1.0 - e*e) * sin(E)
        //
        //           v = atan2(yv,xv) 関数
        //           r = sqrt( xv*xv + yv*yv )
        //       (ここで計算された r は、後でrsとして使用されることに注意してください)
        //
        //       atan2() は、x、y 座標のペアを 4 つの象限すべてで正しい角度に変換する関数です。これは、Fortran、C、C++ のライブラリ関数として使用できます。
        //       他の言語では、独自の atan2() 関数を作成する必要があります。それほど難しくはありません。
        //        atan2(y, x ) = atan(y/x)                 if x positive
        //        atan2(y, x ) = atan(y/x) +- 180 degrees  if x negative
        //        atan2(y, x ) = sign(y) * 90 degrees      if x zero

        //       Basicまたは Pascal のコードについては、これらのリンクを参照してください。
        //       Fortran と C/C++ には、すでに標準ライブラリ関数として atan2() があります。
        //
        //       次に、太陽の真の経度を計算します。
        //            lonsun = v + w

        //       lonsun,r を黄道直交地心座標 xs,ys に変換します。
        //           xs = r * cos(lonsun)
        //           ys = r * sin(lonsun)

        //       (太陽は常に黄道面にあるため、zs は当然ゼロです)。xs、ys は黄道面の座標系における太陽の位置です。
        //       これを赤道直角地心座標に変換するには、次を計算します。
        //           xe = xs
        //           ye = ys * cos(ecl)
        //           ze = ys * sin(ecl)

        //       最後に、太陽の赤経 (RA) と赤緯 (Dec) を計算します。
        //           RA = atan2( ye, xe )
        //           Dec = atan2( ze, sqrt(xe*xe+ye*ye) )
        //


        //    5b. The Sidereal Time 恒星時
        //       恒星時と呼ばれる量が必要になることがよくあります。地方恒星時 (LST) は、単にローカル子午線の RA です。
        //       グリニッジ平均恒星時 (GMST) は、グリニッジの LST です。
        //       そして最後に、0h UT のグリニッジ平均恒星時 (GMST0) は、その名のとおり、グリニッジ真夜中の GMST です。
        //       ただし、ここでは GMST0 の概念を少し拡張し、「私たちの」GMST0 を UT 真夜中の従来の GMST0 と同じにし、
        //       また GMST0 を他の任意の時間に定義して、GMST0 が 24 時間ごとに 3 分 51 秒増加するようにします。
        //       すると、次の式はいつでも有効になります。
        //           GMST = GMST0 + UT

        //       また、太陽の平均経度 Ls も必要です。これは、太陽の M と w から次のように計算できます。
        //           Ls = M + w

        //       GMST0 は Ls から簡単に計算できます (GMST0 を度ではなく時間で表したい場合は 15 で割ります)。次に UT を加算して GMST を計算し、最後にローカル経度を加算して
        //       LST を計算します (東経は正、西経は負)。
        //
        //       「時間」は時間で表され、「角度」は度で表されます。地球の自転により、この 2 つは互いに関連しています。ここでは、1 時間は 15 度と同じです。「時間」と
        //       「角度」を加算または減算する前に、必ず同じ単位に変換してください。たとえば、度に変換する場合は、加算/減算する前に時間を 15 倍にします。
        //           GMST0 = Ls + 180_degrees
        //           GMST = GMST0 + UT
        //           LST = GMST + local_longitude

        //       上記の式は、時間が度で表されているかのように書かれています。代わりに、時間が時間で、角度が度で表されていると仮定し、変換係数 15 を明示的に書き出すと、
        //       次のようになります。
        //           GMST0 = (Ls + 180_degrees)/15 = Ls/15 + 12_hours
        //           GMST = GMST0 + UT
        //           LST = GMST + local_longitude/15
        //
        //
        //     9. Perturbations of the Moon 月の摂動
        //        月の位置を計算する場合、約 2 度よりも高い精度が求められるときは、最も重要な摂動を考慮する必要があります。2 分の精度が求められるときは、
        //        以下のすべての項を考慮する必要があります。より低い精度が必要な場合は、いくつかの小さな項を省略できます。
        //
        //        まず、次の式を計算します。

        //           Ms, Mm             Mean Anomaly of the Sun and the Moon
        //           Nm                 Longitude of the Moon's node
        //           ws, wm             Argument of perihelion for the Sun and the Moon
        //           Ls = Ms + ws       Mean Longitude of the Sun(Ns= 0)
        //           Lm = Mm + wm + Nm  Mean longitude of the Moon
        //           D = Lm - Ls        Mean elongation of the Moon
        //           F = Lm - Nm        Argument of latitude for the Moon

        public void Update()
        {
            Sun.update();
            Moon.update();

            Mercury.update();
            Venus.update();
            Mars.update();
            Jupiter.update();
            Saturn.update();
            Uranus.update();
            Neptune.update();

            Sun.sunlocation();
            Moon.planetlocation();

            double Ms = Sun.M;  //   Ms、Mm             太陽と月の平均赤道角
            double Mm = Moon.M;
            double Nm = Moon.N; //   Nm                 月の交点の経度
            double ws = Sun.w;  //   ws、wm             太陽と月の近日点引数
            double wm = Moon.w;
            double Ls = Ms + ws;    //   Ls = Ms + ws       太陽の平均経度 (Ns=0) 
            double Lm = Mm + wm + Nm;//  Lm = Mm + wm + Nm  月の平均経度
            double D = Lm - Ls;     //   D = Lm - Ls        月の平均離角
            double F = Lm - Nm;     //   F = Lm - Nm        月の緯度引数

            //Ms、Mm
            //Nm
            //ws、wm
            //Ls = Ms + ws
            //Lm = Mm + wm + Nm
            //D = Lm - Ls
            //F = Lm - Nm        

            //        月の経度（度）に次の項を加えます。
            //            -1.274 * sin(Mm - 2*D)           (the Evection)(エビクション) 
            //            +0.658 * sin(2*D)                (the Variation)(変動) 
            //            -0.186 * sin(Ms)                 (the Yearly Equation)(年間方程式)
            //            -0.059 * sin(2*Mm - 2*D)
            //            -0.057 * sin(Mm - 2*D + Ms)
            //            +0.053 * sin(Mm + 2*D)
            //            +0.046 * sin(2*D - Ms)
            //            +0.041 * sin(Mm - Ms)
            //            -0.035 * sin(D)                  (the Parallactic Equation)(視差方程式)
            //            -0.031 * sin(Mm + Ms)
            //            -0.015 * sin(2*F - 2*D)
            //            +0.011 * sin(Mm - 4*D)

            double lonm = -1.274 * Math.Sin(Mm - 2 * D);//     (the Evection)(エビクション) 
            lonm += +0.658 * Math.Sin(2 * D);//     (the Variation)(変動) 
            lonm += -0.186 * Math.Sin(Ms);//     (the Yearly Equation)(年間方程式)
            lonm += -0.059 * Math.Sin(2 * Mm - 2 * D);//
            lonm += -0.057 * Math.Sin(Mm - 2 * D + Ms);
            lonm += +0.053 * Math.Sin(Mm + 2 * D);//
            lonm += +0.046 * Math.Sin(2 * D - Ms);//
            lonm += +0.041 * Math.Sin(Mm - Ms);//
            lonm += -0.035 * Math.Sin(D);//     (the Parallactic Equation)(視差方程式)
            lonm += -0.031 * Math.Sin(Mm + Ms);//
            lonm += -0.015 * Math.Sin(2 * F - 2 * D);//
            lonm += +0.011 * Math.Sin(Mm - 4 * D);//

            //        月の緯度（度）に次の項を加えます。
            //            -0.173 * sin(F - 2*D)
            //            -0.055 * sin(Mm - F - 2*D)
            //            -0.046 * sin(Mm + F - 2*D)
            //            +0.033 * sin(F + 2*D)
            //            +0.017 * sin(2*Mm + F)

            double latm = -0.173 * Math.Sin(F - 2 * D);//
            latm += -0.055 * Math.Sin(Mm - F - 2 * D);//
            latm += -0.046 * Math.Sin(Mm + F - 2 * D);//
            latm += +0.033 * Math.Sin(F + 2 * D);//
            latm += +0.017 * Math.Sin(2 * Mm + F); //

            //        月までの距離（地球の半径）に次の項を加えます。
            Moon.r += -0.58 * Math.Cos(Mm - 2 * D);
            Moon.r += -0.46 * Math.Cos(2 * D);

            //        ここでは、経度または緯度で 0.01 度未満、距離で 0.1 地球半径未満の摂動項はすべて省略されています。最大の摂動項のいくつかには、
            //        独自の名前さえあります。
            //        エビクション (最大の摂動) は、数千年前にプトレマイオスによってすでに発見されていました (エビクションはプトレマイオスの周転円の
            //        1 つでした)。
            //        変分と年方程式は、どちらも 16 世紀にティコ・ブラーエによって発見されました。
            //
            //        計算は、より小さな摂動項を省略することで簡略化できます。これによって生じる誤差は、省略された 4 ～ 5 個の最大の項の振幅の合計
            //        を超えることはほとんどありません。経度の 3 つの最大の摂動項と緯度の最大の項のみを計算する場合、経度の誤差は
            //        0.25 度を超えることはほとんどなく、緯度では 0.15 度を超えることはありません。
            //

            Mercury.planetlocation();
            Venus.planetlocation();
            Mars.planetlocation();

            //    10. 木星、土星、天王星の摂動
            //        摂動が 0.01 度を超える惑星は、木星、土星、天王星だけです。まず、次を計算します。
            //            Mj Mean anomaly of Jupiter木星の平均近点
            //            Ms Mean anomaly of Saturn土星の平均近点
            //            Mu Mean anomaly of Uranus(needed for Uranus only)天王星の平均近点（天王星のみ必要）
            Jupiter.planetlocation();
            Saturn.planetlocation();
            Uranus.planetlocation();
            Neptune.planetlocation();

            #region setudou
            double Mj = Jupiter.M;
             Ms = Saturn.M;
            double Mu = Uranus.M;

            // Perturbations for Jupiter.Add these terms to the longitude:木星の摂動。経度に次の項を追加します
            //   -0.332 * sin(2*Mj - 5*Ms - 67.6 degrees)
            //   -0.056 * sin(2*Mj - 2*Ms + 21 degrees)
            //   +0.042 * sin(3*Mj - 5*Ms + 21 degrees)
            //   -0.036 * sin(Mj - 2*Ms)
            //   +0.022 * cos(Mj - Ms)
            //   +0.023 * sin(2*Mj - 3*Ms + 52 degrees)
            //   -0.016 * sin(Mj - 5*Ms - 69 degrees)
            double lonj = -0.332 * Math.Sin(2 * Mj - 5 * Ms - 67.6 * d2rad);
            lonj += -0.056 * Math.Sin(2 * Mj - 2 * Ms + 21 * d2rad);
            lonj += +0.042 * Math.Sin(3 * Mj - 5 * Ms + 21 * d2rad);
            lonj += -0.036 * Math.Sin(Mj - 2 * Ms);
            lonj += +0.022 * Math.Cos(Mj - Ms);
            lonj += +0.023 * Math.Sin(2 * Mj - 3 * Ms + 52 * d2rad);
            lonj += -0.016 * Math.Sin(Mj - 5 * Ms - 69 * d2rad);
          //  Jupiter.L += lonj;

            // Perturbations for Saturn.Add these terms to the longitude:土星の摂動。経度に次の項を追加します
            //   +0.812 * sin(2*Mj - 5*Ms - 67.6 degrees)
            //   -0.229 * cos(2*Mj - 4*Ms - 2 degrees)
            //   +0.119 * sin(Mj - 2*Ms - 3 degrees)
            //   +0.046 * sin(2*Mj - 6*Ms - 69 degrees)
            //   +0.014 * sin(Mj - 3*Ms + 32 degrees)
            // For Saturn: also add these terms to the latitude:土星の場合: 緯度に次の項も追加します。
            //   -0.020 * cos(2*Mj - 4*Ms - 2 degrees)
            //   +0.018 * sin(2*Mj - 6*Ms - 49 degrees)
            double lons = + 0.812 * Math.Sin(2 * Mj - 5 * Ms - 67.6 * d2rad);
            lons += -0.229 * Math.Cos(2 * Mj - 4 * Ms - 2 * d2rad);
            lons += +0.119 * Math.Sin(Mj - 2 * Ms - 3 * d2rad);
            lons += +0.046 * Math.Sin(2 * Mj - 6 * Ms - 69 *d2rad);
            lons += +0.014 * Math.Sin(Mj - 3 * Ms + 32 * d2rad);
                //For Saturn: also add these terms to the latitude:土星の場合: 緯度に次の項も追加します。
            double lats = -0.020 * Math.Cos(2 * Mj - 4 * Ms - 2 * d2rad);
            lats += +0.018 * Math.Sin(2 * Mj - 6 * Ms - 49 * d2rad);

           // Saturn.L += lons;
         //????   Saturn.L += lats;
            //       Perturbations for Uranus: Add these terms to the longitude:天王星の摂動: 経度に次の項を追加します。
            //         +0.040 * sin(Ms - 2*Mu + 6 degrees)
            //         +0.035 * sin(Ms - 3*Mu + 33 degrees)
            //         -0.015 * sin(Mj - Mu + 20 degrees)
            double lonu = +0.040 * Math.Sin(Ms - 2 * Mu + 6 * d2rad);
            lonu += + 0.035 * Math.Sin(Ms - 3 * Mu + 33 * d2rad);
            lonu += - 0.015 * Math.Sin(Mj - Mu + 20 * d2rad);

      //      Uranus.L += lonu;

            //        「大木星土星項」は、木星と土星の両方にとって最大の摂動です。その周期は 918 年で、その振幅は木星では 0.332 度、土星では 0.812 度です。
            //        これはまた「大土星天王星項」でもあり、周期は 560 年、天王星の振幅は 0.035 度、土星では 0.01 度未満です (したがって省略)。その他の摂動の周期は
            //        14 年から 100 年です。また、「大天王星海王星項」についても言及する必要があります。これは周期が 4220 年で振幅が約 1 度です。これはここには含まれず、
            //        代わりに天王星と海王星の軌道要素に含まれています。水星
            //
            //        、金星、火星については、すべての摂動を無視できます。海王星の場合、前述のように、唯一の大きな摂動は既に軌道要素に含まれているため、
            //        それ以上の摂動項を考慮する必要はありません。
            //

            //public void EarthCenter(double xsun, double ysun, double ecl)
            //     11. 地球中心座標
            //        これで、惑星の太陽中心座標が計算され、最も重要な摂動が組み込まれました。次に、地球中心座標を計算します。
            //        摂動された lonecl、latecl、r を  (摂動された)xh、yh、zh に変換する必要があります。
            //            xh = r * cos(lonecl) * cos(latecl)
            //            yh = r * sin(lonecl) * cos(latecl)
            //            zh = r * sin(latecl)

            //        月の位置を計算する場合、これはすでに地心位置なので、xg=xh、yg=yh、zg=zh と設定するだけです。
            //        まだの場合は、太陽の位置も計算する必要があります。
            //        lonsun、rs (rs はSunlocationで計算された r です) を xs、ys に変換します。

            //            xs = rs * cos(lonsun)//Sunlocationですでに計算済み
            //            ys = rs * sin(lonsun)

            //        (もちろん、 xh、yh、zh および xs、ys に変換する前に、 loneclおよびlonsunに歳差補正を追加する必要があります)。
            //        次に、太陽中心の位置から地心位置に変換します。
            //            xg = xh + xs
            //            yg = yh + ys
            //            zg = zh

            #endregion
            //すべての惑星について行う

            Moon.LocForMoon(lonm, latm);
            Mercury.EarthCenter();//, Mercury.L);
            Venus.EarthCenter();//Venus.L);
            Mars.EarthCenter();//Mars.L);
            Jupiter.EarthCenter(lonj);// Jupiter.L + lonj);
            Saturn.EarthCenter(lons,lats);// Saturn.L + lons);
            Uranus.EarthCenter(lonu);//Uranus.L + lonu);
            Neptune.EarthCenter();// Neptune.L);

            //    これで、地球を中心とする惑星の地心位置が、直交黄道座標でわかるようになりました。
            //
            // 12. 赤道座標
            //    直交黄道座標を直交赤道座標に変換してみましょう。yz 平面を黄道傾斜角 ecl だけ回転させるだけです。
            //        xe = xg
            //        ye = yg * cos(ecl) - zg * sin(ecl)
            //        ze = yg * sin(ecl) + zg * cos(ecl)
            //    最後に、惑星の赤経 (RA) と赤緯 (Dec) を計算します。
            //        RA = atan2( ye, xe )
            //        Dec = atan2( ze, sqrt(xe*xe+ye*ye) )
            //    地心距離を計算します。
            //        rg = sqrt(xg*xg+yg*yg+zg*zg) = sqrt(xe*xe+ye*ye+ze*ze)
            //    これで赤道座標の計算が完了します。
            //

            //     12b. 方位座標
            //        方位座標 (方位角と高度) を見つけるには、まずオブジェクトの HA (時角) を計算します。ただし、まず LST (地方恒星時) を計算する必要があります。
            //        これは、 上記の5bで説明したとおりに行います。LST がわかれば、次のように HA を簡単に計算できます。
            //            HA = LST − RA
            //        HA は通常、-12 ～ +12 時間、または -180 ～ +180 度の範囲で与えられます。HA がゼロの場合、オブジェクトは南に直接見えます。
            //        HA が負の場合、オブジェクトは南の東にあり、HA が正の場合、オブジェクトは南の西にあります。計算された HA がこの範囲外になる場合は、
            //        HA がこの範囲内になるまで 24 時間 (または 360 度) を加算または減算します。
            //
            //        次に、オブジェクトの HA と Decl をローカルの方位角と高度に変換します。これを行うには、ローカルの緯度である lat も知っておく必要があります。
            //        次に、次のように進めます。
            //            x = cos(HA) * cos(Decl)
            //            y = sin(HA) * cos(Decl)
            //            z = sin(Decl)
            //
            //            xhor = x * sin(緯度) - z * cos(lat)
            //            yhor = y
            //            zhor = x * cos(緯度) + z * sin(lat)
            //
            //            az = atan2( yhor, xhor ) + 180_degrees
            //            alt = asin( zhor ) = atan2( zhor, sqrt(xhor*xhor+yhor*yhor) )

            //        これで、ローカル方位角と高度の計算が完了します。方位角は北で 0、東で 90 度、南で 180 度、西で 270 度であることに注意してください。
            //        高度は、もちろん (数学的な) 地平線で 0、天頂で 90 度、地平線の下では負になります。
            //
            //     13. 月の地心位置
            //        先ほど計算した月の位置は、地球の中心にいる架空の観測者から見た地球中心の位置です。しかし、実際の観測者は地球の表面にいるので、別の位置、
            //        つまり地心位置を見ることになります。この位置は地心位置と 1 度以上異なることがあります。地心位置を計算するには、地心位置に補正を加える
            //        必要があります。
            //
            //        まず、月の視差、つまり月から見た地球の (赤道) 半径の見かけの大きさを計算してみましょう。
            //            mpar = asin( 1/r )

            //        ここで、r は地球の半径における月の距離です。水平座標 (方位角と高度) で補正を適用するのが最も簡単です。1 ～ 2 分角の精度目標の範囲内
            //        であれば、方位角に補正を適用する必要はありません。地平線上の高度に補正を適用するだけで済みます。
            //            alt_topoc = alt_geoc - mpar * cos(alt_geoc)

            //        ただし、赤道座標で直接地形中心の位置を修正する必要がある場合があります。たとえば、特定の場所から見た月がプレアデスの前を通過する
            //        様子を星図に描きたい場合などです。その場合、月の地心赤経と赤緯 (RA、Decl)、地方恒星時 (LST)、および緯度 (lat) を知る必要があります。
            //
            //        天文緯度 (lat) は、まず地心緯度 (gclat) に変換し、地球の赤道半径で表した地球の中心からの距離 (rho) を算出しなければなりません。
            //        おおよその地形中心の位置だけが必要な場合は、地球が完全な球体であると仮定して、次のように設定するのが最も簡単です。
            //            gclat = lat,  rho = 1.0

            //        しかし、地球の平坦化を考慮したい場合は、代わりに次の式を計算します。
            //            gclat = lat - 0.1924_deg * sin(2*lat)

            //            rho   = 0.99833 + 0.00167 * cos(2*lat)

            //        次に、月の地心 RA から月の地心時角 (HA) を計算します。まず、 上記の5bで説明したように LST を計算し、次に HA を次のように計算します。
            //            HA = LST − RA
            //        補助角 g も必要です。
            //            g = Tan( Tan(gclat) / cos(HA) )

            //        これで、地心赤経と赤緯 (RA、Decl) を地心値 (topRA、topDecl) に変換する準備ができました。
            //            topRA = RA - mpar * rho * cos(gclat) * sin(HA) / cos(Decl)
            //            topDecl = Decl - mpar * rho * sin(gclat) * sin(g - Decl) / sin(g)

            //        （declが正確に90度の場合、cos(Decl)はゼロになり、topRAを計算するときにゼロ除算が発生しますが、その式は天の極に非常に近い場合に
            //        のみ破綻し、そこで月を見ることはありません。また、gclatが正確にゼロの場合、gもゼロになり、topDeclを計算するときにゼロ除算が発生します。
            //        その場合、topDeclの式を次のように置き換えます。
            //            topDecl = Decl - mpar * rho * sin(-Decl) * cos(HA)

            //        これは gclat がゼロの場合に有効です。また、gclat が極めてゼロに近い場合にも使用できます。
            //
            //        このトポセントリック位置の補正は、太陽と惑星にも適用できます。ただし、それらははるかに遠いため、補正ははるかに小さくなります。
            //        補正は、金星が内合のときに最大になります。このとき、金星の視差は 32 秒角よりもいくらか大きくなります。
            //        最終的な精度を 1 ～ 2 分角にするという目標の範囲内で、金星が内合に近いとき、およびおそらく火星が好転しているときに
            //        トポセントリック位置に補正することは、ほとんど正当化されない可能性があります。ただし、その他のすべての場合、この補正は、
            //        精度の目標の範囲内で無視しても問題ありません。この場合、月についてのみ考慮する必要があります。
            //
            //        惑星のトポセントリック座標も計算する場合は、月の場合と同じ方法で行いますが、1 つの例外があります。月の視差は、次の式で計算される
            //        惑星の視差 (ppar) に置き換えられます。
            //            ppar = (8.794/3600)_deg / r

            //        ここで、r は天文単位で表した地球からの惑星の距離です。
            //
            //     14. 冥王星の位置
            //        冥王星については、これまで解析理論が構築されたことはありません。この惑星の運動を最も正確に表現できるのは、数値積分です。
            //        しかし、これらの数値積分に「曲線の当てはめ」を行うと、1800 年頃から 2100 年頃まで有効な以下の式が得られます。
            //        日数 d を通常どおり計算します (セクション 3 )。次に、次の角度を計算します。
            //            S = 50.03 + 0.033459652 * d
            //            P = 238.95 + 0.003968789 * d

            //        次に、太陽中心の黄道経度と緯度（度）、および距離（au）を計算します。
            //            lonecl  = 238.9508 + 0.00400703 * d
            //                    - 19,799 * sin(P) + 19,848 * cos(P)
            //                     + 0.897 * sin(2*P) - 4.956 * cos(2*P)
            //                     + 0.610 * sin(3*P) + 1.211 * cos(3*P)
            //                     - 0.341 * sin(4*P) - 0.190 * cos(4*P)
            //                     + 0.128 * sin(5*P) - 0.034 * cos(5*P)
            //                     - 0.038 * sin(6*P) + 0.031 * cos(6*P)
            //                     + 0.020 * sin(S-P) - 0.010 * cos(S-P)
            //
            //            latecl  = -3.9082
            //                     - 5.453 * sin(P) - 14.975 * cos(P)
            //                     + 3.527 * sin(2*P) + 1.673 * cos(2*P)
            //                     - 1.051 * sin(3*P) + 0.328 * cos(3*P)
            //                     + 0.179 * sin(4*P) - 0.292 * cos(4*P)
            //                     + 0.019 * sin(5*P) + 0.100 * cos(5*P)
            //                     - 0.031 * sin(6*P) - 0.026 * cos(6*P)
            //                                           + 0.011 * cos(S-P)
            //
            //           r = 40.72
            //                   + 6.68 * sin(P) + 6.90 * cos(P)
            //                   - 1.18 * sin(2*P) - 0.03 * cos(2*P)
            //                   + 0.15 * sin(3*P) - 0.14 * cos(3*P)

            //        これで冥王星の太陽中心距離と黄道経度/緯度がわかりました。地心座標に変換するには、他の惑星の場合と同じ手順を実行します。
            //
            //     15. 惑星の離角と物理的な暦
            //        惑星の太陽中心座標と地球中心座標の計算がようやく完了したら、惑星がどのように見えるかを知ることも興味深いでしょう。
            //        惑星はどのくらいの大きさに見えるでしょうか? 位相と大きさ (明るさ) はどれくらいでしょうか?
            //        これらの計算は位置の計算よりもはるかに簡単です。
            //
            //        まずは惑星の見かけの直径を計算してみましょう。
            //            d = d0 / R

            //        R は天文単位での惑星の地心距離、d は 1 天文単位の距離における惑星の見かけの直径です。d0 は当然ながら惑星ごとに異なります。
            //        以下の値は秒角で示されています。惑星によっては赤道直径と極直径が異なる場合があります。
            //           Mercury     6.74"                  //            マーキュリー 
            //           Venus      16.92"                  //            金星
            //           Earth      17.59" equ    17.53" pol//            地球
            //           Mars        9.36" equ     9.28" pol//            火星 
            //           Jupiter   196.94" equ   185.08" pol//            木星
            //           Saturn    165.6"  equ   150.8"  pol//            土星
            //           Uranus     65.8"  equ    62.1"  pol//            天王星
            //           Neptune    62.2"  equ    60.9"  pol//            ネプチューン
            //   
            //        1天文単位での太陽の見かけの直径は1919.26" です。月の見かけの直径は次のとおりです。
            //            d = 1873.7" * 60 / r



            //        ここで、r は地球の半径における月の距離です。
            //
            //        他に知りたい 2 つの量は、位相角と離角です。
            //
            //        位相角は位相を示します。位相角が 0 の場合、惑星は「満月」に見え、90 度の場合、「半分」に見え、180 度の場合、「新月」に見えます。
            //        月と内惑星 (つまり、水星と金星) のみが、約 50 度を超える位相角を持つことができます。
            //
            //        離角は、惑星の太陽からの見かけの角度距離です。離角が約 20 度より小さい場合、惑星を観測するのは困難であり、約 10 度より小さい場合、
            //        通常、惑星を観測することはできません。
            //
            //        位相角と離角を計算するには、惑星の太陽中心距離 r、地心距離 R、および太陽までの距離 s を知る必要があります。これで、位相角 FV と
            //        離角 elong を計算できます。
            //            elong = acos( ( s*s + R*R - r*r ) / (2*s*R) )
            //
            //            FV    = acos( ( r*r + R*R - s*s ) / (2*r*R) )

            //        位相角がわかれば、位相を簡単に計算できます。
            //            phase  =  ( 1 + cos(FV) ) / 2  =  hav(180_deg - FV)

            //        hav は「ハーフサイン」関数です。「ハーフサイン」(または「ハーフバーサイン」) は古くて現在は使われていない三角関数です。
            //        次のように定義されます。
            //           hav(x)  =  ( 1 - cos(x) ) / 2   =   sin^2 (x/2)

            //        いつものように、月については別の手順を使用する必要があります。月は地球に非常に近いため、上記の手順では誤差が大きくなりすぎます。
            //        代わりに、月の黄道経度と緯度、mlon と mlat、および太陽の黄道経度、slon を使用して、まず月の離角を計算し、次に月の位相角を計算します。
            //            elong = acos( cos(slon - mlon) * cos(mlat) )
            //    
            //            FV = 180_deg - elong

            //        最後に、惑星の等級 (または明るさ) を計算します。ここでは、惑星ごとに異なる式を使用する必要があります。FV は位相角 (度)、
            //        r は太陽中心距離、R は地心距離 (いずれも AU) です。

            //            Mercury:   -0.36 + 5*log10(r* R) + 0.027 * FV + 2.2E-13 * FV**6   //            水星
            //            Venus:     -4.34 + 5*log10(r* R) + 0.013 * FV + 4.2E-7  * FV**3   //            金星:
            //            Mars:      -1.51 + 5*log10(r* R) + 0.016 * FV                     //            火星:
            //            Jupiter:   -9.25 + 5*log10(r* R) + 0.014 * FV                     //            木星:
            //            Saturn:    -9.0  + 5*log10(r* R) + 0.044 * FV + ring_magn         //            土星:
            //            Uranus:    -7.15 + 5*log10(r* R) + 0.001 * FV                     //            天王星:
            //            Neptune:   -6.90 + 5*log10(r* R) + 0.001 * FV                     //            海王星:
            //
            //            Moon:      +0.23 + 5*log10(r* R) + 0.026 * FV + 4.0E-9 * FV**4    //            月:
            //                              ** is the power operator
            //        ** はべき乗演算子なので、FV**6 は位相角 (度) の 6 乗です。FV が 150 度の場合、FV**6 は約 1.14E+13 となり、これはかなり大きな数です。
            //
            //        月については、太陽中心距離 r と地心距離 R も AU (天文単位) で必要です。ここで、r は AU での太陽の地心距離と同じに設定できます。
            //        地球の半径で計算した月の地心距離 R は AU に変換する必要があります。これは sin(17.59"/2) = 1/23450 を掛けて行います。または、
            //        月の等級式を変更して、r を AU で、R を地球の半径で使用することも可能です。
            //            Moon:     -21.62 + 5*log10(r*R) + 0.026 * FV + 4.0E-9 * FV**4

            //        土星は環があるため、特別な扱いが必要です。土星の環が「開いている」場合、土星は、環を端から見るときよりもずっと明るく見えます。
            //        ring_mang は次のように計算します。
            //            ring_magn = -2.6 * sin(abs(B)) + 1.2 * (sin(B))**2

            //        ここで、B は土星の環の傾きであり、これも計算する必要があります。次に、土星の地心黄道経度と緯度 (los、las) から始めます。
            //        これは、上記の段落 11で土星に対して計算された xg、yg、zg から計算します。
            //            los = atan2( yg, xg )
            //            las = atan2( zg, sqrt( xg*xg + yg*yg ) )

            //        また、黄道に対するリングの傾き ir と、リング面の「昇交点」Nr も必要です。
            //            ir = 28.06_deg
            //            Nr = 169.51_deg + 3.82E-5_deg * d

            //        ここで、d はこれまで何度も使用してきた「日数」です。次に、リングの傾きを計算してみましょう。
            //            B = asin( sin(las) * cos(ir) - cos(las) * sin(ir) * sin(los-Nr) )
            //        これで惑星の等級の計算は終了です。
            //
            //     16. 小惑星の位置
            //        小惑星の場合、軌道要素は N、i、w、a、e、M のように表されることが多く、N、i、w は特定の時代 (現在では通常 2000.0) に有効です。
            //        簡略化された計算スキームでは、時代による大きな変化は N のみで発生します。N_Epoch を、使用したい N (今日の時代) に変換するには、
            //        歳差運動の補正を追加するだけです。
            //            N = N_Epoch + 0.013967 * ( 2000.0 - Epoch ) + 3.82394E-5 * d

            //        ここで、Epoch は 1950.0 や 2000.0 のように、分数付きの年として表されます。
            //
            //        ほとんどの場合、平均近点 M は、小惑星の位置を計算したい日とは別の日に与えられます。日周運動 n が与えられている場合は、
            //        n * (日数での時差) を M に加算するだけです。n は与えられていないが、期間 P (日数) が与えられている場合は、n = 360.0/P となります。
            //        P が与えられていない場合は、次のように計算できます。
            //            P = 365.2568984 * a**1.5 (days) = 1.00004024 * a**1.5   (years)

            //        ** はべき乗演算子です。a**1.5 は sqrt(a*a*a) と同じです。
            //
            //        すべての軌道要素が計算されたら、他の惑星と同じように進めます (セクション 6 )。
            //
            //     17. 彗星の位置。
            //        楕円軌道を持つ彗星の場合、M は通常は与えられません。代わりに、近日点の時間 T が与えられます。近日点では M はゼロです。
            //        他の瞬間の M を計算するには、まず T (セクション 3 ) の「日数」d を計算します。これを dT と呼びます。
            //        次に、位置を計算したい瞬間の「日数」d を計算します。これを d と呼びます。次に、平均近点角 M が次のように計算されます。
            //            M = 360.0 * (d-dT)/P  (degrees)(度)
            //        ここで、P は日数で与えられ、d-dT は当然のことながら、最後の近日点からの経過時間で、これも日数で与えられます。
            //
            //        また、a (長半径) は通常与えられません。代わりに、q (近日点距離) が与えられます。a は、q と e から簡単に計算できます。
            //            a = q / (1.0 - e)
            //        その後は小惑星の場合と同じように進めます（セクション16）。
            //
            //     18. 放物線軌道
            //        彗星が放物線軌道を描いている場合は、別の方法を使用する必要があります。その場合、彗星の軌道周期は無限大で、
            //        M (平均近点角) は常に 0 です。離心率 e は常に 1 です。半長軸 a は無限大なので、代わりに近日点距離 q を
            //        直接使用する必要があります。放物線軌道を計算するには、次のように進めます。
            //
            //        近日点の瞬間 T の「日数」d を計算します (これを dT と呼びます)。位置を計算したい瞬間の d を計算します (セクション 3 )。
            //        定数 k はガウスの重力定数です。k = 0.01720209895 です。
            //
            //        次に、次を計算します。
            //            H = (d-dT) * (k/sqrt(2)) / q**1.5

            //        ここで、q**1.5 は sqrt(q*q*q) と同じです。また、次を計算します。
            //            h = 1.5 * H
            //            g = sqrt( 1.0 + h*h )
            //            s = cbrt(g + h) - cbrt(g - h)

            //        cbrt() は立方根関数です: cbrt(x) = x**(1.0/3.0)。式は、g+h と gh が常に正になるように考案されています。
            //        したがって、ここでは cbrt(x) を exp(log(x)/3.0) として安全に計算できます。一般に、cbrt(-x) = -cbrt(x) であり、
            //        もちろん cbrt(0) = 0 です。
            //
            //        離心率の異常値を計算する代わりに、真の異常値と太陽中心距離を直接計算します:
            //            v = 2.0 * atan(s)
            //            r = q * ( 1.0 + s*s )

            //        真の近点角vと太陽中心距離rがわかれば、空間内の位置を計算することができます（セクション7）。
            //
            //    19. 放物線に近い軌道。
            //        新しく発見された彗星の最も一般的なケースは、軌道が正確な放物線ではないが、それに近い場合です。
            //        その離心率は 1 よりわずかに下、またはわずかに上です。ここで紹介するアルゴリズムは、離心率が約 0.98 から 1.02 の場合に使用できます。
            //        離心率が 0.98 より小さい場合は、代わりに楕円アルゴリズム (ケプラーの方程式など) を使用する必要があります。
            //        既知の彗星で離心率が 1.02 を超えるものはありません。
            //
            //        純粋な放物線軌道の場合、近日点からの経過時間 (日数) d-dT と近日点距離 q を計算することから始めます。
            //        離心率 e も知っておく必要があります。定数 k はガウスの重力定数です。正確には k = 0.01720209895 です。
            //
            //        次に、次のように進めます。
            //            a = 0.75 * (d-dT) * k * sqrt( (1 + e) / (q*q*q) )
            //            b = 平方根(1 + a*a)
            //            W = cbrt(b + a) - cbrt(b - a)
            //            f = (1 - e) / (1 + e)
            //
            //            a1 = (2/3) + (2/5) * W*W
            //            a2 = (7/5) + (33/35) * W*W + (37/175) * W**4
            //            a3 = W*W * ( (432/175) + (956/1125) * W*W + (84/1575) * W**4 )
            //
            //            C = W*W / (1 + W*W)
            //            ｇ＝ｆ＊Ｃ＊Ｃ
            //            w = W * ( 1 + f * C * ( a1 + a2*g + a3*g*g ) )
            //
            //            v = 2 * atan(w)
            //            r = q * ( 1 + w*w ) / ( 1 + w*w * f )

            //        このアルゴリズムは、ほぼ放物線軌道の真の近点角 v と太陽中心距離 r を算出します。
            //        このアルゴリズムは近点から非常に離れた場所では失敗することに注意してください。
            //        ただし、冥王星よりも近いすべての彗星に対しては精度は十分です。
            //
            //        真の近点角 v と太陽中心距離 r がわかれば、空間内の位置を計算する作業に進むことができます (セクション 7 )。
            //
            //     20. 双曲軌道
            //        近年、軌道が非常に強い双曲線軌道を持つ小惑星 1 個と彗星 1 個が発見されました。
            //        このため、上で概説したような放物線に近い軌道では不正確になりすぎます。
            //        小惑星の場合、離心率は 1.2 で、彗星の場合、離心率は 3 近くでした。
            //        このような強い双曲線軌道の場合、軌道位置を計算するには別の方法が必要です。
            //
            //        まず、近日点距離 q と離心率 e から双曲線の長半径 a を計算します。双曲線の場合、離心率 e は
            //        1 より大きいため、長半径は負になることに注意してください。
            //            a = q / (1 - e)

            //        次に、楕円軌道の平均異常度 M の双曲等価を計算します。ここでもこれを M と呼びます。
            //        双曲の場合、a は負なので、1.5 乗するにはこれを反転する必要があります。
            //            M = (tT) / (-a)**1.5

            //        ここで、双曲線関数 sinh(x) と cosh(x) を使用する必要があります。
            //        コンピュータ システムでこれらの関数が利用できない場合は、自分で定義する必要があります。
            //        これらの関数は次のように定義されます。
            //            sinh(x) = ( exp(x) - exp(-x) ) / 2
            //            cosh(x) = ( exp(x) + exp(-x) ) / 2

            //        ケプラーの方程式の双曲バージョンは次のようになります。
            //            M = e * sinh(F) - F

            //        そして、楕円軌道の場合と同様に、まず F の最初の近似値を計算し、
            //        次に反復式を使用して F の値が収束するまで反復します。
            //        F の最初の近似値は次のように単純になります。
            //            F = M

            //        F の反復式は次のようになります。
            //            F1 = ( M + e * ( F0 * cosh(F0) - sinh(F0) ) ) / ( e * cosh(F0) - 1 )

            //        最初の反復の前に F0 = F に設定し、各反復の後に F0 = F1 に設定します。F1
            //        と F0 が十分な精度で同じ値に収束するまで反復を繰り返します。F
            //
            //        がわかれば、真の異常値 v と太陽中心距離 r を計算できます。
            //            v = 2 * arctan( sqrt((e+1)/(e-1)) ) * tanh(F/2)
            //            r = a * ( 1 - e * e ) / ( 1 + e * cos(v) )

            //        真の近点角vと太陽中心距離rがわかれば、空間内の位置を計算することができます（セクション7）。
            //
            //     21. Rise and set times.
            //        (この件については独自の文書が発行されています)http://stjarnhimlen.se/comp/riset.html
            //
            //     22. 軌道要素の妥当性。Validity of orbital elements.

            //        主に木星や土星のような巨大惑星からの摂動により、天体の軌道要素は常に変化しています。
            //        ここで示す太陽、月、および主要な惑星の軌道要素は、長期間有効です。
            //        ただし、彗星や小惑星に与えられた軌道要素は、限られた期間のみ有効です。
            //        それらがどのくらいの期間有効であるかを一概に言うことは困難です。それは、必要な精度や、
            //        彗星や小惑星が木星などから受ける摂動の大きさなど、多くの要因に依存します。
            //        彗星は、わずかな摂動しか受けずに、ほぼ同じ軌道で数軌道周期移動するかもしれませんが、
            //        突然、木星に非常に接近して軌道が大幅に変更される可能性があります。
            //        これを信頼できる方法で計算することは非常に複雑であり、この説明の範囲外です。
            //        しかし、経験則として、特定の時代の軌道要素を使用した場合、その瞬間から
            //        1 回転または数回回転した小惑星の計算された位置には、少なくとも 1 分または数分、
            //        場合によってはそれ以上の誤差があると想定できます。誤差は時間とともに蓄積されます。
            //
            //     23. 他のサイトへのリンク。
            //        JD Fernie による ユリウス日数の 1 行アルゴリズム: http://adsbit.harvard.edu//full/1983IAPPP..13...16F/0000016.000.html
            //        Keith Burnett による
            //
            //        天文学的計算: https://web.archive.org/web/20050730090118/http://www.xylem.f2s.com/kepler/
            //
            //        無料の BASIC プログラムは、 https://web.archive.org/web/20030726043728/ftp
            //        ://seds.lpl.arizona.edu/pub/software/pc/general/ の ast.exe (GWBASIC が必要)
            //        およびduff2ed.exe (Pete Duffett-Smiths のプログラム)にあります。Willmann -Bellの数学と天体力学に関する
            //
            //        書籍: http://www.willbell.com/math/index.htm John Walker のフリーウェア プログラムHome Planet
            //        + その他のもの: http://www.fourmilab.ch/ Elwood Downeyの Xephem
            //        およびEphemプログラム (C ソース コード付き): http://www.clearskyinstitute.com/xephem/。
            //        Ephem は、 https://web.archive.org/web/20030726043728/ftp
            //        ://seds.lpl.arizona.edu/pub/software/pc/general/のephem421.zipにも保存されています。
            //        Steven Moshier : 天文学および数値ソフトウェアのソースコード: http://www.moshier.net/
            //        Dan Brutonの天文学ソフトウェアのリンク: http://www.midnightkite.com/index.aspx?URL=Software
            //        Mel Bartel のソフトウェア (ATM 関連のものが多数): https://web.archive.org/web/20050212132844/http://www.efn.org/~mbartels/tm/software.html USNO の Almanac データ: http://aa.usno.navy.mil/data/ https://web.archive.org/web/*/http://aa.usno.navy.mil/data/ 小惑星ローウェル天文台の軌道要素: http://asteroid.lowell.edu/ ftp://ftp.lowell.edu/pub/elgb/astorb.html
            //        SACダウンロード: https://web.archive.org/web/20180402105538/http://www.saguaroastro.org/content/downloads.htm
            //        AMSAT の地球衛星ソフトウェア: http://www.amsat.org/amsat-n ​​
            //        ew/tools/software.php https://web.archive.org/web/20130727105126/http://www.amsat.org/amsat/ftpsoft.html
            //        IMCCE (旧 Bureau des Longitudes): http://www.imcce.fr/
            //        VSOP87: ftp://ftp.imcce.fr/pub/ephem/planets/vsop87/
            //        VSOP2010: ftp://ftp.imcce.fr/pub/ephem/planets/vsop2010/
            //        VSOP2013: ftp://ftp.imcce.fr/pub/ephem/planets/vsop2013/
            //        NRAOのSSEphem: ftp://ftp.cv.nrao.edu/NRAO-staff/rfisher/SSEphem/ いくつかのカタログは
            //
            //
            //          CDS、フランス、ストラスブール- 高精度軌道理論:
            //          概要: http://cdsweb.u-strasbg.fr/cgi-bin/qcat?VI/
            //          歳差運動と平均軌道要素: http://cdsweb.u-strasbg.fr/cgi-bin/qcat?VI/66/
            //          ELP2000-82 (月の軌道理論): http://cdsweb.u-strasbg.fr/cgi-bin/qcat?VI/79/
            //          VSOP87 (惑星の軌道理論): http://cdsweb.u-strasbg.fr/cgi-bin/qcat?VI/81/
            //          
            //          VizieR 検索エンジン(フランス、ストラスブール)、ミラー (米国ハーバード)
            //          USNO ZZCAT: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I%2F157
            //          黄道星の XZ カタログ: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/291&-to=3
            //          ヒッパルコスとティコのカタログ: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/239&-to=2
            //          ティコ 2 カタログ: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/259&-to=3
            //          USNO A2 カタログ: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/252&-to=3
            //          USNO B1 カタログ: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/284&-to=3
            //          HST ガイド スター カタログ 1.1: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/220&-to=3
            //          HST ガイド スター カタログ 1.2: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/254&-to=3
            //          HST ガイド スター カタログ 2.2: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/271&-to=3
            //          HST ガイド スター カタログ 2.3: http://vizier.u-strasbg.fr/viz-bin/VizieR?-source=I/305&-to=3
            //          
            //          JPL Ephemerides DE102 - DE438: ftp://ssd.jpl.nasa.gov/pub/eph/planets/ 
            //          
            //          The original ZC (Zodiacal Catalogue) from 1940: http://stjarnhimlen.se/zc/ http://web.archive.org/web/20030604102426/sorry.vse.cz/~ludek/zakryty/pub.phtml#zc http://web.archive.org/web/20030728014108/http://sorry.vse.cz/~ludek/zakryty/pub/
            //          
            //
            //
            //
        }
    }

















}