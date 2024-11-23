/**************************************************************************
 This is an example for our Monochrome OLEDs based on SSD1306 drivers

 Pick one up today in the adafruit shop!
 ------> http://www.adafruit.com/category/63_98

 This example is for a 128x64 pixel display using SPI to communicate
 4 or 5 pins are required to interface.

 Adafruit invests time and resources providing this open
 source code, please support Adafruit and open-source
 hardware by purchasing products from Adafruit!

 Written by Limor Fried/Ladyada for Adafruit Industries,
 with contributions from the open source community.
 BSD license, check license.txt for more information
 All text above, and the splash screen below must be
 included in any redistribution.
 **************************************************************************/
/*
Makoto kuwabara 2024 
  
*/

#include <EEPROM.h>
#include <SPI.h>
#include <Wire.h>
//#include <Adafruit_GFX.h>
#include "Adafruit_SSD1306.h"
//#include "Adafruit_SSD1351.h"
#include "Fonts/FreeSans12pt7b.h"
//#include "Adafruit_ILI9341.h"
#include "Channel.h"

#include <SD.h>
void openSDcard();

// for MPU9250
#include <MahonyAHRS.h>
#include "mpu9250.h"
#define MPU9250_CS PB10

Mahony filter;
bfs::Mpu9250 imu(&SPI, MPU9250_CS);

/**/
float oax, oay, oaz;
float ogx, ogy, ogz;
float ax, ay, az;
float gx, gy, gz;
float mx, my, mz;
float roll, pitch, heading;

void IMUloop();

///////////////////////////
#define PIN_PICKUP PB11  //

//#define PIN_XXX PB13   //
#define PIN_IGN PB14   //
#define PIN_PASS PB15  //
#define PIN_HILOW PA8  //

#define PIN_CHANGE PB3  //
#define PIN_HORN PB4    //
#define PIN_TL PB5      //
#define PIN_TR PB6      //
//#define PIN_HILOW PB7   //
#define PIN_GP4 PB7  //
#define PIN_GP2 PB8  //
#define PIN_GP1 PB9  //

#define PIN_OUT0 PA15  //
#define PIN_OUT1 PB12  //
////////////////////////////

//#define MPU9250_CS PB10
#define SD_CS PB13  //PC13  //share led

//SSD1306 OLED Display
#define SCREEN_WIDTH 128  // OLED display width, in pixels
#define SCREEN_HEIGHT 64  // OLED display height, in pixels

// Declaration for SSD1306 display connected using software SPI (default case):
/*
  #define OLED_MOSI   PB5
  #define OLED_CLK   PB3
  #define OLED_DC    PB4
  #define OLED_CS    PA15
  #define OLED_RESET PA8
  */

/*　ソフトウェアSPI？　*@/
  #define OLED_MOSI  PB5
  #define OLED_CLK   PB3

  #define OLED_DC PB13
  #define OLED_CS PB14
  #define OLED_RESET PB15

  //Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT,
  //  OLED_MOSI, OLED_CLK, OLED_DC, OLED_RESET, OLED_CS);
  Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT,
                          OLED_MOSI, OLED_CLK, OLED_DC, OLED_RESET, OLED_CS);  //*/

/*  Comment out above, uncomment this block to use hardware SPI  
 ハードウェアSPI？*/
#define OLED_DC PC15  //
#define OLED_CS PC14
#define OLED_RESET -1  //PB15
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &SPI, OLED_DC, OLED_RESET, OLED_CS);
//*/
// SSD1306 PINOUT
/*
          PA5 PA7 PB15 PB13 PB14
  GND VDD SCK SDA RES  DC   CS
  |                           |
  |         SSD1306           |
  |                           |
  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    GND VDD SCK SDA RES  DC   CS
  |                           |
  |         SSD1351           |
  |                           |
  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
*/
/*
#define SCREEN_HEIGHT 128
const uint8_t OLED_pin_cs_ss = PC14;  //10;
const uint8_t OLED_pin_res_rst = -1;  //9;
const uint8_t OLED_pin_dc_rs = PC15;  //8;



// declare the display
Adafruit_SSD1351 display =
  Adafruit_SSD1351(
    SCREEN_WIDTH,
    SCREEN_HEIGHT,
    &SPI,
    OLED_pin_cs_ss,
    OLED_pin_dc_rs,
    OLED_pin_res_rst);
// Color definitions
#define BLACK 0x0000
#define BLUE 0x001F
#define RED 0xF800
#define GREEN 0x07E0
#define CYAN 0x07FF
#define MAGENTA 0xF81F
#define YELLOW 0xFFE0
#define WHITE 0xFFFF

#ifdef	BLACK 
display.fillScreen(BLACK);
#else
display.clearDisplay();
#endif

#ifdef	BLACK 
#define display_clear() display.fillScreen(BLACK)
#define display_display 
#else
#define display_clear() display.clearDisplay()
#define display_display display.display
#endif
*/
//ILI9341 ???
// If using the breakout, change pins as desired
//Adafruit_ILI9341 display = Adafruit_ILI9341(TFT_CS, TFT_DC, TFT_MOSI, TFT_CLK, TFT_RST, TFT_MISO);
//Adafruit_ILI9341 display = Adafruit_ILI9341(TFT_CS, OLED_DC, OLED_RESET);
//#define TFT_CS PA8

// for SD card
const char filename[] = "LOGX1.txt";
// File object to represent file
File logFile;
// string to buffer output
String logbuffer = "";
unsigned long loglastMillis = 0;

