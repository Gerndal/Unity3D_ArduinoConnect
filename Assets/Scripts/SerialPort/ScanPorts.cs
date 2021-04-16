using System.IO;
using System.ComponentModel;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using Sirenix.OdinInspector;

public class ScanPorts: MonoBehaviour
{
    public GameObject debugView;

    private SerialPort stream;
    public int baudrate            = 57600;
    public int readTimeout         = 50;
    public float handShakeDuration = 1.0f;

    private Thread threadSerialDataReceived;

    private string strEnd         = "\n";
    private const float Frequency = 0.06f;

    [Space]
    public TalkToArduino talkToArduino;
    private bool isHandShake = false;


    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftAlt) &&
            Input.GetKeyDown(KeyCode.V))
        {
            debugView.SetActive(!debugView.activeSelf);
        }
    }

    [Button]
    void Start()
    {
        if (stream == null)
            UnityMainThreadDispatcher.I?.Enqueue(() => StartCoroutine(CoScanForArduinoPort()));
    }

    public void SerialOpen(string _portName, int _baudrate)
    {
        try
        {
            ($"Trying with port: {_portName} !").Log();
            stream?.Close();
            stream             = new SerialPort(_portName, _baudrate, Parity.None, 8, StopBits.One);
            stream.ReadTimeout = readTimeout;
            stream.Open();
        }
        catch (IOException)
        {
            stream.Close();
            "Connection Retry !!!!!!!!!!!!!!!!!!!!!!!!".Log(Color.red);
        }
    }

    public void SerialWrite(string message)
    {
        try
        {
            if (stream.IsOpen == false) return;

            stream.WriteLine(message + strEnd);
            stream.BaseStream.Flush();
        }
        catch (TimeoutException)
        {
        }
        catch (Exception)
        {
            Reconnect();
        }
    }

    public string SerialRead(int timeout = 0)
    {
        if (stream == null) return null;
        
        stream.ReadTimeout = timeout;
        try
        {
            return stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
        catch (Exception)
        {
            Reconnect();
            return null;
        }
    }

    private void Reconnect()
    {
        if (isHandShake == false) return;

        stream?.Close();
        stream = null;
        threadSerialDataReceived.Interrupt();
        Start();
    }

    public void AsynchronousSerialDataReceived()
    {
        while (true)
        {
            try
            {
                var dataString = SerialRead(readTimeout);
                talkToArduino.CheckCallback(dataString);
                Thread.Sleep(2);
            }
            catch
            {
                ($"AsynchronousSerialDataReceived ").Log(Color.red);
                break;
            }
        }
    }

    public IEnumerator CoAsynchronousSerialRead(Action<string> callback, float timeout = float.PositiveInfinity)
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
        ($"Scan For Arduino Port").Log();
        isHandShake = false;
        while (isHandShake == false)
        {
            foreach (var port in ScanUSBPortNames())
            {
                SerialOpen(port, baudrate);
                yield return StartCoroutine(CoSendHandshake());
                if (isHandShake) yield break;

                yield return new WaitForSeconds(3);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator CoSendHandshake()
    {
        ($"handshake sent... waiting for reply").Log();

        SerialWrite("GameIsland");
        yield return CoAsynchronousSerialRead(s => CheckHandshake(s), handShakeDuration);
    }

    void CheckHandshake(string str)
    {
        ($"CheckHandshake {str}").Log();

        isHandShake = (str == "GameIsland");
        if (isHandShake == false) return;

        talkToArduino.CheckCallback(str);

        ($"Arduino Connected Port is : {stream.PortName}").Log(Color.yellow);
        threadSerialDataReceived = new Thread(AsynchronousSerialDataReceived);
        threadSerialDataReceived.Start();
    }

    private List<string> ScanUSBPortNames()
    {
        var usbPortNames = new List<string>();
        var platform     = System.Environment.OSVersion.Platform;

        if ((platform == PlatformID.Unix) ||
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

    private void OnDisable()
    {
        if (threadSerialDataReceived != null) threadSerialDataReceived.Interrupt();
        stream.BaseStream.Dispose();
        stream.Close();
        "OnApplicationQuit".Log();
    }
}
