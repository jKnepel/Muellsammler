using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Options {
    public float audioLevel; //volume for master mixer
    public string up, down, left, right, jump; //string keycodes for keyboard input
    public float gyroXNorm, gyroYNorm, gyroZNorm; //norm values for gyro values
    public string gyroPort; //name of usb port
}