/*
#define NUMFLAKES 10  // Number of snowflakes in the animation example

#define LOGO_HEIGHT 16
#define LOGO_WIDTH 16
*/
#define LEDON digitalWrite(LED_BUILTIN,LOW)
#define LEDOFF digitalWrite(LED_BUILTIN,HIGH)

/////////////////////////////
int CUTTIME[6] = { -1, 120, 80, 60, 60, 55 };  //N 1 2 3 4 5    //millis
int HOLDOFFTIME = 300;                         //millis  should be HOLDOFFTIME>300

unsigned long cutstart = 0;

bool state_Cut = false;
bool state_UP = false;
int change = 1;

//millis()　50日間でオーバーフローし、ゼロに戻ります。
unsigned long tmprev = 0;
int lap = 0;

int fpscnt;
unsigned long fpstmprev = 0;

////////////////////
/*
int FRONTPOT_PIN = PA0;
int REARPOT_PIN = PA1;
int TOPOT_PIN = PA2;
int O2_PIN = PA3;
int AP_PIN = PB0;
int PICKUP_PIN = PB1;
*/

Channel FRONTPOT;
Channel REARPOT;
Channel TOPOT;
Channel O2;
Channel AP;
Channel PICKUP;
Channel ROLL;
Channel PITCH;
Channel ACC;

#define CHNUM 9
int MaxVal[CHNUM];
int MinVal[CHNUM];
int mMaxVal[CHNUM];
int mMinVal[CHNUM];
int curVal[CHNUM];
int lastVal[CHNUM];
int traceVal[CHNUM];

int Offset[CHNUM] = { 870, 870, 900, 870, 870, 900, -900, -900, 0 };
float Scale[CHNUM] = { -0.0053173828, -0.0053173828, -0.1329345703125, -0.0053173828, -0.0053173828, -0.1329345703125, 1, 1, 0.0193359375 };

void chinit() {
  Offset[0] = 0;

  //pa4  acc]
  //V33 = ad * 3.3 / 1024
  //V5 = (3.030303 - V33)*1.65
  // = (3.030303 - ad * 3.3 / 1024)*1.65
  // = 5-5.445*ad/1024

  Scale[6] = 3.3 * 6.0 / 1024;  //19.8

  FRONTPOT.setRangeLow(900, 0);
  FRONTPOT.setRangeHigh(143, 100);

  REARPOT.setRangeLow(900, 0);
  REARPOT.setRangeHigh(143, 100);

  ROLL.setRangeLow(900, 0);
  ROLL.setRangeHigh(-900, 100);
  PITCH.setRangeLow(900, 0);
  PITCH.setRangeHigh(-900, 100);

  //Channel TOPOT;
  //Channel O2;
  //Channel AP;
  //Channel PICKUP;
  //Channel ACC;
}
int rpm = 0;
volatile int32_t pulsewidth = 3334;
//volatile uint32_t count = 0;  // Holds the count of interrupts
volatile uint32_t microsprev = 0;  // Holds the count of interrupts
volatile uint32_t microscurr = 0;  // Holds the count of interrupts

int Pulse[12] = { 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000 };
int pulsePos = 0;
int32_t pulseIntg = 0;

void int_pickup() {
  //  digitalWrite(PIN_OUT1, LOW);
  microscurr = micros();
  uint32_t past = microscurr - microsprev;
  if (past > 15000) {  //1200rpm 50msec/rev     4.16666/pls 8.333/pls wide
    past = 4294967295 - past;
  }
  if (past > 15000) past = 15000;
  if (Pulse[pulsePos] * 3 < past * 2) {
    pulsewidth = pulseIntg + past;
    pulsePos = 0;
    pulseIntg = 0;
  } else {
    pulseIntg += past;
    pulsePos++;
    if (pulsePos > 11) {
      pulsewidth = pulseIntg;
      pulsePos = 0;
      pulseIntg = 0;
    }
  }
  Pulse[pulsePos] = past;
  microsprev = microscurr;
  //  digitalWrite(PIN_OUT1, HIGH);
}

/*
  noInterrupts();
  pulsewidth = pulsewidth;
  Serial.println(count);
  interrupts();
*/

void pininit() {
  pinMode(PIN_GP1, INPUT_PULLUP);  //  PB9 //
  pinMode(PIN_GP2, INPUT_PULLUP);  //  PB8 //
  pinMode(PIN_GP4, INPUT_PULLUP);  //  PB7 //

  //pinMode(PIN_XXX, INPUT_PULLUP);    // PA13 //
  pinMode(PIN_IGN, INPUT_PULLUP);    // PA14 //
  pinMode(PIN_PASS, INPUT_PULLUP);   // PA15 //
  pinMode(PIN_HILOW, INPUT_PULLUP);  // PA8  //

  pinMode(PIN_CHANGE, INPUT_PULLUP);  // PB3  //
  pinMode(PIN_HORN, INPUT_PULLUP);    // PB4  //
  pinMode(PIN_TL, INPUT_PULLUP);      // PB5  //
  pinMode(PIN_TR, INPUT_PULLUP);      // PB6  //

  pinMode(PIN_PICKUP, INPUT_PULLUP);  //PB11

  pinMode(PIN_OUT0, OUTPUT);  // PA15 //
  pinMode(PIN_OUT1, OUTPUT);  // PB12 //
  digitalWrite(PIN_OUT0, LOW);

  digitalWrite(PIN_OUT1, LOW);
  digitalWrite(PIN_OUT1, HIGH);  //
  digitalWrite(PIN_OUT1, LOW);   //


  cutstart = millis();


  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

  digitalWrite(SD_CS, HIGH);

  // attachInterrupt(digitalPinToInterrupt(PIN_PICKUP), int_pickup, RISING);
}

