#include "Channel.h"
#include "Arduino.h"

  Channel::Channel(){

  }

  void Channel::setRawValue(float val){
    curVal=val;

    if (MaxVal < curVal) MaxVal = curVal;
    if (MinVal > curVal) MinVal = curVal;
    if (mMaxVal <= curVal) mMaxVal = curVal;
    else mMaxVal--;
    if (mMinVal >= curVal) mMinVal = curVal;
    else mMinVal++;

    if (traceVal > curVal) traceVal--;
    if (traceVal < curVal) traceVal++;

  }
  float Channel::getRawValue(){
    return curVal;
  }
  float Channel::getValue(){
    return fmap(curVal, fromLow,fromHigh,toLow, toHigh);
  }
  float Channel::getValue(float _curVal){
    return fmap(_curVal, fromLow,fromHigh,toLow, toHigh);
  }
  float Channel::setZero(float val){
    Offset=val;
  }
  float Channel::setScale(float val){
    Scale=val;
  }
  
  float Channel::setRangeLow(float rawval,float val){
    fromLow = rawval;
    toLow = val;
    if( abs(fromHigh - fromLow) < 0.01 || abs(toHigh - toLow) < 0.01 ){
      Scale=1.0;
      Offset=0;
      return true;
    }
    Scale=1.0/(fromHigh-fromLow)*(toHigh-toLow);
    Offset = fromLow - toLow / Scale ;
    return false;
  }  
  
  bool Channel::setRangeHigh(float rawval,float Highval){
    fromHigh = rawval;
    toHigh = Highval;
    if( abs(fromHigh - fromLow) < 0.01 || abs(toHigh - toLow) < 0.01 ){
      Scale=1.0;
      Offset=0;
      return true;
    }
    Scale=1.0/(fromHigh-fromLow)*(toHigh-toLow);
    Offset = fromLow - toLow / Scale ;
    return false;
  }

  float Channel::fmap(float val){
    //Scale=1.0/(fhigh-flow)*(thigh-tlow);
    //Offset = flow - tlow / Scale ;
      // value = (Offset - flow) * Scale + tlow;
      // 0 = (Offset - flow) * Scale + tlow;
      // 0 = Offset * Scale - flow * Scale  + tlow;

    float value = (val - Offset) * Scale;
    return value;
  } 

  float Channel::fmap(float val,float flow,float fhigh,float tlow,float thigh){
    float value = (val-flow)/(fhigh-flow)*(thigh-tlow)+tlow;
    return value;
  } 

  void Channel::setSettings(String props){
    int index = props.indexOf('\r');
    String prop;
    if(index>0){
      prop = props.substring(0,index);
      prop.trim();
    }
    while( index = prop.indexOf('\r')>0)
    {
      prop = props.substring(0,index);
      prop.trim();
      props = props.substring(index+1);
      props.trim();
      setSetting(prop);
    }
    setSetting(props);
  }

  //String "propname=value"
  void Channel::setSetting(String prop){
    //prop.split(String prop,'=',cmds);
    int index = prop.indexOf('=');
    if(index<0)return;
    String val = prop.substring(index+1);
    val.trim();
    int datalength = val.length()+1;
    char vectorChar[datalength];
    val.getBytes((unsigned char*)vectorChar, datalength);
    float f = atof((const char*)vectorChar);
    
    if(prop.startsWith("Channel:Name"))
        Name = val;//String 
    if(prop.startsWith("MaxVal"))
        MaxVal = f;
    if(prop.startsWith("MinVal"))
        MinVal = f;
    if(prop.startsWith("curVal"))
        curVal = f;
    if(prop.startsWith("Offset"))
        Offset = f;
    if(prop.startsWith("Scale"))
        Scale = f;
    if(prop.startsWith("fromLow"))
        fromLow = f;
    if(prop.startsWith("fromHigh"))
        fromHigh = f;
    if(prop.startsWith("toLow"))
        toLow = f;
    if(prop.startsWith("toHigh"))
        toHigh = f;

    

  }

  String Channel::getSetting(String prop){
    String res = "Channel:Name=" + Name + "\r\n";
    if(prop == ""){
      res += "MaxVal=" + String(MaxVal,5) + "\r\n";
      res += "MinVal=" + String(MinVal,5) + "\r\n";
      res += "curVal=" + String(curVal,5) + "\r\n";
      res += "Offset=" + String(Offset,5) + "\r\n";
      res += "Scale=" + String(Scale,5) + "\r\n";
      res += "fromLow=" + String(fromLow,5) + "\r\n";
      res += "fromHigh=" + String(fromHigh,5) + "\r\n";
      res += "toLow=" + String(toLow,5) + "\r\n";
      res += "toHigh=" + String(toHigh,5) + "\r\n";
    }
  }
