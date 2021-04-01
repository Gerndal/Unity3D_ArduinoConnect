using UnityEngine;
using System.Collections;

public class TalkToArduino : MonoBehaviour
{
    //connect class
    public ArduinoConnect arduinoConnect;

    //call sensor readings on Arduino
    public string[] sensorChars = { "A", "B", "C" };
    private string strEnd = "\n";
    private int sensorIndex = 0;

    const float Frequency = 0.06f;

    [HideInInspector]
    public bool GotHandShake = false;

    public void StartTalkingToArduino()
    {
        Camera.main.GetComponent<Camera>().backgroundColor = Color.green;
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

        yield return arduinoConnect.AsynchronousReadFromArduino(s => CheckHandshake(s), 1);
    }

    void CheckHandshake(string str)
    {
        Debug.Log(str);
        if (str == "GameIsland")
            GotHandShake = true;
    }

    void CheckCallback(string str)
    {
        switch (str)
        {
            case "A":
                Debug.Log("<color=blue>" + "A" + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.blue;
            break;
            case "B":
                Debug.Log("<color=yellow>" + "B" + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.yellow;
            break;
            case "C":
                Debug.Log("<color=red>" + "C" + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.red;
            break;
            case "H":
                Debug.Log("<color=grey>" + "Cathed a spare handshake " + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.magenta;
            break;
            default:
                Debug.Log("Button Up");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.gray;
            break;
        }
    }
}