//#define XXX (digitalRead(PIN_XXX) == 0)
#define IGN (digitalRead(PIN_IGN) == 0)
#define PASS (digitalRead(PIN_PASS) == 0)
#define HILOW (digitalRead(PIN_HILOW) == 0)

#define CHANGE (digitalRead(PIN_CHANGE) == 0)
#define HORN (digitalRead(PIN_HORN) == 0)

#define TLEFT (digitalRead(PIN_TL) == 0 && digitalRead(PIN_TR) == 1)
#define TRIGHT (digitalRead(PIN_TR) == 0 && digitalRead(PIN_TL) == 1)
#define HAZARD (digitalRead(PIN_TL) == 0 && digitalRead(PIN_TR) == 0)
#define NOMAL (digitalRead(PIN_TL) == 1 && digitalRead(PIN_TR) == 1)

#define PICKUP (digitalRead(PIN_PICKUP) == 0)

//11pulse /rev

int GP_stable = 0;
int GP_prev = 0;
int GP_stableCNT = 4;
/*/////////////////////////
      GP
//////////////////////////*/
//result = GP_stable
int GearPos() {
  int GP = 0;
  if (digitalRead(PIN_GP1) == 0) GP++;
  if (digitalRead(PIN_GP2) == 0) GP += 2;
  if (digitalRead(PIN_GP4) == 0) GP += 4;

  if (GP_prev != GP)
    GP_stableCNT = 4;
  else if (GP_stableCNT > 0)
    GP_stableCNT--;

  GP_prev = GP;
  if ((GP_stableCNT == 0) && (GP != 0))  //シフト中は更新しない
  {
    GP_stable = GP;
  }
  return GP_stable;
}

float GRatio[6] = { 2.923, 1.933, 1.476, 1.217, 1.045, 0.925 };
float ChangeRatio[5] = { 0.6613, 0.7635, 0.8245, 0.8586, 0.8852 };
float ChangeRatioSqrtInv[5] = { 1.229704433, 1.144446458, 1.101297542, 1.079206514, 1.062867916 };
float PowerRpm = 11000;
float LimitRpm = 12200;

float shiftupRpm(int gp) {
  if (gp > 5 || gp < 1) return LimitRpm;
  float rev = PowerRpm * ChangeRatioSqrtInv[gp - 1];
  if (rev > LimitRpm) return LimitRpm;
  return rev;
}
////////////////////////////////

int cutstartGp = 0;
void qs() {
  unsigned long now = millis();
  if (CHANGE) {
    if (change < 2) change++;
  } else {
    if (change > 0) change--;
  }

  if (!state_UP) {                                        //トップギアではカットしない//カット開始判定
    if ((GP_stable < 6)                                   //&& !HAZARD
        && ((TLEFT && (PowerRpm < rpm))                   //Lの時　通常
            || (NOMAL)                                    // && (shiftupRpm(GP_stable) < rpm)) //Rの時　ギアー毎のRPM優先
            || (TRIGHT && (shiftupRpm(GP_stable) < rpm))  //Rの時　ギアー毎のRPM優先
            )                                             /**/
    ) {                                                   //カット開始
      if (change > 1) {                                   //遅延時間　0は即カット
        digitalWrite(PIN_OUT0, HIGH);                     //CUT
        state_Cut = true;
        state_UP = true;
        cutstart = millis();
        cutstartGp = GP_stable;
      }
    }
  } else {
    //カット実行中
    int cuttime = CUTTIME[cutstartGp];

    if (now - cutstart < cuttime && cutstartGp == GP_stable) {
      digitalWrite(PIN_OUT0, HIGH);  //CUT
      state_Cut = true;
    } else {
      //カット終了
      digitalWrite(PIN_OUT0, LOW);
      state_Cut = false;
      if (HAZARD || (change != 2 && (now - cutstart > HOLDOFFTIME))) {
        state_UP = false;
      }
    }
  }
}


//凡例
//指定したデータを文字列にする
String collectdata() {
  String dataString = "";
  for (int i = 0; i < CHNUM - 1; i++) {
    int sensor = curVal[i];
    dataString += String(sensor);
    if (i < CHNUM - 2) {
      dataString += ",";
    }
  }
  return dataString;
}

float string2float(File dataFile) {
  int i = 0;
  char vectorChar[20];
  while (dataFile.available()) {
    int inChar = dataFile.read();
    vectorChar[i++] = (char)inChar;
    if (inChar == '\n') {

      float f = atof(vectorChar);
      Serial.print("Value:");
      Serial.println(String(f, 5));
      Serial.print("String: ");
      Serial.println(vectorChar);
      return f;
    }
  }
  return -999;
}

///////////////////////////////////////////////////////*/
unsigned int ChunkSize = 555;
unsigned long SD_flushprev = 0;

