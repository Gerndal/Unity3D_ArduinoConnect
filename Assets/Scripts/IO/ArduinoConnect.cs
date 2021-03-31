﻿using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

public class ArduinoConnect : MonoBehaviour
{
    private SerialPort stream;

    public void Open(string portName)
    {
        stream = new SerialPort(portName, PortData.Current.baudrate, Parity.None, 8, StopBits.One);
        stream.ReadTimeout = 50;
        stream.Open();
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        try
        {
            stream.WriteLine(message);
            stream.BaseStream.Flush();
        }
        catch (TimeoutException)
        {

        }
    }

    public string ReadFromArduino(int timeout = 0)
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

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            // A single read attempt
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield break;
            }
            else
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

            yield return new WaitForEndOfFrame();
        } while (diff.Milliseconds < timeout);

        yield return null;
    }

    void OnApplicationQuit()
    {
        stream.Close();
    }
}