﻿using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

public class ArduinoConnect : MonoBehaviour
{
    private SerialPort stream;

    public void Open(string _portName, int _baudrate)
    {
        stream = new SerialPort(_portName, _baudrate, Parity.None, 8, StopBits.One);
        stream.ReadTimeout = 50;
        stream.Open();
    }

    public void WriteToArduino(string message)
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

    // public string ReadFromArduino(int timeout = 0)
    // {
    //     stream.ReadTimeout = timeout;
    //     try
    //     {
    //         return stream.ReadLine();
    //     }
    //     catch (TimeoutException)
    //     {
    //         return null;
    //     }
    // }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
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
                callback(dataString);
            
            yield return new WaitForEndOfFrame();

            diff = DateTime.Now - initialTime;
        } while (diff.Milliseconds < timeout);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        stream.Close();
    }
}