void openSDcard(String filename) {
  // If you want to start from an empty file,
  // uncomment the next line:
  // SD.remove(filename);
  display.print("Open SD...");
  display.print(filename);
  display.display();
  // try to open the file for writing
  logFile = SD.open(filename, FILE_WRITE);
  if (!logFile) {
    display.print("error opening ");
    while (1)
      ;
  } else {
    //   display.print(" open ");
    //   display.println(filename);
    display.print(" OK ");
  }


  display.display();

  logFile.println("Start Power");

  loglastMillis = SD_flushprev = millis();
  delay(5000);
}

/*
  logbuffer;
  unsigned long loglastMillis
  logFile
　nonblock　need open file(openSDcard) before store
*/

/*/*////////////////////////////////////////////////////////////////////////////////////
void storelog() {
    unsigned long now = millis();

  if ((now - loglastMillis) >= 30) {

    String dataString = collectdata();
    // add a new line to the buffer
    logbuffer += String(now, HEX);
    logbuffer += ":";
    logbuffer += dataString ;
   // logbuffer += "\r\n";
    loglastMillis = now;

    // open the file. note that only one file can be open at a time,
    // so you have to close this one before opening another.
 //   File dataFile = SD.open(filename, FILE_WRITE);
    ChunkSize = logFile.availableForWrite();


    // if the file is available, write to it:
    if (logFile) {
      LEDON;
      logFile.println(logbuffer);
      
 //     dataFile.close();
      LEDOFF ;
      // print to the serial port too:
      Serial.println(dataString);
    }
    // if the file isn't open, pop up an error:
    else {
      Serial.println("error opening dataXX.txt");
    }
  }
  logbuffer="";
}



void nbstorelog() {  // check if it's been over 10 ms since the last line added



  unsigned long now = millis();

  if ((now - SD_flushprev) >= 1000) {
    logFile.flush();
    SD_flushprev = now;
  }

  if ((now - loglastMillis) >= 30) {
    String dataString = collectdata();
    // add a new line to the buffer
    logbuffer += String(now, HEX);
    logbuffer += ":";
    logbuffer += dataString ;
    logbuffer += "\r\n";
    loglastMillis = now;
  }

  // check if the SD card is available to write data without blocking
  // and if the buffered data is enough for the full chunk size
  // LEDON;
  unsigned int chunkSize = logFile.availableForWrite();

  // LEDOFF;

  ChunkSize = chunkSize;
  if ((logbuffer.length() >= chunkSize)) {
    // write to file and blink LED
    //  ChunkSize = chunkSize;

    //        LEDON;
    //      delay(10);
    LEDON;

    logFile.write(logbuffer.c_str(), chunkSize);

    LEDOFF ;
      //      logFile.flush();
      // remove written data from buffer
    logbuffer.remove(0, chunkSize);
  }
}

void closelog() {
  logFile.close();
}

//データの読み出し（すべて）
void dumpdatas(String filename) {
  Serial.print("dump SD...");
  Serial.println(filename);

  display.clearDisplay();
  display.print("dump SD...");
  display.println(filename);
  display.print("pudh Horn to start");

  display.display();

  display.clearDisplay();
  display.print("dumping SD...");
  display.print("push Horn to Cansel");
  display.println(filename);
  display.display();

  /*
  // see if the card is present and can be initialized:
  if (!SD.begin(chipSelect)) {
    Serial.println("Card failed, or not present");
    // don't do anything more:
    while (1);
  }
  Serial.println("card initialized.");
  */
  // open the file. note that only one file can be open at a time,
  // so you have to close this one before opening another.

  File dataFile = SD.open(filename);
  // if the file is available, write to it:
  if (dataFile) {
    while (dataFile.available()) {
      Serial.write(dataFile.read());
    }
    dataFile.close();
  }
  // if the file isn't open, pop up an error:
  else {
    Serial.print("error opening ");
    Serial.println(filename);
  }
}

//アナログデータ初期化
void initLog() {
  tmprev = fpstmprev = millis();
  updatecurLog();
  updatecurLog();
  for (int i = 0; i < CHNUM; i++) {
    MaxVal[i] = curVal[i];
    MinVal[i] = curVal[i];
    mMaxVal[i] = curVal[i];
    mMinVal[i] = curVal[i];
    traceVal[i] = curVal[i];
  }
}

///アナログデータ拾得 (IMU含む)
void updatecurLog() {
  unsigned long curtm = millis();
  lap = curtm - tmprev;
  tmprev = curtm;

  for (int i = 0; i < CHNUM; i++)
    lastVal[i] = curVal[i];

  curVal[0] = analogRead(PA0);  //pickup
  curVal[1] = analogRead(PA1);  //AP
  curVal[2] = analogRead(PA2);  //FSUS
  curVal[3] = analogRead(PA3);  //O2
  curVal[4] = analogRead(PB0);  //TO
  curVal[5] = analogRead(PB1);  //RSUS

  curVal[6] = (int)(roll * 10);  // deg*10 -900 ... 900
  curVal[7] = (int)(pitch * 10);
  curVal[8] = analogRead(PA4);  //ACC

  for (int i = 0; i < CHNUM; i++) {
    if (MaxVal[i] < curVal[i]) MaxVal[i] = curVal[i];
    if (MinVal[i] > curVal[i]) MinVal[i] = curVal[i];
    if (mMaxVal[i] <= curVal[i]) mMaxVal[i] = curVal[i];
    else mMaxVal[i]--;
    if (mMinVal[i] >= curVal[i]) mMinVal[i] = curVal[i];
    else mMinVal[i]++;

    if (traceVal[i] > curVal[i]) traceVal[i]--;
    if (traceVal[i] < curVal[i]) traceVal[i]++;
  }
}


