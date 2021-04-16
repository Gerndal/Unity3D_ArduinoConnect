using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static MonoSingleton<T> mInstance
    {
        get
        {
            try
            {
                if (!_mInstance)
                {
                    T[] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
                    if (managers.Length != 0)
                    {
                        //if (managers.Length == 1)
                        {
                            _mInstance = managers[0];
                            _mInstance.gameObject.name = typeof(T).Name;
                            return _mInstance;
                        }
                        // else
                        // {
                        //     Debug.LogError("You have more than one " + typeof(T).Name + " in the scene. You only need 1, it's a singleton!");
                        //     foreach (T manager in managers)
                        //     {
                        //         Destroy(manager.gameObject);
                        //     }
                        // }
                    }
                    GameObject gO = new GameObject(typeof(T).Name, typeof(T));
                    _mInstance = gO.GetComponent<T>();
                    //DontDestroyOnLoad(gO);
                }
                return _mInstance;
            }
            catch
            {
                return null;
            }
        }
        set
        {
            _mInstance = value as T;
        }
    }
    private static T _mInstance;

	public static T I
    {
		get
        {
			return ((T)mInstance);
		}
        set
        {
			mInstance = value;
		}
	}
	
}

public class SelectableSingleton<T> : Selectable where T : SelectableSingleton<T>
{
    protected static SelectableSingleton<T> mInstance
    {
        get
        {
            if (!_mInstance)
            {
                T[] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
                if (managers.Length != 0)
                {
                    //if (managers.Length == 1)
                    {
                        _mInstance = managers[0];
                        _mInstance.gameObject.name = typeof(T).Name;
                        return _mInstance;
                    }
                    // else
                    // {
                    //     Debug.LogError("You have more than one " + typeof(T).Name + " in the scene. You only need 1, it's a singleton!");
                    //     foreach (T manager in managers)
                    //     {
                    //         Destroy(manager.gameObject);
                    //     }
                    // }
                }
                GameObject gO = new GameObject(typeof(T).Name, typeof(T));
                _mInstance = gO.GetComponent<T>();
                //DontDestroyOnLoad(gO);
            }
            return _mInstance;
        }
        set
        {
            _mInstance = value as T;
        }
    }
    private static T _mInstance;

	public static T I
    {
		get
        {
			return ((T)mInstance);
		}
        set
        {
			mInstance = value;
		}
	}
	
}

public class SerializedMonoSinglton<T> : SerializedMonoBehaviour where T : SerializedMonoSinglton<T>
{
    protected static SerializedMonoSinglton<T> mInstance
    {
        get
        {
            if (!_mInstance)
            {
                T[] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
                if (managers.Length != 0)
                {
                    //if (managers.Length == 1)
                    {
                        _mInstance = managers[0];
                        _mInstance.gameObject.name = typeof(T).Name;
                        return _mInstance;
                    }
                    // else
                    // {
                    //     Debug.LogError("You have more than one " + typeof(T).Name + " in the scene. You only need 1, it's a singleton!");
                    //     foreach (T manager in managers)
                    //     {
                    //         Destroy(manager.gameObject);
                    //     }
                    // }
                }
                GameObject gO = new GameObject(typeof(T).Name, typeof(T));
                _mInstance = gO.GetComponent<T>();
                //DontDestroyOnLoad(gO);
            }
            return _mInstance;
        }
        set
        {
            _mInstance = value as T;
        }
    }
    private static T _mInstance;

	public static T I
    {
		get
        {
			return ((T)mInstance);
		}
        set
        {
			mInstance = value;
		}
	}
	
}
