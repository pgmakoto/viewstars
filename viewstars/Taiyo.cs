using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ucPgmac
{
    public class Taiyometds
    {
        //https://www.metds.co.jp/wp-content/uploads/2023/07/TE_Simplified_SP_230724.pdf

        double lat, lon, lons, year, month, day, hour, min, sec, in0, dlt, et, degh, dega;
        int fi = 8, fo = 10;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"> 計算対象地点の緯度 （°）(東経は＋,西経は-)</param>
        /// <param name="lon">計算対象地点の経度 （°）(北半球は＋，南半球は-)</param>
        /// <param name="lons">標準時の地点の緯度 （°）(東経は＋,西経は-,日本では 135.0)</param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec">視赤緯(°) </param>
        /// <param name="dlt">均時差(°)</param>
        /// <param name="et">均時差(°)</param>
        /// <param name="degh">太陽高度(°)</param>
        /// <param name="dega">太陽方位角(°)（真南を 0 とし、西回りを＋、東回りを-）</param>
        public void pos(double lat, double lon, double lons
            , int year, int month, int day, int hour, int min, int sec
            // , IN0
            , ref double dlt, ref double et, ref double degh, ref double dega)
        {
            double J0 = 1.37;
            double rad = 3.141592 / 180.0;
        }

    }

    /*
     https://www.metds.co.jp/wp-content/uploads/2019/03/TE_SunPosition_160401.pdf

    2.2 計算プログラムと計算例
    2.2.1 太陽位置の計算プログラム（松本による C++コード）
    C++言語による計算プログラムを以下に示す。
    太陽位置は，関数 SolPos( const SInputparams*
    aInput, const SDate* aDate, const double aTime, SPOutputParams* aOutput) 
    を呼び出して
    求めることができる。
    */

    /*

#ifndef SolPosH
#define SolPosH
// ===================================================
// SolPos.h - 太陽位置計算関数 SolPos その他の
// 関数群ヘッダーファイル
// ===================================================
//
#define YEAR_MIN (1971) // 計算対象年の下限
//
typedef unsigned int uint;
typedef unsigned long ulong;
//
// 日付構造体
typedef struct {
uint YYYY; // 年 (1971 <= YYYY)
uint MM; // 月 (1 <= MM <= 12)
uint DD; // 日 (1 <= DD <= 28|29|30|31)
} SDate;
//
// 時刻構造体
typedef struct {
uint hh; // 時 (0 <= hh <= 23)
uint mm; // 分 (0 <= mm <= 59)
double ss; // 秒 実数であることに注意
} STime;
//
// 太陽位置計算用入力パラメータ構造体
typedef struct {
double Latit; // 計算対象地点の緯度 (deg.), +は北緯
double Longit; // 計算対象地点の経度 (deg.), +は東経
double RefLongit; // 計算対象地点の標準時経度 (deg.),
// +は東経（日本: 135.0）
} SInparams;
//
// 太陽位置計算結果 (出力) パラメータ構造体
typedef struct {
double SinH, CosH; // 太陽高度角の正弦，余弦
double SinA, CosA; // 太陽方位角の正弦，余弦
double SinD, CosD; // 視赤緯の正弦，余弦
double Et, hAngle; // 均時差 (deg.) と時角 (deg.)
double R; // 動径 (地心距離)(AU)
} SOutparams;
//
typedef const int cint;
typedef const double cdouble;
typedef const SDate cSDate;
typedef const STime cSTime;
typedef const SInparams cSInparams;


//
// 2000 年 1 月 1.5 日のユリウス日 (J2000.0)
const ulong JC2000 = 2451545UL;
//
// ===================================================
// SolPos に関係する関数群の宣言
// ===================================================
//
void SetSTime( STime* aSTime, cdouble aHour = 0.0 );
// 実数で与えられた時刻 (aHour)(h)(デフォルト 0 時) を
// 用いて時刻構造体 (aSTime) を初期化する
// ただし，0.0 <= aHour < 24.0
// ---------------------------------------------------
void STtoTD( cSDate* aDate, cSTime* aTime,
cdouble RefTime,
SDate* TDDate, STime* TDTime );
// 標準時刻系で表された年月日 (aDate)，時刻 (aTime) を
// TCG 時刻系の年月日 (TDDate)，時刻 (TDTime) に変換する
// 標準時と世界時の時差は，RefTime で与える（日本: +9.0）
// ---------------------------------------------------
ulong JD( cSDate& aDate );
// SDate 構造体 (aDate) で与えた年月日のユリウス日を
// 計算して，unsigned long 値として返す
// ---------------------------------------------------
inline double JC( cdouble aJD )
{ return ( (aJD - JC2000) / 36525.0 ); };
// ユリウス日 (aJD)(実数であり，時刻に応じた端数有効) を
// J2000.0 を元期とするユリウス世紀数の換算して返す
// ---------------------------------------------------
double MAscens( cdouble aUTJC );
// UT 時刻系の J2000.0 元期のユリウス世紀数 (aJC) に対する
// 平均赤経を返す (返値は deg.単位)
// ---------------------------------------------------
long DT( cSDate* aDate, cdouble aUTTime );
// SDate 構造体 (aDate) で与えた年月日のある時刻
// aUTTime(UT 時系) の地心座標時 TCG を求める際の補正値
// (Delta T_1 = TCG - UTC) を返す (返値は msec.単位)
// ---------------------------------------------------
double CLongit( cdouble aJC, const bool IsAp = true );
// J2000.0 元期のユリウス世紀数 (aJC) に対する太陽黄経を
// 返す　 IsAp が true(デフォルト) のとき視黄経，
// false のとき平均黄経 (返値は deg.単位)
// ---------------------------------------------------
double Obliq( cdouble aJC, const bool IsPrc = true );
// J2000.0 元期のユリウス世紀数 (aJC) に対する黄道傾斜角
// を返す　 IsPrc が true(デフォルト) のとき章動を含み，
// false のときは章動を含まない平均黄道傾斜角とする


// (返値は deg.単位)
// ---------------------------------------------------
double Eq( cdouble aJC );
// J2000.0 元期のユリウス世紀数 (aJC) に対する分点差を
// 返す (返値は deg.単位)
// ---------------------------------------------------
double DecEqt( cSDate* aDate, cSTime* aTime,
cdouble aMAscens,
cdouble& Decl, double& Et );
// TCG 時刻系で年月日 (aDate) と時刻 (aTime)，および
// 平均赤経 (aMAscens) (deg.) を与えて，
// (視) 赤緯 Decl(deg.) と均時差 (deg.) を参照形式で返す
// また，返値として動径 (地心距離)(AU) を返す
// ---------------------------------------------------
double SolRadius( cdouble aJC );
// J2000.0 元期のユリウス世紀数 (aJC) に対する
// 動径 (地心距離) を返す (返値は AU 単位)
// ---------------------------------------------------
void SolPos( cSInparams* aInput, cSDate* aDate,
cdouble aTime, SOutparams* aOutput );
// 計算用入力パラメータ構造体 (aInput) で与えた地点に対して
// 標準時刻系で与えた年月日 (aDate) と時刻 (aTime) における
// 太陽位置を計算し，結果を計算結果 (出力) パラメータ構造体
// (aOutput) に入れて返す
// ---------------------------------------------------
#endif // end of flag SolPosH
// ===================================================
// SolPos.cpp - 太陽位置計算関数 SolPos その他の関数群
// ===================================================
//
#include <math.h>
#include "SolPos.h" // 前頁のヘッダーファイルをインクルード
// ---------------------------------------------------
cdouble _rpd = M_PI / 180.0;
cdouble ZERO = 1.0e-7; // ゼロと判断する基準値
// ---------------------------------------------------
//
void SolPos( cSInparams* aInput, cSDate* aDate,
cdouble aTime, SOutparams* aOutput )
{ // 計算用入力パラメータ構造体 (aInput) で与えた地点に対して
// 標準時刻系で与えた年月日 (aDate) と時刻 (aTime) における
// 太陽位置を計算し，結果を計算結果 (出力) パラメータ構造体
// (aOutput) に入れて返す
SDate TDDate;
STime STTime, TDTime;
double RefTime = aInput->RefLongit / 15.0;
SetSTime( &STTime, aTime );
STtoTD( aDate, &STTime, RefTime, &TDDate, &TDTime );
//
double jday = (double)JD( aDate );
jday += (aTime - RefTime - 12.0) / 24.0;
double Tu = JC( jday );
double Alpha_m = MAscens( Tu );
//
double Decl, Et, t, SinD, CosD, SinT, CosT;
aOutput->R = DecEqt( &TDDate, &TDTime, Alpha_m,
Decl, Et );
//
Decl *= _rpd;
SinD = sin( Decl ); CosD = cos( Decl );
aOutput->SinD = SinD; aOutput->CosD = CosD;
aOutput->Et = t = Et;
t += 15.0 * (aTime - 12.0)
+ (aInput->Longit - aInput->RefLongit);
aOutput->hAngle = t;
t *= _rpd;
SinT = sin( t ); CosT = cos( t );
//
double phi = aInput->Latit * _rpd;
double SinH, CosH, SinA, CosA;
SinH = sin( phi ) * SinD + cos( phi ) * CosD * CosT;
CosH = sqrt( 1.0 - SinH * SinH );
if ( ZERO > fabs( CosH ) ) {
// H = PI/2(90deg.), A = 0 とみなす
SinA = 0.0; CosA = 1.0; SinH = 1.0; CosH = 0.0;
}
else {
SinA = CosD * SinT / CosH;
CosA = sqrt( 1.0 - SinA * SinA );
// ZERO Div.回避のため, 式 (3) は使わない
}
aOutput->SinA = SinA; aOutput->CosA = CosA;
aOutput->SinH = SinH; aOutput->CosH = CosH;
return;
}
//
// ===================================================
// 内部使用の関数群の定義
// ===================================================
//
bool IsLeap( cint aYear )
{ // 西暦年 (aYear) がうるう年のとき true を，
// うるう年でないとき false を返す
if ( 1583 > aYear ) {
// グレゴリオ暦採用 (1582 年 10 月 15 日) 以前は
// うるう年を考慮しない
return ( false );
}
if ( 0 == (aYear % 400) ) {
// 400 年に一度のうるう年である
return ( true );
}
int ymod = aYear % 100; // 100 で除した余り
if ( 0 == ymod ) {
// YY00 年はうるう年ではない
return ( false );
}
return ( (0 == (ymod % 4)) ? true : false );
}
// ---------------------------------------------------
int SerialDay( cSDate* aDate )
{ // 日付構造体 (aDate) で与えた年月日から 1 月 1 日を 1 とする
// 通日を計算して返す (1-366)
// 引数 aDate が不正な場合，0 を返す
int DaysofMonth[12] = {
31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, };
if ( !aDate ) return ( 0 );
int y = aDate->YYYY;
int m = aDate->MM;
int d = aDate->DD;
if ( YEAR_MIN > y || 1 > m || 12 < m ||
1 > d ) return ( 0 );
DaysofMonth[1] += (IsLeap( y ) ? 1 : 0);
if ( DaysofMonth[m - 1] < d ) return ( 0 );
int sd = 0;
for ( int mi = 1; mi < m; mi++ ) {
sd += DaysofMonth[mi - 1];
}
return ( sd + d );
}
// ---------------------------------------------------
void SetSDate( SDate* aDate, cint aYear,
cint aSerialDay )
{ // ある年 (aYear) の通日 (aSerialDay) を与えて，
// 年月日を求め，日付構造体 (aDate) に入れて返す
// (デフォルトは 2006 年の通日 1)
int DaysofMonth[12] = {
31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, };
if ( !aDate ) return;
int y = aYear;
int sd = aSerialDay, d, m;
while ( 0 >= sd ) {
sd += 365 + (IsLeap( --y ) ? 1 : 0);
};
while ( 365 + (IsLeap( y ) ? 1 : 0) < sd ) {
sd -= 365 + (IsLeap( ++y ) ? 1 : 0);
};
if ( YEAR_MIN > y ) { // デフォルトの年を使用する
y = 2006;
}
DaysofMonth[1] += (IsLeap( y ) ? 1 : 0);
d = m = 0;
for ( int mi = 1; mi <= 12; mi++ ) {
d += DaysofMonth[mi - 1];
if ( d > sd ) {
m = mi;
d -= DaysofMonth[mi - 1];
d = sd - d;
break;
}
}
aDate->YYYY = y; aDate->MM = m; aDate->DD = d;
return;
}
//
// ===================================================
// 公開する関数群の定義
// ===================================================
//
void SetSTime( STime* aSTime, cdouble aHour )
{ // 実数で与えられた時刻 (aHour)(h)(デフォルト 0 時) を
// 用いて時刻構造体 (aSTime) を初期化する
// ただし，0.0 <= aHour < 24.0
if ( !aSTime ) return;
double h = aHour;
if ( 0.0 > h ) {
aTime->hh = 0U; aTime->mm = 0U; aTime->ss = 0.0;
}
else if (24.0 <= h ) {
aTime->hh = 23U; aTime->mm = 59U;
5
Copyright © 2016, Meteorological Data System, Co., Ltd. All Rights Reserved.
(TE-8J Ver.1, May 2016)
aTime->ss = 60.0 - ZERO;
return;
}
else {
// 引数が正常な場合
　　 int hh = int(h);
double res = 60.0 * (h - double(hh));
int mm = int(res);
res = 60.0 * (res - double(mm));
aTime->hh = (uint)hh;
aTime->mm = (uint)mm;
aTime->ss = res;
}
return;
}
// ---------------------------------------------------
void STtoTD( cSDate* aDate, cSTime* aTime,
cdouble aRefTime,
SDate* TDDate, STime* TDTime )
{ // 標準時刻系で表された年月日 (aDate)，時刻 (aTime) を
    // TCG 時刻系の年月日 (UTDate)，時刻 (UTime) に変換する
    // 標準時と世界時の時差は，aRefTime で与える（日本: +9.0）
    if ( !aDate || !aTime || !TDDate || !TDTime ) {
    return;
    }
    double r = aRefTime;
    if ( 12.0 < fabs ( r ) ) {
        r = ((aRefTime > 0) ? 12.0 : -12.0);
    }
    double uth = aTime->ss / 3600.0 + aTime->mm / 60.0
    + aTime->hh - r;
    if ( 0.0 > uth ) {
        // 標準時->UT1 時変換で日付が前日になる場合
        int sd = SerialDay( aDate ) - 1;
        SetSDate( TDDate, aDate->YYYY, sd );
        uth += 24.0;
        SetSTime( TDTime, uth );
    }
    else if ( 24.0 <= uth ) {
    // 標準時->UT1 時変換で日付が翌日になる場合
    int sd = SerialDay( aDate ) + 1;
        SetSDate( TDDate, aDate->YYYY, sd );
        uth -= 24.0;
        SetSTime( TDTime, uth );
    }
    else {
        *TDDate = *aDate;
        SetSTime( TDTime, uth );
    }
    // ここまでは，標準時 -> UT1 時変換
    //
    // dt [sec] を標準時正午について求める
    double dt = (double)DT( aDate, 12.0 - r );
    // UT1 時を dt で補正
    double tdh = (dt * 0.001 + TDTime->ss) / 3600.0
    + TDTime->mm / 60.0 + TDTime->hh;
    if ( 0.0 > tdh ) {
    // UT1 時->TD 時変換で日付が前日になる場合
        int sd = SerialDay( TDDate ) - 1;
        SetSDate( TDDate, TDDate->YYYY, sd );
        tdh += 24.0;
        SetSTime( TDTime, tdh );
    }
    else if ( 24.0 <= tdh ) {
    // UT1 時->TD 時変換で日付が翌日になる場合
        int sd = SerialDay( TDDate ) + 1;
        SetSDate( TDDate, TDDate->YYYY, sd );
        tdh -= 24.0;
        SetSTime( TDTime, tdh );
    }
    else {
        *TDDate = *aDate;
        SetSTime( TDTime, tdh );
    }
    return;
}
// ---------------------------------------------------
// グレゴリオ暦採用年月日参照用マクロ
#define LGREG(Y,M,D) ((D)+31L*((M)+12L*(Y)))
#define GREG0 LGREG(1582,10,15)
//
ulong JD( cSDate& aDate )
{ // SDate 構造体 (aDate) で与えた年月日のユリウス日を
    // 計算して，unsigned long 値として返す
    int ja, jm, jy = aDate.YYYY;
    long jul;
    long GREG1 = LGREG(aDate.YY, aDate.MM, aDate.DD);
    if ( 0 > jy ) ++jy;
    if ( 2 < int(aDate.MM) ) jm = (int)aDate.MM + 1;
    else {
        --jy; jm = (int)aDate.MM + 13;
    }
    jul = long(floor(365.25*jy) + floor(30.6001*jm)
    + aDate.DD) + 1720995L;
    if ( GREG0 <= GREG1 ) {
        ja = int(0.01*jy);
        jul += 2 - ja + int(0.25*ja);
    }
    return ( ulong(jul) );
}
// ---------------------------------------------------
double MAscens( cdouble aUTJC )
{ // UT 時刻系の JC2000.0 元期のユリウス世紀数 (aJC) に対する
    // 平均赤経を返す (返値は deg.単位)
    static cdouble A[4] = { 67310.54841,
    8640184.812866, 0.093104, -0.0000062, };
    double Tu = aUTJC;
    double asec = ((A[3] * Tu + A[2]) * Tu + A[1]) * Tu
    + A[0]; // in [s]
    return ( asec / 240.0 ); // 15/3600=1/240[deg./s]
}
// ---------------------------------------------------
long DT( cSDate* aDate, cdouble aUTTime )
{ // SDate 構造体 (aDate) で与えた年月日のある時刻
    // aUTTime(UT 時系) の力学時を求める際の 補正値
    // (Delta T_1 = TD - UT1) を返す (返値は msec.単位)
    double jday = (double)JD( *aDate ) // 12UT1 のとき
    + (aUTTime - 12.0) / 24.0;
    double Tu = JC( jday );
    double dt = - 0.311; // in [s]
    double dp = 1.0 + 0.2605601 * exp( -4.423790 * Tu );
    dt += 80.84308 / dp;
    dt *= 1000.0; // in [ms]
    dt += 0.5; // 四捨五入のため
    return ( long(dt) );
}
// ---------------------------------------------------
typedef struct {
    bool T;
    double P, Q, R;
} SCoef;
#define PCOEF_MAX (18)
const SCoef PCoef[PCOEF_MAX] = {
    { false, 1.9147, 35999.05, 267.52, }, // 1
    { false, 0.0200, 71998.10, 265.10, }, // 2
    { false, 0.0020, 32964.00, 158.00, }, // 3
    { false, 0.0018, 19.00, 159.00, }, // 4
    { false, 0.0018, 445267.00, 208.00, }, // 5
    { false, 0.0015, 45038.00, 254.00, }, // 6
    { false, 0.0013, 22519.00, 352.00, }, // 7
    { false, 0.0007, 65929.00, 45.00, }, // 8
    { false, 0.0007, 3035.00, 110.00, }, // 9
    { false, 0.0007, 9038.00, 64.00, }, // 10
    { false, 0.0006, 33718.00, 316.00, }, // 11
    { false, 0.0005, 155.00, 118.00, }, // 12
    { false, 0.0005, 2281.00, 221.00, }, // 13
    { false, 0.0004, 29930.00, 48.00, }, // 14
    { false, 0.0004, 31557.00, 161.00, }, // 15
    { true, -0.0048, 35999.00, 268.00, }, // 16*
    { false, 0.0048, 1934.00, 145.00, }, // 17
    { false, -0.0004, 72002.00, 111.00, }, // 18
};
//
double CLongit( cdouble aJC, const bool IsAp )
{ // JD2000.0 元期のユリウス世紀数 (aJC) に対する太陽黄経を
    // 返す　 IsAp が true(デフォルト) のとき視黄経，
    // false のとき平均黄経 (返値は deg.単位)
    double T = aJC;
    double psi = 0.0, qtr, p;
    int icmax = (IsAP ? PCOEF_MAX : PCOEF_MAX - 2);
    for ( int ic = icmax; ic >= 1; ic-- ) {
    qtr = fmod( PCoef[ic - 1].Q * T
    + PCoef[ic - 1].R, 360.0 ) * _rpd;
    p = PCoef[ic - 1].P;
    if ( PCoef[ic - 1].T ) p *= T;
        psi += p * cos( qtr );
    }
    if ( IsAP ) psi -= 0.00569;
    psi += 36000.7695 * T + 280.4602;
    return ( psi );
}
// ---------------------------------------------------
double Obliq( cdouble aJC, const bool IsPrc )
{ // JD2000.0 元期のユリウス世紀数 (aJC) に対する黄道傾斜角
    // を返す　 IsPrc が true(デフォルト) のとき章動を含み，
    // false のときは章動を含まない平均黄道傾斜角とする
    // (返値は deg.単位)
    static cdouble E[4] = { -89381.448000, 46.815000,
    0.000590, -0.001813, };
    double T = aJC;
    double eps = 0.0, qtr, epss;
    if ( IsPrc ) {
        qtr = fmod( 72002.0 * T + 201.0, 360.0 ) * _rpd;
        eps -= 0.00015 * cos( qrt );

        qtr = fmod( 1934.0 * T + 235.0, 360.0 ) * _rpd;
        eps -= 0.00256 * cos( qrt );
    }
    epss = ((E[3] * T + E[2]) * T + E[1]) * T + E[0];
    eps += epss / 3600.0; // in [deg.]
    return ( eps );
}
// ---------------------------------------------------
#define RCOEF_MAX (9)
const SCoef RCoef[RCOEF_MAX] = {
    { false, 1.000140, 0.00, 0.00, }, // 1
    { false, 0.016706, 35,999.05, 177.53, }, // 2
    { false, 0.000139, 71,998.00, 175.00, }, // 3
    { false, 0.000031, 445,267.00, 298.00, }, // 4
    { false, 0.000016, 32,964.00, 68.00, }, // 5
    { false, 0.000016, 45,038.00, 164.00, }, // 6
    { false, 0.000005, 22,519.00, 233.00, }, // 7
    { false, 0.000005, 33,718.00, 226.00, }, // 8
    { true, -0.000042, 35,999.00, 178.00, }, // 9*
    };
//
double SolRad( cdouble aJC )
{ // JC2000.0 元期のユリウス世紀数 (aJC) に対する
    // 動径 (地心距離) を返す (返値は AU 単位)
    double T = aJC;
    double r = 0.0, qtr, p;
    int icmax = RCOEF_MAX;
    for ( int ic = icmax; ic >= 2; ic-- ) {
    qtr = fmod( RCoef[ic - 1].Q * T
    + RCoef[ic - 1].R, 360.0 ) * _rpd;
    p = RCoef[ic - 1].P;
    if ( RCoef[ic - 1].T ) p *= T;
        r += p * cos( qtr );
    }
    r += RCoef[0].P;
    return ( r );
}
// ---------------------------------------------------
double Eq( cdouble aJC )
{ // JD2000.0 元期のユリウス世紀数 (aJC) に対する分点差を返す
    // (返値は deg.単位)
    double eps = Obliq( aJC, true );
    eps = fmod( eps, 360.0 ) * _rpd;
    double dpsi = 0.0, qtr, p;
    qtr = fmod( SCoef[SCOEF_MAX - 1].Q * T
    + SCoef[SCOEF_MAX - 1].R, 360.0 ) *_rpd;
    p = SCoef[SCOEF_MAX - 1].P;
    dpsi += p * cos( qtr );
    qtr = fmod( SCoef[SCOEF_MAX - 2].Q * T
    + SCoef[SCOEF_MAX - 2].R, 360.0 ) *_rpd;
    p = SCoef[SCOEF_MAX - 2].P;
    dpsi += p * cos( qtr ) - 0.00569;
    return ( dpsi * cos( eps ) );
}
// ---------------------------------------------------
double DecEqt( cSDate* aDate, cSTime* aTime,
cdouble aMAscens,
cdouble& Decl, double& Et )
{ // TD 時刻系で年月日 (aDate) と時刻 (aTime)，および
    // 平均赤経 (aMAscens) (deg.) を与えて，
    // (視) 赤緯 Decl(deg.) と均時差 (deg.) を参照形式で返す
    // また，返値として動径 (地心距離)(AU) を返す
    double jday = (double)JD( *aDate );
    double hour = aTime->hh + aTime->mm / 60.0
    + aTime->ss / 3600.0;
    jday += (hour - 12.0) / 24.0;
    double T = JC( jday );
    double eps = Obliq( T, true );
    eps = fmod( eps, 360.0 ) * _rpd;
    double psi = CLongit( T, true );
    psi = fmod( psi, 360.0 ) * _rpd;
    double eq = Eq( T );
    double sind = sin( psi ) * sin( eps );
    double cosd = sqrt( 1.0 - sind * sind );
    double tanam = tan( aMAscens * _rpd );
    double tpce = tan( psi ) * cos( eps );
    double dAsc = atan( (tanam - tpce)
    / (1.0 + tanam * tpce) ) / _rpd;
    Et = fmod( eq + dAsc, 360.0 );
    Decl = atan( sind / cosd ) / _rpd; // rad.->deg.
    return ( SolRadius( T ) );
}
/* */



    /*
     * 以下のように使用
        public Taiyo.SInparams aInput = new Taiyo.SInparams();
        public Taiyo.SOutparams aOutput = new Taiyo.SOutparams();
        public Taiyo taiyo= new Taiyo();
    
     */

    public class Taiyo
    {
        public const int YEAR_MIN = 1971;

        // 日付構造体
        public struct SDate
        {
            public uint YYYY; // 年 (1971 <= YYYY)
            public uint MM; // 月 (1 <= MM <= 12)
            public uint DD; // 日 (1 <= DD <= 28|29|30|31)
        };
        //  SDate;

        public struct STime
        {
            public uint hh;
            public uint mm;
            public double ss; 
        };
        // 太陽位置計算用入力パラメータ構造体
        public struct SInparams
        {
            public double Latit; // 計算対象地点の緯度 (deg.), +は北緯
            public double Longit; // 計算対象地点の経度 (deg.), +は東経
            public double RefLongit; // 計算対象地点の標準時経度 (deg.),
                                     // +は東経（日本: 135.0）
        };// SInparams;
          //
          // 太陽位置計算結果 (出力) パラメータ構造体
        public struct SOutparams
        {
            public double SinH, CosH; // 太陽高度角の正弦，余弦
            public double SinA, CosA; // 太陽方位角の正弦，余弦
            public double SinD, CosD; // 視赤緯の正弦，余弦
            public double Et, hAngle; // 均時差 (deg.) と時角 (deg.)
            public double R; // 動径 (地心距離)(AU)
        };// SOutparams;

        //
        // 2000 年 1 月 1.5 日のユリウス日 (J2000.0)
        const ulong JC2000 = 2451545UL;
        //
        // ===================================================
        // SolPos に関係する関数群の宣言
        // ===================================================

        double _rpd = Math.PI / 180.0;
        double ZERO = 1.0e-7; // ゼロと判断する基準値

        // ---------------------------------------------------
        // J2000.0 元期のユリウス世紀数 (aJC) に対する
        // 動径 (地心距離) を返す (返値は AU 単位)
        // ---------------------------------------------------
        double SolRadius(double aJC)
        {
  //          Console.WriteLine("SolRadius " + aJC.ToString());
            return 1.0;
        }

        public void SolPos(SInparams aInput, DateTime aDateTime, ref SOutparams aOutput)
        { // 計算用入力パラメータ構造体 (aInput) で与えた地点に対して
          // 標準時刻系で与えた年月日 (aDate) と時刻 (aTime) における
          // 太陽位置を計算し，結果を計算結果 (出力) パラメータ構造体
          // (aOutput) に入れて返す
            SDate TDDate;
            STime STTime, TDTime;
            DateTime TDDateTime;
            double RefTime = aInput.RefLongit / 15.0;
            
            double aTime = aDateTime.TimeOfDay.TotalHours;
            SetSTime(out STTime, aTime);

    //        Console.WriteLine(aTime.ToString());

            SDate aDate;
            aDate.YYYY = (uint)aDateTime.Year;
            aDate.MM = (uint)aDateTime.Month;
            aDate.DD = (uint)aDateTime.Day;

         //   aDate.MM = 3;
         //   aDate.DD = 21;

            STtoTD(ref aDate,ref STTime, RefTime, out TDDate,out TDTime);
            //
            double jday = (double)JD(aDate);

            jday += (aTime - RefTime+12) / 24.0;
            double Tu = JC(jday);
            double Alpha_m = MAscens(Tu);
            //
            double Decl, Et, t, SinD, CosD, SinT, CosT;
            aOutput.R = DecEqt(TDDate, TDTime, Alpha_m, out Decl, out Et);
            //
            Decl *= _rpd;
            SinD = Math.Sin(Decl); CosD = Math.Cos(Decl);
            aOutput.SinD = SinD; aOutput.CosD = CosD;
            aOutput.Et = t = Et;
            aTime = 0;
        //    t += 15.0 * (aTime - 12.0) + (aInput.Longit - aInput.RefLongit);

            aOutput.hAngle = t+Alpha_m;
            //t *= _rpd;
            //SinT = Math.Sin(t); CosT = Math.Cos(t);
            ////
            //double phi = aInput.Latit * _rpd;
            //double SinH, CosH, SinA, CosA;
            //SinH = Math.Sin(phi) * SinD + Math.Cos(phi) * CosD * CosT;
            //
            //CosH = Math.Sqrt(1.0 - SinH * SinH);
            ////  if (ZERO > Math.Abs(CosH))
            ////  {
            //// H = PI/2(90deg.), A = 0 とみなす
            ////     SinA = 0.0; CosA = 1.0; SinH = 1.0; CosH = 0.0;
            //
            //SinA = CosD * SinT / CosH;
            //CosA = Math.Sqrt(1.0 - SinA * SinA);
            //if (SinH < SinD)
            //    CosA = -CosA;
            //// ZERO Div.回避のため, 式 (3) は使わない
            ////        }
            //aOutput.SinA = SinA; aOutput.CosA = CosA;
            //aOutput.SinH = SinH; aOutput.CosH = CosH;
            return;
        }
        //
        // ===================================================
        // 内部使用の関数群の定義
        // ===================================================
        //
        bool IsLeap(int aYear)
        { // 西暦年 (aYear) がうるう年のとき true を，
          // うるう年でないとき false を返す
            if (1583 > aYear)
            {
                // グレゴリオ暦採用 (1582 年 10 月 15 日) 以前は
                // うるう年を考慮しない
                return (false);
            }
            if (0 == (aYear % 400))
            {
                // 400 年に一度のうるう年である
                return (true);
            }
            int ymod = aYear % 100; // 100 で除した余り
            if (0 == ymod)
            {
                // YY00 年はうるう年ではない
                return (false);
            }
            return ((0 == (ymod % 4)) ? true : false);
        }
        // ---------------------------------------------------
        int SerialDay(SDate aDate)
        { // 日付構造体 (aDate) で与えた年月日から 1 月 1 日を 1 とする
          // 通日を計算して返す (1-366)
          // 引数 aDate が不正な場合，0 を返す
            int[] DaysofMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            //    if (aDate != null) return (0);
            int y = (int)aDate.YYYY;
            int m = (int)aDate.MM;
            int d = (int)aDate.DD;
            if (YEAR_MIN > y || 1 > m || 12 < m || 1 > d) return (0);
            DaysofMonth[1] += (IsLeap(y) ? 1 : 0);
            if (DaysofMonth[m - 1] < d) return (0);
            int sd = 0;
            for (int mi = 1; mi < m; mi++)
            {
                sd += DaysofMonth[mi - 1];
            }
            return (sd + d);
        }
        // ---------------------------------------------------
        void SetSDate(out SDate aDate, int aYear, int aSerialDay)
        { // ある年 (aYear) の通日 (aSerialDay) を与えて，
          // 年月日を求め，日付構造体 (aDate) に入れて返す
          // (デフォルトは 2006 年の通日 1)
            int[] DaysofMonth = new int[12] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, };
            //    if (!aDate) return;
            int y = aYear;
            int sd = aSerialDay, d, m;
            while (0 >= sd)
            {
                sd += 365 + (IsLeap(--y) ? 1 : 0);
            };
            while (365 + (IsLeap(y) ? 1 : 0) < sd)
            {
                sd -= 365 + (IsLeap(++y) ? 1 : 0);
            };
            if (YEAR_MIN > y)
            { // デフォルトの年を使用する
                y = 2006;
            }
            DaysofMonth[1] += (IsLeap(y) ? 1 : 0);
            d = m = 0;
            for (int mi = 1; mi <= 12; mi++)
            {
                d += DaysofMonth[mi - 1];
                if (d > sd)
                {
                    m = mi;
                    d -= DaysofMonth[mi - 1];
                    d = sd - d;
                    break;
                }
            }
            aDate.YYYY = (uint)y; aDate.MM = (uint)m; aDate.DD = (uint)d;
            return;
        }
        //
        // ===================================================
        // 公開する関数群の定義
        // ===================================================
        //
        void SetSTime(out STime aTime, double aHour)
        { // 実数で与えられた時刻 (aHour)(h)(デフォルト 0 時) を
          // 用いて時刻構造体 (aSTime) を初期化する
          // ただし，0.0 <= aHour < 24.0
          //    if (!aSTime) return;
            double h = aHour;
            if (0.0 > h)
            {
                aTime.hh = 0U; aTime.mm = 0U; aTime.ss = 0;

            }
            else if (24.0 <= h)
            {
                 aTime.hh = 23U; aTime.mm = 59U;
                 aTime.ss = 60.0 - ZERO;

                return;
            }
            else
            {
                // 引数が正常な場合
                 int hh = (int)(h);
                 double res = 60.0 * (h - (double)(hh));
                 int mm = (int)(res);
                 res = 60.0 * (res - (double)(mm));
                 aTime.hh = (uint)hh;
                 aTime.mm = (uint)mm;
                 aTime.ss = res;


            }
            return;
        }
        // ---------------------------------------------------
        void STtoTD(ref SDate aDate, ref STime aTime, double aRefTime,
            out SDate TDDate, out STime TDTime)
        {
            // 標準時刻系で表された年月日 (aDate)，時刻 (aTime) を
            // TCG 時刻系の年月日 (UTDate)，時刻 (UTime) に変換する
            // 標準時と世界時の時差は，aRefTime で与える（日本: +9.0）
            // if (!aDate || !aTime || !TDDate || !TDTime)
            // {
            //     return;
            // }
            double r = aRefTime;//+9 for Japan
            if (12.0 < Math.Abs(r))
            {
                r = ((aRefTime > 0) ? 12.0 : -12.0);
            }

            double uth = aTime.ss / 3600.0 + aTime.mm / 60.0
            + aTime.hh - r;
            if (0.0 > uth)
            {
                // 標準時->UT1 時変換で日付が前日になる場合
                int sd = SerialDay(aDate) - 1;
                SetSDate(out TDDate, (int)aDate.YYYY, sd);
                uth += 24.0;
                SetSTime(out TDTime, uth);
            }
            else if (24.0 <= uth)
            {
                // 標準時->UT1 時変換で日付が翌日になる場合
                int sd = SerialDay(aDate) + 1;
                SetSDate(out TDDate, (int)aDate.YYYY, sd);
                uth -= 24.0;
                SetSTime(out TDTime, uth);
            }
            else
            {
                TDDate = aDate;
                SetSTime(out TDTime, uth);
            }
            //    TDDatetime = aDateTime.AddHours(-aRefTime);

            // ここまでは，標準時 -> UT1 時変換
            //
            // dt [sec] を標準時正午について求める
            double dt = (double)DT(aDate, 12.0 - r);
            // UT1 時を dt で補正
            double tdh = (dt * 0.001 + TDTime.ss) / 3600.0
            + TDTime.mm / 60.0 + TDTime.hh;
            if (0.0 > tdh)
            {
                // UT1 時->TD 時変換で日付が前日になる場合
                int sd = SerialDay(TDDate) - 1;
                SetSDate(out TDDate, (int)TDDate.YYYY, sd);
                tdh += 24.0;
                SetSTime(out TDTime, tdh);
            }
            else if (24.0 <= tdh)
            {
                // UT1 時->TD 時変換で日付が翌日になる場合
                int sd = SerialDay(TDDate) + 1;
                SetSDate(out TDDate, (int)TDDate.YYYY, sd);
                tdh -= 24.0;
                SetSTime(out TDTime, tdh);
            }
            else
            {
                TDDate = aDate;
                SetSTime(out TDTime, tdh);
            }

            return;
        }
        // ---------------------------------------------------
        // グレゴリオ暦採用年月日参照用マクロ
        //#define LGREG(Y,M,D) ((D)+31L*((M)+12L*(Y)))
        //#define GREG0 LGREG(1582,10,15)
        //        //
        //        ulong JDorg(cSDate& aDate )
        //        { // SDate 構造体 (aDate) で与えた年月日のユリウス日を
        //          // 計算して，unsigned long 値として返す
        //            int ja, jm, jy = aDate.YYYY;
        //            long jul;
        //            long GREG1 = LGREG(aDate.YY, aDate.MM, aDate.DD);
        //            if (0 > jy) ++jy;
        //            if (2 < int(aDate.MM)) jm = (int)aDate.MM + 1;
        //            else
        //            {
        //                --jy; jm = (int)aDate.MM + 13;
        //            }
        //            jul = long(floor(365.25 * jy) + floor(30.6001 * jm)
        //            + aDate.DD) + 1720995L;
        //            if (GREG0 <= GREG1)
        //            {
        //                ja = int(0.01 * jy);
        //                jul += 2 - ja + int(0.25 * ja);
        //            }
        //            return (ulong(jul));
        //        }

        ulong JD(SDate date)
        {
            int jy = (int)date.YYYY;
            int ja, jm;
            long jul;
            long GREG1 = (date.YYYY * 12 + date.MM) * 31 + date.DD;
            if (0 > jy) ++jy;
            if (2 < date.MM)
                jm = (int)date.MM + 1;
            else
            {
                --jy; jm = (int)date.MM + 13;
            }
            jul = (long)(Math.Floor(365.25 * jy) + Math.Floor(30.6001 * jm) + date.DD) + 1720995L;
            if (((1582 * 12 + 10) * 31 + 15) <= GREG1)
            {
                ja = (int)(0.01 * jy);
                jul += 2 - ja + (int)(0.25 * ja);
            }
            return (ulong)jul;
        }

        // ---------------------------------------------------
        double JC(double aJD)
        { return ((aJD - JC2000) / 36525.0); }

        // ユリウス日 (aJD)(実数であり，時刻に応じた端数有効) を
        // J2000.0 を元期とするユリウス世紀数の換算して返す
        // ---------------------------------------------------

        // ---------------------------------------------------
        double MAscens(double aUTJC)
        { // UT 時刻系の JC2000.0 元期のユリウス世紀数 (aJC) に対する
          // 平均赤経を返す (返値は deg.単位)
            double[] A = new double[4] { 67310.54841, 8640184.812866, 0.093104, -0.0000062, };
            double Tu = aUTJC;
            double asec = ((A[3] * Tu + A[2]) * Tu + A[1]) * Tu + A[0]; // in [s]
            return asec / 240.0; // 15/3600=1/240[deg./s]
        }
        // ---------------------------------------------------
        long DT(SDate aDate, double aUTTime)
        { // SDate 構造体 (aDate) で与えた年月日のある時刻
          // aUTTime(UT 時系) の力学時を求める際の 補正値
          // (Delta T_1 = TD - UT1) を返す (返値は msec.単位)
            double jday = (double)JD(aDate) // 12UT1 のとき
            + (aUTTime - 12.0) / 24.0;
            double Tu = JC(jday);
            double dt = -0.311; // in [s]
            double dp = 1.0 + 0.2605601 * Math.Exp(-4.423790 * Tu);
            dt += 80.84308 / dp;
            dt *= 1000.0; // in [ms]
            dt += 0.5; // 四捨五入のため
            return (long)dt;
        }
        // ---------------------------------------------------
        public struct SCoef
        {
            public bool T;
            public double P, Q, R;
            public SCoef(bool _T, double _P, double _Q, double _R)
            {
                T = _T;
                P = _P;
                Q = _Q;
                R = _R;
            }
        };// SCoef;

        const int PCOEF_MAX = 18;

        SCoef[] PCoef = new SCoef[PCOEF_MAX] {
            new SCoef( false, 1.9147, 35999.05, 267.52 ), // 1
            new SCoef( false, 0.0200, 71998.10, 265.10 ), // 2
            new SCoef( false, 0.0020, 32964.00, 158.00 ), // 3
            new SCoef( false, 0.0018,    19.00, 159.00 ), // 4
            new SCoef( false, 0.0018,445267.00, 208.00 ), // 5
            new SCoef( false, 0.0015, 45038.00, 254.00 ), // 6
            new SCoef( false, 0.0013, 22519.00, 352.00 ), // 7
            new SCoef( false, 0.0007, 65929.00,  45.00 ),  // 8
            new SCoef( false, 0.0007,  3035.00, 110.00 ), // 9
            new SCoef( false, 0.0007,  9038.00,  64.00 ), // 10
            new SCoef( false, 0.0006, 33718.00, 316.00 ),// 11
            new SCoef( false, 0.0005,   155.00, 118.00 ),// 12
            new SCoef( false, 0.0005,  2281.00, 221.00 ),// 13
            new SCoef( false, 0.0004, 29930.00,  48.00 ), // 14
            new SCoef( false, 0.0004, 31557.00, 161.00 ),// 15
            new SCoef( true, -0.0048, 35999.00, 268.00 ),// 16*
            new SCoef( false, 0.0048,  1934.00, 145.00 ),// 17
            new SCoef( false,-0.0004, 72002.00, 111.00 ) // 18
        };
        //
        double CLongit(double aJC, bool IsAP)
        { // JD2000.0 元期のユリウス世紀数 (aJC) に対する太陽黄経を
          // 返す　 IsAp が true(デフォルト) のとき視黄経，
          // false のとき平均黄経 (返値は deg.単位)
            double T = aJC;
            double psi = 0.0, qtr, p;
            int icmax = (IsAP ? PCOEF_MAX : PCOEF_MAX - 2);
            for (int ic = icmax; ic >= 1; ic--)
            {
                /////////////////////////////////?????? ? +?-? ???????/////////////////
                qtr = ((PCoef[ic - 1].Q * T + PCoef[ic - 1].R) % 360.0) * _rpd;
                p = PCoef[ic - 1].P;
                if (PCoef[ic - 1].T) p *= T;
                psi += p * Math.Cos(qtr);
            }
            if (IsAP) psi -= 0.00569;
            psi += 36000.7695 * T + 280.4602;
            return (psi);
        }
        // ---------------------------------------------------
        double Obliq(double aJC, bool IsPrc)
        { // JD2000.0 元期のユリウス世紀数 (aJC) に対する黄道傾斜角
          // を返す　 IsPrc が true(デフォルト) のとき章動を含み，
          // false のときは章動を含まない平均黄道傾斜角とする
          // (返値は deg.単位)
            double[] E = new double[4] { -89381.448000, 46.815000, 0.000590, -0.001813, };
            double T = aJC;
            double eps = 0.0, qtr, epss;
            if (IsPrc)
            {
                qtr = ((72002.0 * T + 201.0) % 360.0) * _rpd;
                eps -= 0.00015 * Math.Cos(qtr);

                qtr = ((1934.0 * T + 235.0) % 360.0) * _rpd;
                eps -= 0.00256 * Math.Cos(qtr);
            }
            epss = ((E[3] * T + E[2]) * T + E[1]) * T + E[0];
            eps += epss / 3600.0; // in [deg.]
            return (eps);
        }
        // ---------------------------------------------------
        //#define RCOEF_MAX (9)
        const int RCOEF_MAX = 9;
        SCoef[] RCoef = new SCoef[RCOEF_MAX] {
            new SCoef( false, 1.000140,     0.00, 0.00 ), // 1
            new SCoef( false, 0.016706, 35999.05, 177.53 ), // 2
            new SCoef( false, 0.000139, 71998.00, 175.00 ), // 3
            new SCoef( false, 0.000031,445267.00, 298.00), // 4
            new SCoef( false, 0.000016, 32964.00, 68.00   ), // 5
            new SCoef( false, 0.000016, 45038.00, 164.00 ), // 6
            new SCoef( false, 0.000005, 22519.00, 233.00 ), // 7
            new SCoef( false, 0.000005, 33718.00, 226.00 ), // 8
            new SCoef( true, -0.000042, 35999.00, 178.00 ), // 9*
        };
        //
        double SolRad(double aJC)
        { // JC2000.0 元期のユリウス世紀数 (aJC) に対する
          // 動径 (地心距離) を返す (返値は AU 単位)
            double T = aJC;
            double r = 0.0, qtr, p;
            int icmax = RCOEF_MAX;
            for (int ic = icmax; ic >= 2; ic--)
            {
                qtr = ((RCoef[ic - 1].Q * T + RCoef[ic - 1].R) % 360.0) * _rpd;
                p = RCoef[ic - 1].P;
                if (RCoef[ic - 1].T) p *= T;
                r += p * Math.Cos(qtr);
            }
            r += RCoef[0].P;
            return (r);
        }
        // ---------------------------------------------------
        double Eq(double aJC)
        { // JD2000.0 元期のユリウス世紀数 (aJC) に対する分点差を返す
            double eq = 0.00029*15 * Math.Sin((1.934 * aJC + 235) * _rpd);
            return eq;
            //    // (返値は deg.単位)
            //      double eps = Obliq(aJC, true);
            //      eps = (eps% 360.0) * _rpd;
            //      double dpsi = 0.0, qtr, p;
            //      qtr = ((SCoef[SCOEF_MAX - 1].Q * T+ SCoef[SCOEF_MAX - 1].R) % 360.0) * _rpd;
            //      p = SCoef[SCOEF_MAX - 1].P;
            //      dpsi += p * Math.Cos(qtr);
            //      qtr = ((SCoef[SCOEF_MAX - 2].Q * T + SCoef[SCOEF_MAX - 2].R)% 360.0) * _rpd;
            //      p = SCoef[SCOEF_MAX - 2].P;
            //      dpsi += p * Math.Cos(qtr) - 0.00569;
            //      return (dpsi * Math.Cos(eps));
        }
        // ---------------------------------------------------
        double DecEqt(SDate aDate,STime aTime ,double aMAscens, out double Decl, out double Et)
        {
            // TD 時刻系で年月日 (aDate) と時刻 (aTime)，および
            // 平均赤経 (aMAscens) (deg.) を与えて，
            // (視) 赤緯 Decl(deg.) と均時差 (deg.) を参照形式で返す
            // また，返値として動径 (地心距離)(AU) を返す
            double jday = (double)JD(aDate);
   //         double jday = Cel.JDNow();
            double hour = aTime.hh + aTime.mm / 60.0 + aTime.ss / 3600.0;
            jday += (hour - 12.0) / 24.0;
            double T = JC(jday);
            double eps = Obliq(T, true);
            eps = (eps % 360.0) * _rpd;
            double psi = CLongit(T, true);
            psi = (psi % 360.0) * _rpd;
            double eq =  Eq(T);
            double sind = Math.Sin(psi) * Math.Sin(eps);
            double cosd = Math.Sqrt(1.0 - sind * sind);
            double tanam = Math.Tan(aMAscens * _rpd);
            double tpce = Math.Tan(psi) * Math.Cos(eps);
            double dAsc = Math.Atan2((tanam - tpce), (1.0 + tanam * tpce)) / _rpd;
            Et = (eq + dAsc) % 360.0;
    //        Decl = Math.Atan(sind / cosd) / _rpd; // rad.->deg.
            Decl = Math.Atan2(sind, cosd) / _rpd; // rad.->deg.
            return SolRadius(T);
        }

    }
}