/////////////////////////////////////////////////////////////////////////////////////////////////////////////

int fps;

void DispStatus() {

  display.setFont(&FreeSans12pt7b);
  display.setTextSize(1);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text

  if (state_UP) {
    display.fillRect(0, 45, 22, 18, SSD1306_WHITE);  //
    if (!state_Cut)
      display.fillRect(5, 45, 12, 18, SSD1306_INVERSE);  //
  }
  display.setCursor(5, 63);
  display.println(GP_stable);

  /*
  display.setCursor(10, 45);
  display.println(pitch);

  display.setTextSize(1);
  display.setCursor(10, 53);
  display.print(MinVal[1]);
  display.setCursor(70, 53);
  display.println(MaxVal[1]);
  */

  display.setCursor(25, 63);
  int rpm_100 = (rpm + 50) / 100;
  if (rpm_100 < 10) display.print(' ');
  if (rpm_100 < 100) display.print(' ');
  display.print(rpm_100);
  display.fillRect(0, 60, rpm_100 - 70, 4, SSD1306_INVERSE);

  display.setFont();
  display.setTextSize(1);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text
  display.setCursor(64, 63);
  display.println("00rpm");

  display.setCursor(105, 54);  // Start at top-left corner
  display.println(fps);

  //unsigned int chunkSize = logFile.availableForWrite();
  display.setCursor(50, 48);
  display.print("ch ");
  display.print(ChunkSize);
  display.print("bf ");
  display.println(logbuffer.length());
}

void DispRoll(int y) {
  int x = (roll * 2.1333);  //128 / 60
                            //30deg = 64
                            //この時度の位置
                            //x-96  X-64   x-32   x   x+32
                            // -60   -30    0    30    60

  //x-64    x    x+64  x+128  x+192
  // -60///-30    0    30 ///60
  display.drawRect(0, y, 127, 15, SSD1306_WHITE);
  display.drawLine(96, y, 96, y + 8, SSD1306_WHITE);
  display.drawLine(64, y, 64, y + 8, SSD1306_WHITE);
  display.drawLine(32, y, 32, y + 8, SSD1306_WHITE);
  //-60deg
  if (x - 64 > 0) {
    display.fillRect(x - 64, y, 64, 14, SSD1306_INVERSE);             //right to left
  } else if (x > 0) {                                                 //x30 = x-32
    display.fillRect(0, y, x, 14, SSD1306_INVERSE);                   //right to left
  } else if (x + 64 > 0) {                                            //x30 = x-32
    display.fillRect(x + 128, y, 128 - x - 64, 14, SSD1306_INVERSE);  //right to left
  } else {                                                            //
    display.fillRect(x + 128, y, 64, 14, SSD1306_INVERSE);            //right to left
  }
  /*
  display.fillTriangle(
    x - 64, y,
    x - 96, y + 14,
    x - 96, y, SSD1306_INVERSE);

  display.fillTriangle(
    x + 192, y,
    x + 224, y + 14,
    x + 224, y, SSD1306_INVERSE);
*/
  if (x - 64 - 5 > 0 && x - 64 - 5 < 115) {
    display.setCursor(x - 64 - 5, y + 7);
    display.print("60");
  }
  if (x - 32 - 5 > 0 && x - 32 - 5 < 115) {
    display.setCursor(x - 32 - 5, y + 7);
    display.print("45");
  }
  if (x - 5 > 0 && x - 5 < 115) {
    display.setCursor(x - 5, y + 7);
    display.print("30");
  }
  if (x + 64 - 2 > 0 && x + 64 - 2 < 115) {
    display.setCursor(x + 64 - 2, y + 7);
    display.print("0");
  }
  if (x + 128 - 5 > 0 && x + 128 - 5 < 115) {
    display.setCursor(x - 5 + 128, y + 7);
    display.print("30");
  }
  if (x + 160 - 5 > 0 && x + 160 - 5 < 115) {
    display.setCursor(x - 5 + 160, y + 7);
    display.print("45");
  }
  if (x + 192 - 5 > 0 && x + 192 - 5 < 115) {
    display.setCursor(x - 5 + 192, y + 7);
    display.print("60");
  }
}

void DispIMU() {
  display.clearDisplay();

  int y = (pitch * 2.1333);  // * (128.0/90) ;
  if (y > 0) {
    display.fillRect(0, 0, 6, 64 - y, SSD1306_INVERSE);  //right to left
  } else {
    display.fillRect(0, -y, 6, 64 + y, SSD1306_INVERSE);  //right to left
  }

  DispRoll(14);
  display.setCursor(0, 30);

  //display.println("Accele:");
  display.print("X:");
  display.print(ax);
  display.print(" Y:");
  display.print(ay);
  display.print(" Z:");
  display.println(az);

  DispStatus();
  display.display();
}

