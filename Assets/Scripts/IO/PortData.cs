using UnityEngine;

public class PortData : MonoBehaviour
{
	public static PortData Current;
	public string USBportName;
	public int baudrate = 57600;

	void Awake ()
	{
		Current = this;
	}
}
