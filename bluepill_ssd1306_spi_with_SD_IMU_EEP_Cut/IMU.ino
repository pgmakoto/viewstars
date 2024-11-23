// for MPU9250 
#include <MahonyAHRS.h>
#include "mpu9250.h"
#define MPU9250_CS PB10
//#include "IMU.h"

extern Mahony filter;
extern bfs::Mpu9250 imu;

/*
#include <MahonyAHRS.h>
Mahony filter;
bfs::Mpu9250 imu(&SPI, MPU9250_CS);


*/
  extern float ax, ay, az;
  extern float gx, gy, gz;
  extern float mx, my, mz;

  extern float oax, oay, oaz;
  extern float ogx, ogy, ogz;

  extern float roll, pitch, heading;

//millis()　50日間でオーバーフローし、ゼロに戻ります。
unsigned long imutimeprev = 0;


void IMUloop() {
  /* Check if data read */
  if (imu.Read()) {

    // Update the Mahony filter, with scaled gyroscope
    float gyroScale =  57.29578f;//0.01;  // TODO: the filter updates too fast
    //  filter.updateIMU(gx * gyroScale, gy * gyroScale, gz * gyroScale, ax, ay, az);


    gx = imu.gyro_y_radps()-ogy;
    gy = -imu.gyro_x_radps()-ogx;
    gz = imu.gyro_z_radps()-ogz;


    ax = imu.accel_y_mps2()-oay;
    ay = -imu.accel_x_mps2()-oax;
    az = imu.accel_z_mps2()-oaz;

    unsigned long now = millis();
    filter.invSampleFreq =  (now - imutimeprev) / 1000.0f;
    // filter.updateIMU(gx * gyroScale, gy * gyroScale, gz * gyroScale, ax*1000, ay*1000, az*1000);
    filter.update(gx * gyroScale, gy * gyroScale, gz * gyroScale , ax*0.1, ay*0.1, az*0.1,imu.mag_y_ut(),-imu.mag_x_ut(),imu.mag_z_ut());
    imutimeprev = now;
  //  if (readyToPrint()) {
      // print the heading, pitch and roll

      roll = filter.getRoll() + 180 ;
      if(roll>180)roll-=360;
      pitch = filter.getPitch();
      heading = filter.getYaw();

  /*      Serial.print(heading);
      Serial.print(",");
      Serial.print(pitch);
      Serial.print(",");
      Serial.print(roll);
      Serial.print(",");
      Serial.print(gx);
      Serial.print(",");
      Serial.print(gy);
      Serial.print(",");
      Serial.println(gz);
      */
  //  }
  }
}