void DispBar(int ch) {
  display.clearDisplay();
  int x;
  x = MinVal[ch] / 8;
  display.drawLine(x, 0, x, 35, SSD1306_WHITE);
  x = MaxVal[ch] / 8;
  display.drawLine(x, 0, x, 35, SSD1306_WHITE);

  int sx = mMinVal[ch] / 8;
  display.drawLine(sx, 0, sx, 25, SSD1306_WHITE);
  int ex = mMaxVal[ch] / 8;
  display.drawLine(ex, 0, ex, 25, SSD1306_WHITE);

  int tx = traceVal[ch] / 8;
  // display.fillRect(0, 4, tx, 20, SSD1306_INVERSE);
  x = curVal[ch] / 8;
  display.fillRect(x, 4, 127 - x, 18, SSD1306_INVERSE);  //right to left
    // display.fillRect(0, 4, x, 18, SSD1306_INVERSE);//left to right

  display.setFont(&FreeSans12pt7b);
  display.setTextSize(2);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text

  display.setCursor(35, 40);
  display.println(curVal[ch]);

  display.setCursor(x - 10, 42);
  //display.println(curVal[0]);

  display.setFont();
  display.setTextSize(1);
  display.setCursor(10, 53);
  display.print(MinVal[ch]);
  display.setCursor(70, 53);
  display.println(MaxVal[ch]);

  DispStatus();
  display.display();
  /////////////////////////////
}

void DispBar2(int ch) {
  display.clearDisplay();
  int x = (int)FRONTPOT.getValue();

  //   display.fillRect(x, 4, 127 - x, 18, SSD1306_INVERSE);  //right to left
  display.fillRect(0, 4, x, 18, SSD1306_INVERSE);  //left to right
  display.setFont(&FreeSans12pt7b);
  display.setTextSize(1);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text

  display.setCursor(0, 40);
  display.println(x);
  //display.println(curVal[ch]);
  //display.println(curVal[0]);

  display.setFont();
  display.setTextSize(1);
  display.setCursor(40, 35);
  display.print(MinVal[ch]);
  display.setCursor(70, 35);
  display.println(MaxVal[ch]);

  DispStatus();
  display.display();
  /////////////////////////////
}

void DispBars() {
  display.clearDisplay();

  int x;
  //display.setFont(&FreeSans12pt7b);
  display.setFont();
  display.setTextSize(1);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text
  for (int ch = 0; ch < 6; ch++) {
    x = MinVal[ch] / 8;
    display.drawLine(x, ch * 8, x, ch * 8 + 8, SSD1306_WHITE);
    x = MaxVal[ch] / 8;
    display.drawLine(x, ch * 8, x, ch * 8 + 8, SSD1306_WHITE);

    int sx = mMinVal[ch] / 8;
    display.drawLine(sx, ch * 8, sx, ch * 8 + 5, SSD1306_WHITE);
    int ex = mMaxVal[ch] / 8;
    display.drawLine(ex, ch * 8, ex, ch * 8 + 5, SSD1306_WHITE);

    int tx = traceVal[ch] / 8;
    // display.fillRect(0, 4, tx, 20, SSD1306_INVERSE);
    x = curVal[ch] / 8;


    // display.fillRect(x, ch*8, 127 - x, 7, SSD1306_INVERSE);  //right to left
    display.fillRect(0, ch * 8, x, 7, SSD1306_INVERSE);  //left to right

    display.setCursor(x * 7 / 8, ch * 8);
    float val = (curVal[ch] - Offset[ch]) * Scale[ch];
    display.print(val);
  }

  x = (roll * 1.42);  // * (128.0/90) ;
  //x %= 128;
  if (x > 0)
    display.fillRect(x, 6 * 8, 128 - x, 7, SSD1306_INVERSE);  //right to left
  else
    display.fillRect(0, 6 * 8, 128 + x, 7, SSD1306_INVERSE);  //right to left
  x = (pitch * 1.42);                                         // * (128.0/90) ;
  //x %= 128;
  if (x > 0)
    display.fillRect(x, 7 * 8, 128 - x, 7, SSD1306_INVERSE);  //right to left
  else
    display.fillRect(0, 7 * 8, 128 + x, 7, SSD1306_INVERSE);  //right to left


  DispStatus();
  display.display();
  /////////////////////////////
}

//Horn sw
#define clicktime 200
#define dblclicktime 800
int state = 0;  //

bool Horn_click = false;
bool Horn_dblclick = false;

unsigned long pushhorn = 0;
unsigned long releasehorn = 0;
bool hornprev = false;

void Hornsw() {
  Horn_click = Horn_dblclick = false;
  unsigned long now = millis();
  bool Push = (digitalRead(PIN_HORN) == 0);
  if (Push != hornprev) {
    if (Push) {
      if (state == 1 && now - pushhorn < dblclicktime) {
        //dblClick
        Horn_dblclick = true;
        state = 0;
      }
      pushhorn = now;
    } else {
      releasehorn = now;
      if (state == 0 && now - pushhorn < clicktime) {
        //Click
        Horn_click = true;

      } else {
      }
      state++;
    }
  } else {
    if (state == 0 && Push && now - pushhorn > 3000) {
      //Hold
      // } else if (!Push && state == 1 && now - pushhorn < clicktime) {
      //cli//ck

    } else if (now - pushhorn > dblclicktime) {
      state = 0;
    }
  }
  hornprev = Push;
}

//DispMode  0:Tacho  1:quickshift 2:
int DispMode = 0;

void Disp1306() {
  Hornsw();
  if (Horn_dblclick) {

  } else if (Horn_click) {
    DispMode++;
    if (DispMode > 3) DispMode = 0;
  }

  ///////////// SSD Display Fpot Horizontal bar with num///////////////
  if (DispMode == 0) {

    int ch = 2;
    //DispBar(ch);
    DispBar2(ch);

  } else if (DispMode == 1) {
    //DispBar(ch);
    DispIMU();
  } else if (DispMode == 2) {
    int ch = 2;
    //DispBar(ch);
    DispBars();
  } else {
    DispIMU();
  }
}

