using UnityEngine;

public class SingletonExample : MonoBehaviour
{
    private static SingletonExample _instance;

    public static SingletonExample Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("SingletonExample").AddComponent<SingletonExample>();
                DontDestroyOnLoad(_instance.gameObject); // Persist across scenes
            }
            return _instance;
        }
    }

    public string JsonData { get; set; } // Store the JSON data from Flutter

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
