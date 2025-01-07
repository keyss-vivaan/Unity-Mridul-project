// using System.Collections.Generic;
// using TMPro;

// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using UnityEngine.XR.ARFoundation;

// [RequireComponent(typeof(ARRaycastManager))]
// public class StateManager : MonoBehaviour
// {
//     public GameObject placedPrefab;
//     public GameObject scanFloorInstruction;
//     public GameObject screenCenter;
//     public GameObject largePlane;
//     public LineRenderer lineRenderer;
//     public GameObject measurementTextPrefab;
//     public GameObject windowMeasurementTextPrefab;
//     public GameObject heightTextInMeter;
//     public GameObject heightTextInFeet;
//     public GameObject windowObject;

//     [Space(20)]

//     public GameObject UndoButton;
//     public GameObject FinishFloorButton;
//     public GameObject SetHeightButton;
//     public GameObject FinishWindowsButton;
//     public GameObject windowTypePanel;
//     public GameObject VisualizeButton;

//     [Space(20)]
//     public Camera visualizationCamera;
//     public Button meterButton;
//     public Button feetButton;


//     private List<GameObject> measurements = new List<GameObject>();
//     public List<GameObject> Measurements => measurements;

//     private List<GameObject> windowsObjects = new List<GameObject>();
//     public List<GameObject> WindowsObjects => windowsObjects;

//     private List<GameObject> wallObjects = new List<GameObject>();
//     public List<GameObject> WallObjects => wallObjects;

//     private List<GameObject> displayMeasurements = new List<GameObject>();

//     public LineRenderer floorLine;
//     public LineRenderer roofLine;

//     public GameObject createPolygonWithHolesPrefab;

//     private ARState currentState;

//     private void Start()
//     {

//         PlayerPrefs.SetInt("measurement", 0);

//         SetState(new ScanningFloorState(this));

//         floorLine = Instantiate(lineRenderer);
//         roofLine = Instantiate(lineRenderer);
//     }

//     private void LateUpdate()
//     {
//         currentState?.Update();
//     }


//     public void SetState(ARState newState)
//     {
//         currentState?.Exit();
//         currentState = newState;
//         currentState.Enter();
//     }


//     //Helper Functions
//     public bool TryGetTouchPosition(out Vector2 touchPosition)
//     {
// #if UNITY_EDITOR
//         if (Input.GetMouseButton(0))
//         {
//             var mousePosition = Input.mousePosition;
//             touchPosition = new Vector2(mousePosition.x, mousePosition.y);
//             return true;
//         }
// #else
//         if (Input.touchCount > 0)
//         {
//             touchPosition = Input.GetTouch(0).position;
//             return true;
//         }
// #endif

//         touchPosition = default;
//         return false;
//     }

//     public Vector2 ScreenCenter() => new Vector2(Screen.width / 2, Screen.height / 2);


//     public void DisplayMeasurement(Vector3 point1, Vector3 point2)
//     {
//         float distance = Vector3.Distance(point1, point2);
//         GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2, Quaternion.LookRotation(point1 - point2, Vector3.up) * Quaternion.Euler(0, 90, 0));
//         displayMeasurements.Add(g);
//         if (PlayerPrefs.GetInt("measurement") == 0)
//         {
//             g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " m";
//         }
//         else
//         {
//             distance = distance * 3.28084f;
//             g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " ft";
//         }
//     }


//     public void LoadHomeScene()
//     {
//         SceneManager.LoadScene("HomeScene");
//     }

//     public void SwitchToFt()
//     {
//         PlayerPrefs.SetInt("measurement", 1);
//         feetButton.GetComponent<Button>().interactable = false;
//         meterButton.GetComponent<Button>().interactable = true;

//         foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
//         {
//             string s = g.GetComponentInChildren<TMP_Text>().text;
//             string[] split = s.Split(' ');
//             s = split[0];
//             float f = float.Parse(s);
//             f = f * 3.28084f;
//             g.GetComponentInChildren<TMP_Text>().text = f.ToString("F2") + " ft";
//         }