///////////////////////////////////setup/////////////////////////////////////////////////

void setup() {

  chinit();
  oax = oay = oaz = ogx = ogy = ogz = 0.0;
  Serial.begin(9600);
  /*
  Serial.println("ILI9341 Test!"); 
  display.begin();
  */
  pininit();

  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  if (!display.begin(SSD1306_SWITCHCAPVCC)) {
    while (!Serial && millis() < 2000)
      ;
    Serial.println(F("SSD1306 allocation failed"));
    while (1) {}  // Don't proceed, loop forever
  }

  // Clear the buffer
  // display.clearDisplay();
  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(20);               // Pause for 2 seconds
  display.setRotation(2);  //rotates text on OLED 1=90 degrees, 2=180 degrees

  /*
  //EEPROM.length();
  //value = EEPROM.read(address);
  //EEPROM.write(addr, val);
  display.clearDisplay();*/
  display.setFont();
  display.setTextSize(1);
  display.setTextColor(SSD1306_INVERSE);
  display.setCursor(0, 0);
  display.println("EEPROM.length();");
  display.println(EEPROM.length());  //    1024

  digitalWrite(PIN_OUT0, LOW);

  for (int i = 0; i < 5; i++) {
    display.display();
    display.println(EEPROM.read(i));
  }

  display.display();
  delay(200);  // Pause for 2 seconds

  // Clear the buffer
  display.clearDisplay();
  display.display();
  delay(100);  // Pause for 2 seconds
  ///////////////////////
  display.setFont(&FreeSans12pt7b);
  display.setTextSize(1);                 // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);  // Draw white text

  digitalWrite(PIN_OUT0, HIGH);

  display.setCursor(0, 20);
  display.println("QuiqShifter");
  display.println("QuiqLogger");
  display.display();
  //display.startscrollleft(0x00, 0x08);
  delay(1000);

  digitalWrite(PIN_OUT1, LOW);
  ///////////////////////////
  display.clearDisplay();

  display.setFont();
  display.setCursor(0, 5);
  display.print("Serial ");
  display.display();
  //////////////////////////

  while (!Serial && millis() < 2000)
    ;
  if (Serial) {
    display.println(" OK ");
  } else {
    display.println("no connect ");
  }
  display.display();

  // example of nonblockingWrite
  //init for SD Logger
  // reserve 1kB for String used as a buffer
  logbuffer.reserve(4096);
  // set LED pin to output, used to blink when writing

  //LEDON;

  digitalWrite(SD_CS, HIGH);  // init the SD card
  display.print("Init SD...");
  Serial.print("initializing the SD card...");
  display.display();

  if (!SD.begin(SD_CS)) {
    //Serial.println("Card failed, or not present");
    // don't do anything more:
    //while (1);
    Serial.println("failed");
    display.println("failed ");
  } else {
    display.println(" OK ");
    Serial.println(" OK ");
  }
  display.display();


  ///
  // Draw a single pixel in white
  // display.drawPixel(10, 10, SSD1306_WHITE);

  // Show the display buffer on the screen. You MUST call display() after
  // drawing commands to make them visible on screen!
  //  display.display();
  delay(1000);

  initLog();
  //LEDOFF;

  //initIMU();
  display.print("Init IMU...");
  Serial.print("initializing the IMU...");
  display.display();
  if (!imu.Begin()) {
    Serial.println("Error initializing communication with IMU");
    display.println("Error");
    display.display();
    Serial.println("Error");
    while (1) {}
  } else {
    display.println("OK");
    Serial.println(" OK ");
  }
  display.display();
  /* Set the sample rate divider */
  if (!imu.ConfigSrd(19)) {
    //Serial.println("Error configured SRD");
    display.println("failed SRD(19)");
    display.display();
    while (1) {}
  } else {
    display.println("OK");
  }
  if (!imu.ConfigDlpfBandwidth(bfs::Mpu9250::DLPF_BANDWIDTH_92HZ)) {  //bfs::Mpu9250::DLPF_BANDWIDTH_92HZ)) {
    //Serial.println("Error configured SRD");
    display.println("failed DLPF_BANDWIDTH_92HZ");
    display.display();
    //while (1) {}
  } else {
    display.println("OK");
  }


  display.display();
  ////////////////////////

  if (HORN) {
    display.clearDisplay();
    display.setCursor(0, 20);
    display.println("HOLD 3s");
    display.println("Set up Mode");
    display.display();

    unsigned long now = millis();
    loglastMillis = now;
    delay(1000);

    while (HORN) {
      updatecurLog();
    }
    if ((millis() - loglastMillis) < 3000) return;
    display.clearDisplay();
    display.println("--Set up Mode--");
    delay(1000);
    //

    float GX = 0.0;
    float GY = 0.0;
    float GZ = 0.0;
    float AX = 0.0;
    float AY = 0.0;
    float AZ = 0.0;
    int sample = 3000;
    int cnt = sample;
    while (!HORN && (cnt--) >= 0) {
      updatecurLog();
      // GearPos();
      IMUloop();
      DispIMU();
      GX += gx;
      GY += gy;
      GZ += gz;
      AX += ax;
      AY += ay;
      AZ += az;
    }

    GX /= sample;
    GY /= sample;
    GZ /= sample;
    AX /= sample;
    AY /= sample;
    AZ /= sample;

    // AZ=-10.3;    should be -9.80665f

    display.clearDisplay();
    display.setCursor(0, 0);
    display.println("--Set up Mode--");
    display.print("   Finish \r\n ");
    display.print("GX:");
    display.print(GX);
    display.print(",GY:");
    display.print(GY);
    display.print(",GZ:");
    display.println(GZ);

    display.print("AX:");
    display.print(AX);
    display.print(",AY:");
    display.print(AY);
    display.print(",AZ:");
    display.println(AZ);
    display.display();
    delay(1000);

    while (!HORN) {
    }
    while (HORN) {
    }
    while (!HORN) {
    }
    /*
    if (false) {
      int eeAddress = 256;
      int step = sizeof(float);
      EEPROM.put(eeAddress, AX);
      eeAddress += step;
      EEPROM.put(eeAddress, AY);
      eeAddress += step;
      EEPROM.put(eeAddress, AZ + 9.80665f);
      eeAddress += step;
      EEPROM.put(eeAddress, GX);
      eeAddress += step;
      EEPROM.put(eeAddress, GY);
      eeAddress += step;
      EEPROM.put(eeAddress, GZ);
      eeAddress += step;
    }*/
    if (SD.exists("setup.txt")) SD.remove("setup.txt");
    File dataFile = SD.open("setup.txt", FILE_WRITE);
    dataFile.println(String(AX, 5));
    dataFile.println(String(AY, 5));
    dataFile.println(String(AZ + 9.80665f, 5));
    dataFile.println(String(GX, 5));
    dataFile.println(String(GY, 5));
    dataFile.println(String(GZ, 5));

    dataFile.close();

    display.clearDisplay();
    display.setCursor(0, 20);
    display.println("setup.txt");
    display.println("Update");
    display.display();
  }
  /*
  if (false) {
    int eeAddress = 256;
    int step = sizeof(float);
    EEPROM.get(eeAddress, oax);
    eeAddress += step;
    EEPROM.get(eeAddress, oay);
    eeAddress += step;
    EEPROM.get(eeAddress, oaz);
    eeAddress += step;
    EEPROM.get(eeAddress, ogx);
    eeAddress += step;
    EEPROM.get(eeAddress, ogy);
    eeAddress += step;
    EEPROM.get(eeAddress, ogz);
    eeAddress += step;
  }
  */
  /**/
  File dataFile = SD.open("setup.txt");
  // if the file is available, write to it:
  if (dataFile) {
    oax = string2float(dataFile);
    oay = string2float(dataFile);
    oaz = string2float(dataFile);
    ogx = string2float(dataFile);
    ogy = string2float(dataFile);
    ogz = string2float(dataFile);


    dataFile.close();

    display.setCursor(0, 20);
    display.println("oax");
    display.print(oax);
    display.print(" ");
    display.print(oay);
    display.print(" ");
    display.print(oaz);
    display.print(" ");
    display.print(ogx);
    display.print(" ");
    display.print(ogy);
    display.print(" ");
    display.print(ogz);
    display.display();
  }



  delay(10);
  openSDcard(filename);

  delay(1000);
}



