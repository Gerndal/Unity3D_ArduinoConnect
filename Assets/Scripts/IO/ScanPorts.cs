using System.ComponentModel;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ScanPorts: MonoBehaviour
{
    public string USB_PortName;
    public int baudrate = 57600;

    [Space]
    public  ArduinoConnect arduinoConnect;
    public  TalkToArduino talkToArduino;

    private List<string> usbPortNames;
    private bool NoPortYet = true;

    void Start()
    {
        StartCoroutine(ScanForArduinoPort());
    }

    public void StopScan()
    {
        StopAllCoroutines();
    }

    public IEnumerator ScanForArduinoPort()
    {
        Debug.Log("<color=green>" + "Scanning for arduino port" + "</color>\n");

        NoPortYet = true;
        while (NoPortYet)
        {
            yield return StartCoroutine(GetArduinoPort());
        }
    }

    public IEnumerator GetArduinoPort()
    {
        yield return StartCoroutine(ScanUSBPortNames());

        foreach (var port in usbPortNames)
        {
            arduinoConnect.Open(port, baudrate);
            yield return StartCoroutine(talkToArduino.SendHandshake());
            Debug.Log("<color=green>" + "Trying with port: " + port + "</color>\n");

            if (talkToArduino.GotHandShake)
            {
                USB_PortName = port;
                NoPortYet    = false;

                Debug.Log("<color=green>" + "Arduino port is: " + USB_PortName + "</color>\n");
                talkToArduino.StartTalkingToArduino();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    //list usb ports on mac
    IEnumerator ScanUSBPortNames()
    {
            usbPortNames = new List<string>();
        var p            = System.Environment.OSVersion.Platform;

        // Are we on Unix?
        if ((p == PlatformID.Unix  ) ||
            (p == PlatformID.MacOSX))
        {
            string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");
            yield return new WaitForEndOfFrame();

            foreach (string dev in ttys)
            {
                if (dev.StartsWith("/dev/tty.")) usbPortNames.Add(dev);
            }
        }
        else
        {
            SerialPort.GetPortNames().ToList().ForEach(p => usbPortNames.Add(p));
        }
    }
}
