
/*  STM32F103 PIN
//////////////USB top Left SIDE
  PB12
  PB13
  PB14
  PB15
  PA8
  PA9
  PA10
  PA11
  PA12
  PA13
  PB3
  PB4
  PB5
  PB6
  PB7
  PB8
  PB9
  5V
  GND
  3.3V

//////////////USB top Right SIDE
          GND
          GND
          3.3V
          RESET
          PB11
          PB10
          PB1
          PB0
          PA7
          PA6
          PA5
          PA4
          PA3
          PA2
          PA1
          PA0
          PC15
          PC14
          PC13

/////////       STM32F103 PIN OUT             
			        	PB12	[USB]   #GND
				SCK2		PB13	    		#GND
				MISO		PB14	    		#33V
				MOSI		PB15	    		#RES
        				PA8 		      PB11		SDA2
	        			PA9 		 	    PB10		SCL2
	        			PA10	    		PB1
				USB 		PA11	    		PB0
				USB			PA12	    		PA7			MOSI1
	        			PA15	    		PA6			MISO1
				SCK	1		PB3 		      PA5			SCK 1
				MISO1		PB4 		      PA4		
				MOSI1		PB5 		      PA3
				SCL2		PB6 		      PA2		
				SDA2		PB7 		      PA1
				SCL1		PB8 	  	    PA0
				SDA1		PB9 	  	    PC15
	        			#5V 		      PC14
	        			#GND			    PC13
	        			#33V	STLINK	#VBAT
////////////

///////////      STM32F103 PIN CONNECT   ///////////
OUT1	           	PB12	[USB]   #GND    ---------
XXX   	  SCK2		PB13	    		#GND    ---------
IGN   	  MISO2		PB14	    		#33V    ---------
PASS  	  MOSI2		PB15	    		#RES    ---------
HI/LOW  	  			PA8 		      PB11		SDA2    PickupPulse
	TX        			PA9 		 	    PB10		SCL2    ##9250_CS     
	Rx        			PA10	    		PB1             an_Rpot 
          USB 		PA11	    		PB0             an_TO 
      	  USB	    PA12	    		PA7			MOSI1   ##SSD1306_SDA   SD_MOSI 9250?
OUT0(Change) 			PA15	    		PA6			MISO1                   SD_MISO 9250?
Change	  SCK	1		PB3 		      PA5			SCK 1   SSDSCK          SD_SCK  9250?
Horn  	  MISO1		PB4 		      PA4		          ACC Volt 1:6
TL  		  MOSI1		PB5 		      PA3             an_O2
TR			  SCL2		PB6 		      PA2		          an_Fpot
GP4   	  SDA2		PB7 		      PA1             an_AirP
GP2			  SCL1		PB8 	  	    PA0             an_Pickup  0V=+2.5V  1.65V=0V  3.3V=-2.5V
GP1			  SDA1		PB9 	  	    PC15            ##              SSD1306_DC
	    --------		#5V 		      PC14            ##              SSD1306_CS
	    --------		#GND			    PC13            ##              SD_CS
	    --------		#33V	STLINK	#VBAT
///////////                              ////////////


annalog Range
33k:20K

ref 150:100->2.0V r40を50kに変更

-1.055V.....3.3V
input    0V 33k 1.666V  20K (1.666V+1.0101V)=0.656V   2.67V   828
input 1.666V 33k 1.666V  20K 1.6666V      1.666
input  3.33V 33k 1.666V  20K (1.666V+1.0101V)=3.687V 0.65V
input    4V 33k 1.666V  20K (1.666V-1.414)=3.687V 0.25V   77
4.389V.....0V
input    5V 33k 1.666V  20K (1.666V-2.0202V)=3.687V -0.35V


input    0V 33k 2V  20K (2V+1.212V)   3.212V
input    2V 33k 2V  20K 2V      2
input    5V 33k 2V  20K (2V-1.818V)=0.1818V

10bit
1024 =3.3V

// SSD1306 PINOUT
  GND VDD SCK SDA RES DC CS
  |                       |
  |       SSD1306         |
  |                       |
  ~~~~~~~~~~~~~~~~~~~~~~~~~
//
*/

/*

　  データログ開始 　走行時自動開始
    データログ終了  

    クイックシフターON
    クイックシフターOFF　OFF
    QS回転数モード      右
    ギア比モード      　左
    ピットロード        左右

*/


/*

110型　250型

住友電装 090型 HM 非防水 10極 カプラー 
メーター
10　F　M 10  本体 
10　F　M 10  
 8　F　M 8 


サイドスタンド
 住友電装 090型 MT防水コネクターセット 3極　メスのみ180
スタンド　3F　M3  本体


O2センサー
住友電装 090型 MT 防水 4極 カプラー・端子セット 黒色タイプ　￥549 税込
センサー　4F　4Mハーネス

パルス
住友電装 090型 MT 防水 2極 カプラー・端子セット 黒色 タイプ3(アームロック)　￥488 税込
センサー　2F　2Mハーネス


2024/10/10　発注コネクタ
住友電装 060型 HX 防水 8極 カプラー・端子セット￥660￥660 税込
住友電装 060型 TS 非防水 12極 カプラー・端子セット￥767￥767 税込
住友電装 090型 HM 非防水 6極 カプラー・端子セット￥529￥529 税込
住友電装 090型 MT 防水 3極 メスカプラー・端子セット 三角タイプ￥180￥180 税込
住友電装 090型 HM 非防水 10極 カプラー・端子セット 黒色￥774￥774 税込
住友電装 090型 HM 非防水 10極 カプラー・端子セット￥708￥708 税込
*/