//         foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
//         {
//             string s = g.GetComponentInChildren<TMP_Text>().text;
//             if (s.Length > 0)
//             {
//                 string[] split = s.Split(' ');
//                 string s1 = split[1];
//                 string s2 = split[4];
//                 float f1 = float.Parse(s1);
//                 float f2 = float.Parse(s2);
//                 f1 = f1 * 3.28084f;
//                 f2 = f2 * 3.28084f;
//                 g.GetComponentInChildren<TMP_Text>().text = split[0] + " " + f1.ToString("F2") + " ft " + split[3] + " " + f2.ToString("F2") + " ft";
//             }
//         }
//         heightTextInFeet.SetActive(true);
//         heightTextInMeter.SetActive(false);


//     }

//     public void SwitchToMeter()
//     {
//         PlayerPrefs.SetInt("measurement", 0);
//         feetButton.GetComponent<Button>().interactable = true;
//         meterButton.GetComponent<Button>().interactable = false;
//         foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
//         {
//             string s = g.GetComponentInChildren<TMP_Text>().text;
//             string[] split = s.Split(' ');
//             s = split[0];
//             float m = float.Parse(s);
//             m = m / 3.28084f;
//             g.GetComponentInChildren<TMP_Text>().text = m.ToString("F2") + " m";
//         }

//         foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
//         {
//             string s = g.GetComponentInChildren<TMP_Text>().text;
//             if (s.Length > 0)
//             {

//                 string[] split = s.Split(' ');
//                 string s1 = split[1];
//                 string s2 = split[4];
//                 float f1 = float.Parse(s1);
//                 float f2 = float.Parse(s2);
//                 f1 = f1 / 3.28084f;
//                 f2 = f2 / 3.28084f;
//                 g.GetComponentInChildren<TMP_Text>().text = split[0] + " " + f1.ToString("F2") + " m " + split[3] + " " + f2.ToString("F2") + " m";
//             }
//         }
//         heightTextInFeet.SetActive(false);
//         heightTextInMeter.SetActive(true);
//     }



