using UnityEngine;
using System.Collections;

public class TalkToArduino : MonoBehaviour
{
    // private BlcokController blockController;

    // Use this for initialization
    void Start()
    {
        // blockController = GameObject.Find("BlockController").GetComponent<BlcokController>();
    }

    //call sensor readings on Arduino
    public string[] sensorChars = { "A", "B", "C" };

    public void CheckCallback(string str)
    {
        if (str == null) return;

        UnityMainThreadDispatcher.I.Enqueue(() =>
        {
            // if (blockController.actionBTN.ContainsKey(str))
            // {
            //     StartCoroutine(blockController.actionBTN[str]());
            //     blockController.UIshow();
            // }

            switch (str)
            {
                case "GameIsland":
                    ("Congratulation GameIsland Handshake").Log(Color.magenta);
                break;
            }
        });
    }
}
