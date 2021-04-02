using UnityEngine;
using System.Collections;

public class TalkToArduino: MonoBehaviour
{
    //call sensor readings on Arduino
    public  string[] sensorChars  = { "A", "B", "C" };

    public void CheckCallback(string str)
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
            case "GameIsland": 
                Debug.Log("<color=grey>" + "Cathed a spare handshake " + "</color>\n");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.magenta;
            break;
            default: 
                Debug.Log("Button Up");
                Camera.main.GetComponent<Camera>().backgroundColor = Color.cyan;
            break;
        }
    }
}
