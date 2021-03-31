using UnityEngine;
using System.Collections;

public class TalkToArduino: MonoBehaviour
{
    //connect class
    public ArduinoConnect arduinoConnect;

    //call sensor readings on Arduino
    public  string[] sensorChars = { "A", "B", "C" };
    private string strEnd        = "\n";
    private int sensorIndex      = 0;

    const float Frequency = 0.1f;

    [HideInInspector]
    public bool GotHandShake = false;

    public void StartTalkingToArduino()
    {
		Camera.main.GetComponent <Camera> ().backgroundColor = Color.green;
        InvokeRepeating("SendSensorIndexToArduino", Frequency, Frequency);
    }

    public void SendSensorIndexToArduino()
    {
        arduinoConnect.WriteToArduino(sensorChars[(sensorIndex++) % sensorChars.Length] + strEnd);
        StartCoroutine(arduinoConnect.AsynchronousReadFromArduino(s => CheckCallback(s), Frequency));
    }

    public IEnumerator SendHandshake()
    {
        Debug.Log("<color=green>" + "handshake sent... waiting for reply" + "</color>\n");
        arduinoConnect.WriteToArduino("GameIsland" + strEnd);

        yield return arduinoConnect.AsynchronousReadFromArduino(s => CheckHandshake(s), Frequency);
    }

    void CheckHandshake(string str)
    {
        Debug.Log(str);
        if (str == "GameIsland")
            GotHandShake = true;
    }

    void CheckCallback(string str)
    {
        char sensorReading = str[0];

        int result = -1;
        int.TryParse(str.Substring(1), out result);

        //if garbage - return
        if (result == -1)
        {
            return;
        }

        //clean up data
        if (result < 0)
        {
            Debug.Log("<color=red>" + "Data not clean" + "</color>\n");
            result = 0;
        }

        if (result > 1024)
        {
            Debug.Log("<color=red>" + "Data not clean" + "</color>\n");
            result = 1024;
        }

        switch (sensorReading)
        {
            case 'A': 
                Debug.Log("<color=red>" + "A: " + result + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.red;
                break;

            case 'B': 
                Debug.Log("<color=orange>" + "B: " + result + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = new Color(1, 0.65f, 0);
                break;

            case 'C': 
                Debug.Log("<color=yellow>" + "C: " + result + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.yellow;
                break;

            case 'H': 
                Debug.Log("<color=grey>" + "Cathed a spare handshake " + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.gray;
                break;

            default: 
                Debug.LogError("Case not found: " + sensorReading);
                break;
        }
    }
}
