using System.ComponentModel;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ScanPorts: MonoBehaviour
{
    private SerialPort stream;
    public  int baudrate            = 57600;
    public  int readTimeout         = 50;
    public  float handShakeDuration = 1.0f;

    private string strEnd         = "\n";
    private const float Frequency = 0.06f;

    [Space]
    public  TalkToArduino talkToArduino;
    private bool isHandShake = false;

    void Start()
    {
        StartCoroutine(CoScanForArduinoPort());
    }

    public void SerialOpen(string _portName, int _baudrate)
    {
        Debug.Log("<color=green>" + "Trying with port: " + _portName + "</color>\n");
        
        stream             = new SerialPort(_portName, _baudrate, Parity.None, 8, StopBits.One);
        stream.ReadTimeout = readTimeout;
        stream.Open();
    }

    public void SerialWrite(string message)
    {
        try
        {
            stream.WriteLine(message);
            stream.BaseStream.Flush();
        }
        catch (TimeoutException)
        {

        }
    }

    public string SerialRead(int timeout=0)
    {
        stream.ReadTimeout = timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    public IEnumerator CoAsynchronousSerialRead(Action<string> callback, float timeout=float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        TimeSpan diff        = default(TimeSpan);

        do
        {
            var dataString = SerialRead(readTimeout);
            if (dataString != null)
            {
                callback(dataString);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            diff = DateTime.Now - initialTime;
        } while (diff.Milliseconds < timeout);
    }

    public IEnumerator CoScanForArduinoPort()
    {
        Debug.Log("<color=green>" + "Scanning for arduino port" + "</color>\n");

        while (isHandShake == false)
        {
            foreach (var port in ScanUSBPortNames())
            {
                SerialOpen(port, baudrate);
                yield return StartCoroutine(CoSendHandshake());

                if (isHandShake) yield break;
                yield return new WaitForSeconds(handShakeDuration);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator CoSendSensorIndexToArduino()
    {
        while (true)
            yield return CoAsynchronousSerialRead(s => talkToArduino.CheckCallback(s), Frequency);
    }

    public IEnumerator CoSendHandshake()
    {
        Debug.Log("<color=green>" + "handshake sent... waiting for reply" + "</color>\n");

        SerialWrite("GameIsland" + strEnd);
        yield return CoAsynchronousSerialRead(s => CheckHandshake(s), handShakeDuration);
    }

    void CheckHandshake(string str)
    {
        Debug.Log(str);

        isHandShake = (str == "GameIsland");
        if (isHandShake == false) return;

        talkToArduino.CheckCallback(str);

        Debug.Log("<color=green>" + "Arduino Connected Port is : " + stream.PortName + "</color>\n");
        StartCoroutine(CoSendSensorIndexToArduino());
    }

    private List<string> ScanUSBPortNames()
    {
        var usbPortNames = new List<string>();
        var platform     = System.Environment.OSVersion.Platform;

        if ((platform == PlatformID.Unix  ) ||
            (platform == PlatformID.MacOSX))
        {
            var ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");
            ttys?.ToList().ForEach(dev => 
            { 
                if (dev.StartsWith("/dev/tty.")) usbPortNames.Add(dev); 
            });
        }
        else
        {
            SerialPort.GetPortNames()?.ToList().ForEach(p => usbPortNames.Add(p));
        }

        return usbPortNames;
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        stream.Close();
    }
}