// }

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class StateManager : MonoBehaviour
{
    public GameObject placedPrefab;
    public GameObject scanFloorInstruction;
    public GameObject setHeightInstruction; // New instruction for SetHeightButton
    public GameObject setDoorwindowInstruction; // New instruction for Setdoorwindow
    public GameObject screenCenter;
    public GameObject largePlane;
    public LineRenderer lineRenderer;
    public GameObject measurementTextPrefab;
    public GameObject windowMeasurementTextPrefab;
    public GameObject heightTextInMeter;
    public GameObject heightTextInFeet;
    public GameObject windowObject;

    [Space(20)]

    public GameObject UndoButton;
    public GameObject FinishFloorButton;
    public GameObject SetHeightButton;
    public GameObject FinishWindowsButton;
    public GameObject windowTypePanel;
    public GameObject VisualizeButton;

    [Space(20)]
    public Camera visualizationCamera;
    public Button meterButton;
    public Button feetButton;

    private List<GameObject> measurements = new List<GameObject>();
    public List<GameObject> Measurements => measurements;

    private List<GameObject> windowsObjects = new List<GameObject>();
    public List<GameObject> WindowsObjects => windowsObjects;

    private List<GameObject> wallObjects = new List<GameObject>();
    public List<GameObject> WallObjects => wallObjects;

    private List<GameObject> displayMeasurements = new List<GameObject>();

    public LineRenderer floorLine;
    public LineRenderer roofLine;

    public GameObject createPolygonWithHolesPrefab;

    private ARState currentState;

    private void Start()
    {
        PlayerPrefs.SetInt("measurement", 0);

        SetState(new ScanningFloorState(this));

        floorLine = Instantiate(lineRenderer);
        roofLine = Instantiate(lineRenderer);

        SetState(new ScanningFloorState(this));

        // Initially set instructions visibility
        scanFloorInstruction.SetActive(true);
        setHeightInstruction.SetActive(false);
        setDoorwindowInstruction.SetActive(false);
        
    }

    private void Update()
    {
        // Check if SetHeightButton is active and toggle setHeightInstruction visibility
        if (SetHeightButton.activeSelf)
        {
            setHeightInstruction.SetActive(true);
        }
        else
        {
            setHeightInstruction.SetActive(false);
        }

        // Check if SetHeightButton is active and toggle setHeightInstruction visibility
        if ( FinishWindowsButton.activeSelf)
        {
            setDoorwindowInstruction.SetActive(true);
        }
        else
        {
            setDoorwindowInstruction.SetActive(false);
        }

        currentState?.Update();
    }

    public void SetState(ARState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // Helper Functions
    public bool TryGetTouchPosition(out Vector2 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

        touchPosition = default;
        return false;
    }

    public Vector2 ScreenCenter() => new Vector2(Screen.width / 2, Screen.height / 2);

    public void DisplayMeasurement(Vector3 point1, Vector3 point2)
    {
        float distance = Vector3.Distance(point1, point2);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2, Quaternion.LookRotation(point1 - point2, Vector3.up) * Quaternion.Euler(0, 90, 0));
        displayMeasurements.Add(g);
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " m";
        }
        else
        {
            distance = distance * 3.28084f;
            g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " ft";
        }
    }

    public void LoadHomeScene()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void SwitchToFt()
    {
        PlayerPrefs.SetInt("measurement", 1);
        feetButton.GetComponent<Button>().interactable = false;
        meterButton.GetComponent<Button>().interactable = true;

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            string[] split = s.Split(' ');
            s = split[0];
            float f = float.Parse(s);
            f = f * 3.28084f;
            g.GetComponentInChildren<TMP_Text>().text = f.ToString("F2") + " ft";
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            if (s.Length > 0)
            {
                string[] split = s.Split(' ');
                string s1 = split[1];
                string s2 = split[4];
                float f1 = float.Parse(s1);
                float f2 = float.Parse(s2);
                f1 = f1 * 3.28084f;
                f2 = f2 * 3.28084f;
                g.GetComponentInChildren<TMP_Text>().text = split[0] + " " + f1.ToString("F2") + " ft " + split[3] + " " + f2.ToString("F2") + " ft";
            }
        }
        heightTextInFeet.SetActive(true);
        heightTextInMeter.SetActive(false);
    }

    public void SwitchToMeter()
    {
        PlayerPrefs.SetInt("measurement", 0);
        feetButton.GetComponent<Button>().interactable = true;
        meterButton.GetComponent<Button>().interactable = false;

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            string[] split = s.Split(' ');
            s = split[0];
            float m = float.Parse(s);
            m = m / 3.28084f;
            g.GetComponentInChildren<TMP_Text>().text = m.ToString("F2") + " m";
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            if (s.Length > 0)
            {
                string[] split = s.Split(' ');
                string s1 = split[1];
                string s2 = split[4];
                float f1 = float.Parse(s1);
                float f2 = float.Parse(s2);
                f1 = f1 / 3.28084f;
                f2 = f2 / 3.28084f;
                g.GetComponentInChildren<TMP_Text>().text = split[0] + " " + f1.ToString("F2") + " m " + split[3] + " " + f2.ToString("F2") + " m";
            }
        }
        heightTextInFeet.SetActive(false);
        heightTextInMeter.SetActive(true);
    }
    private bool dataReceived = false;

    // Method to handle incoming message from Flutter
    public void UpdateRoomDimensions(string jsonData)
    {
       SingletonExample.Instance.JsonData=jsonData;
        // Load the "Hello" scene if data is received
        SceneManager.LoadScene("Loading 1");
    }

}
