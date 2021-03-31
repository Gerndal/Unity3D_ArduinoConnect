﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ScanPorts : MonoBehaviour
{
    public string USBportName;
    public static int baudrate = 57600;

    [Space]
    public ArduinoConnect arduinoConnect;
    public TalkToArduino talkToArduino;
    [HideInInspector]
    public List<string> usbPortNames = new List<string>();

    [HideInInspector]
    bool NoPortYet = true;

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

        yield return null;
    }

    public IEnumerator GetArduinoPort()
    {
        //get fresh list of portnames
        yield return StartCoroutine(ScanUSBPortNames());

        //set recieved handshake to false
        NoPortYet = true;

        //loop through ports looking for handshake
        foreach (var item in usbPortNames)
        {
            //arduinoConnect
            arduinoConnect.Open(item);
            yield return StartCoroutine(talkToArduino.SendHandshake());

            Debug.Log("<color=green>" + "Trying with port: " + item + "</color>\n");

            //got handshake is true
            if (talkToArduino.GotHandShake)
            {
                //found port... do connect
                USBportName = item;
                NoPortYet = false;

                Debug.Log("<color=green>" + "Arduino port is: " + USBportName + "</color>\n");

                talkToArduino.StartTalkingToArduino();

                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    public IEnumerator ScanUSBPortNames()
    {
        //clear here...
        usbPortNames.Clear();
        yield return StartCoroutine(GetPortNames());
    }

    //list usb ports on mac
    IEnumerator GetPortNames()
    {
        int p = (int)System.Environment.OSVersion.Platform;

        // Are we on Unix?
        if (p == 4 || p == 128 || p == 6)
        {
            string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");
            foreach (string dev in ttys)
            {
                if (dev.StartsWith("/dev/tty."))
                {
                    usbPortNames.Add(dev);
                }
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            SerialPort.GetPortNames().ToList().ForEach(p => usbPortNames.Add(p));
        }
    }
}
