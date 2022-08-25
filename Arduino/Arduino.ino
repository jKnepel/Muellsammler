//We are using the i2cdevlib by jrowberg https://github.com/jrowberg/i2cdevlib and the Arduino Wire library

#include <MPU6050_tockn.h>
#include <Wire.h>
#define button 5

MPU6050 mpu6050(Wire);

void setup() {
  pinMode(button, INPUT); //set pinMode for button press check
  Serial.begin(250000); //begin serial with 250000 baud
  Wire.begin(); //begin wire communication
  mpu6050.begin(); //begin mpu6050 communication
  mpu6050.calcGyroOffsets(true); //calculate offsets
  
  Serial.flush(); //flush serial output
  
  while (!Serial);
}

//update values and write to serial
void loop() {
  mpu6050.update();
  Serial.println( String(mpu6050.getAngleX()) + "," + String(mpu6050.getAngleY()) + "," + String(mpu6050.getAngleZ()) + "," + digitalRead(button) );
}
