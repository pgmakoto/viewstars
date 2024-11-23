#include <SD.h>

#ifndef Channel_h
#define Channel_h

class Channel{
private:
  int PIN;


String cmds[3] = {"\0"}; // 分割された文字列を格納する配列 

static int split(String data, char delimiter, String *dst){
    int index = 0; 
    int datalength = data.length();
    
    for (int i = 0; i < datalength; i++) {
        char tmp = data.charAt(i);
        if ( tmp == delimiter ) {
            index++;
        }
        else dst[index] += tmp;
    }
    
    return (index + 1);
}

public:
  String Name="";
  int MaxVal;
  int MinVal;
  int mMaxVal;
  int mMinVal;
  int curVal;
  int lastVal;
  int traceVal;//時間遅れ

  float Offset;
  float Scale;   // fullscale / 1024 (10bit resolution)

  float fromLow; 
  float fromHigh; 
  float toLow; 
  float toHigh; 

  Channel();
  void begin(int pin,float offset,float Scale);
  void setRawValue(float val);
  float getRawValue(); 
  
  float getValue();
  float getValue(float val);

  //offset range　調整
  //outputvalue = ((rawval) - Offset ) * Scale;
  float setZero(float val);
  float setScale(float val);
  
  float setRangeLow(float rawval,float val);
  bool setRangeHigh(float rawval,float Highval);

  float fmap(float val);
  float fmap(float val,float flow,float fhigh,float tlow,float thigh);

  void setSettings(String props);

//String "propname=value"
  void setSetting(String prop);
  String getSetting(String prop);




};

#endif