////////////////////////////////////////////

void loop() {
  noInterrupts();
  int pulse = pulsewidth;
  interrupts();
  rpm = 60000000 / pulse;
  GearPos();
  qs();
  IMUloop();
  updatecurLog();
  //  FRONTPOT.setRawValue(analogRead(PA2));


  storelog();

  Disp1306();
  /*              */
  unsigned long curtm = millis();
  lap = curtm - fpstmprev;
  if (lap > 100 || fpscnt++ > 30) {
    fpstmprev = curtm;
    fps = (fpscnt * 1000) / lap;
    fpscnt = 0;
  }
}




/* MENU

**** Mode **** Analog Zero set *****
frontup  -> FSUS set to 0mm
rearup   -> RSUS set to 0mm 
TO throttle close - open - close --> set 0% & 100%
O2 rich high voltage   lean low voltage  center 0.45V 0 to 1V Range
AP --> set to 0
jyro set to Zero rad/sec

set Device posture　quaternion　姿勢
ROLL
PITCH 


**** Mode **** acceleration Zero set *****  
Need rotation and stationary state
!! Offline setup

**** Mode **** Data Log set *****  
DataLog Stop Start
*** Data Dump 
*** Dyno 


Channel FRONTPOT;
Channel REARPOT;
Channel TOPOT;
Channel O2;
Channel AP;
Channel PICKUP;
Channel ROLL;
Channel PITCH;
Channel ACC;

*** Pit Run ****

*** 低速　シフト

*** MAX シフト only

*** Disp IMU Bank
*** Disp Rpm
*** Sus Bottom


*/




String inputString = "";      // a String to hold incoming data
bool stringComplete = false;  // whether the string is complete

/*
  SerialEvent occurs whenever a new data comes in the hardware serial RX. This
  routine is run between each time loop() runs, so using delay inside loop can
  delay response. Multiple bytes of data may be available.
*/
void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:

    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inChar == '\n') {
      stringComplete = true;
      doCommand(inputString);
      inputString = "";
    } else {
      inputString += inChar;
    }
  }
}


void doCommand(String inputString) {
  if (inputString == "dump") {
    logFile.close();
    dumpdatas(filename);
  }
  if (inputString == "run") {
    openSDcard(filename);
  }

  stringComplete = false;
}
