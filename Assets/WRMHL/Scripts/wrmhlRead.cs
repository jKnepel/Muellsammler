using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.IO;

/*
The WRMHL Libary was provided by https://github.com/relativty/wrmhl and is used to communicate between Arduino and Unity.
While we have modified some of the scripts, the orignal script is property of Relativity and provided with an MIT License. 
*/

public class wrmhlRead : MonoBehaviour {
    public float gyroX = 0;
        public float gyroXNorm = 0;
    public float gyroY = 0;
        public float gyroYNorm = 0;
    public float gyroZ = 0;
        public float gyroZNorm = 0;
    public int jump;

    public int deviceStatus = 0; //-1 == error, 0 == not connected, 1 == connected

    wrmhl myDevice = new wrmhl();

    [Tooltip("SerialPort of your device.")]
    public string portName;

    [Tooltip("Baudrate")]
    public int baudRate = 250000;


    [Tooltip("Timeout")]
    public int ReadTimeout = 20;

    [Tooltip("QueueLength")]
    public int QueueLength = 1;

    void Awake() {
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";

        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
    }

    public void Start() {
        OnApplicationQuit();

        //connect to arduino usb if possible
        try {
            myDevice.set(portName, baudRate, ReadTimeout, QueueLength);
            myDevice.connect();
            deviceStatus = 1;
        } catch (IOException) {
            deviceStatus = 0;
            myDevice.close();
        }
    }

    void Update() {
        if (deviceStatus != 0) {
            try {
                //read string output from usb 
                var _strings = myDevice.readQueue().Split(","[0]);

                gyroX = float.Parse(_strings[0]) - gyroXNorm;
                gyroY = float.Parse(_strings[1]) - gyroYNorm;
                gyroZ = float.Parse(_strings[2]) - gyroZNorm;
                jump = int.Parse(_strings[3]);

                if (deviceStatus == -1) deviceStatus = 1;
            } catch (System.Exception) { deviceStatus = -1; }
        }
    }

    public void OnApplicationQuit() { if(deviceStatus != 0) myDevice.close(); deviceStatus = 0; }
}
