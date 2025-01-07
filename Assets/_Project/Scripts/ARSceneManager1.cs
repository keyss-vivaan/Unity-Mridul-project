using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARSceneManager1 : MonoBehaviour
{
    [SerializeField] private GameObject placedPrefab;
    [SerializeField] private GameObject ScanFloorInstruction;
    [SerializeField] private GameObject screenCenter;
    [SerializeField] private GameObject largePlane;
    [SerializeField] private GameObject lineRenderer;
    [SerializeField] private GameObject measurementTextPrefab;
    [SerializeField] private GameObject heightText;

    [SerializeField] private GameObject UndoButton;
    [SerializeField] private GameObject FinishFloorButton;
    [SerializeField] private GameObject SetHeightButton;


    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private ARRaycastManager m_RaycastManager;
    private List<GameObject> measurements = new List<GameObject>();
    private List<GameObject> displayMeasurements = new List<GameObject>();
    private LineRenderer floorLine;
    private LineRenderer roofLine;

    private GameObject spawnedObject;
    private LineRenderer tempFloorLine;
    private LineRenderer tempRoofLine;
    private GameObject tempText;

    private bool instruction = true;
    private bool isFloorDrawn = false;
    private bool isHeightSet = false;
    private float heightMark = 0f;


    private void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        GameObject floorLineObject = Instantiate(lineRenderer);
        floorLine = floorLineObject.GetComponent<LineRenderer>();

        GameObject tempFloorLineObject = Instantiate(lineRenderer);
        tempFloorLine = tempFloorLineObject.GetComponent<LineRenderer>();


        GameObject tempRoofLineObject = Instantiate(lineRenderer);
        tempRoofLine = tempRoofLineObject.GetComponent<LineRenderer>();


        GameObject roofLineObject = Instantiate(lineRenderer);
        roofLine = roofLineObject.GetComponent<LineRenderer>();

        tempText = Instantiate(measurementTextPrefab);
        tempText.tag = "Untagged";

        SetHeightButton.SetActive(false);
    }

    private void LateUpdate()
    {
        //Check if there is an ARPlane exist in the scene. If yes, then disable the scan instruction
        // if (GameObject.FindGameObjectsWithTag("ARPlane").Length > 0)
        // {
        //     ScanFloorInstruction.SetActive(false);
        //     screenCenter.SetActive(true);
        // }




        //This automatically find the floor and places a large plane on the floor
        if (GameObject.FindGameObjectsWithTag("ARPlane").Length > 0 && instruction)
        {
            Debug.Log("Found floor");
            Vector3 floorPos = GameObject.FindGameObjectsWithTag("ARPlane")[0].transform.position;
            GameObject largePlane1 = Instantiate(largePlane, floorPos, Quaternion.identity);
            ScanFloorInstruction.SetActive(false);
            instruction = false;
            screenCenter.SetActive(true);
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("ARPlane"))
            {
                g.SetActive(false);
            }
            gameObject.GetComponent<ARPlaneManager>().enabled = false;

        }

        // If the user is clicking UI, it should not interact with AR Scene
        if (EventSystem.current.IsPointerOverGameObject() || !TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(ScreenCenter());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && Input.touchCount > 0 && !isFloorDrawn)
        {
            var hitPose = hit.point;
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:

                    spawnedObject = Instantiate(placedPrefab, hitPose, Quaternion.identity);
                    break;
                case TouchPhase.Moved:

                    spawnedObject.transform.position = hitPose;

                    if (measurements.Count > 2 && Vector3.Distance(measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                    {
                        spawnedObject.transform.position = measurements[0].transform.position;
                    }

                    if (measurements.Count > 0)
                    {
                        tempFloorLine.positionCount = 2;
                        tempRoofLine.positionCount = 2;

                        tempFloorLine.SetPosition(0, measurements[measurements.Count - 1].transform.position);
                        tempFloorLine.SetPosition(1, spawnedObject.transform.position);

                        tempRoofLine.SetPosition(0, measurements[measurements.Count - 1].transform.position + new Vector3(0, 0.5f, 0));
                        tempRoofLine.SetPosition(1, spawnedObject.transform.position + new Vector3(0, 0.5f, 0));

                        float distance = Vector3.Distance(measurements[measurements.Count - 1].transform.position, spawnedObject.transform.position);
                        tempText.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
                        tempText.transform.position = (measurements[measurements.Count - 1].transform.position + spawnedObject.transform.position) / 2;
                        tempText.transform.rotation = Quaternion.LookRotation(measurements[measurements.Count - 1].transform.position - spawnedObject.transform.position, Vector3.up) * Quaternion.Euler(0, 90, 0);
                    }

                    break;

                case TouchPhase.Ended:
                    spawnedObject.transform.position = hitPose;
                    if (measurements.Count > 2 && Vector3.Distance(measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                    {
                        spawnedObject.transform.position = measurements[0].transform.position;
                        FinishAddingPoints();
                        return;
                    }
                    measurements.Add(spawnedObject);
                    RemoveTempObjects();
                    UpdatePositions();
                    break;
            }
        }

        if (Physics.Raycast(ray, out hit) && Input.touchCount > 0 && isFloorDrawn && !isHeightSet)
        {
            var hitPose = hit.point;
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    break;
                case TouchPhase.Moved:
                    if (hitPose.y > measurements[0].transform.position.y + 0.5f)
                    {
                        heightMark = hitPose.y;
                    }
                    else
                    {
                        heightMark = measurements[0].transform.position.y + 0.5f;
                    }
                    heightText.GetComponentInChildren<TMP_Text>().text = "Height : " + (heightMark - measurements[0].transform.position.y).ToString("F2") + "m";
                    for (int i = 0; i < roofLine.positionCount; i++)
                    {
                        roofLine.SetPosition(i, new Vector3(roofLine.GetPosition(i).x, heightMark, roofLine.GetPosition(i).z));
                    }

                    for (int i = 0; i < measurements.Count; i++)
                    {
                        measurements[i].GetComponentInChildren<FollowPin>().height = heightMark - measurements[0].transform.position.y;
                    }
                    break;
                case TouchPhase.Ended:
                    break;
            }

        }
    }

    private void UpdatePositions()
    {
        floorLine.positionCount = measurements.Count;
        roofLine.positionCount = measurements.Count;

        for (int i = 0; i < measurements.Count; i++)
        {
            floorLine.SetPosition(i, measurements[i].transform.position);
            roofLine.SetPosition(i, measurements[i].transform.position + new Vector3(0, 0.5f, 0));
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
        {
            Destroy(g);
        }
        for (int i = 0; i < measurements.Count - 1; i++)
        {
            DisplayMeasurement(measurements[i].transform.position, measurements[i + 1].transform.position);
        }
    }

    public void FinishAddingPoints()
    {

        floorLine.positionCount = measurements.Count + 1;
        roofLine.positionCount = measurements.Count + 1;
        for (int i = 0; i < measurements.Count; i++)
        {
            floorLine.SetPosition(i, measurements[i].transform.position);
            roofLine.SetPosition(i, measurements[i].transform.position + new Vector3(0, 0.5f, 0));
        }
        floorLine.SetPosition(measurements.Count, measurements[0].transform.position);
        roofLine.SetPosition(measurements.Count, measurements[0].transform.position + new Vector3(0, 0.5f, 0));

        Debug.Log(roofLine.positionCount);

        Destroy(tempRoofLine);
        Destroy(tempFloorLine);

        DisplayMeasurement(measurements[measurements.Count - 1].transform.position, measurements[0].transform.position);

        Destroy(tempText);
        isFloorDrawn = true;

        heightText.SetActive(true);


        CreateWallBase();

        SetHeightButton.SetActive(true);
        UndoButton.SetActive(false);
        FinishFloorButton.SetActive(false);


    }


    private void CreateWallBase()
    {
        for (int i = 0; i < measurements.Count; i++)
        {
            int j = i + 1;
            List<Vector3> wallPoints = new List<Vector3>();
            if (i == measurements.Count - 1)
            {
                j = 0;
            }
            wallPoints.Add(measurements[j].transform.position);
            wallPoints.Add(measurements[i].transform.position);
            wallPoints.Add(measurements[i].transform.position + new Vector3(0, 5, 0));
            wallPoints.Add(measurements[j].transform.position + new Vector3(0, 5, 0));


            GameObject wall = new GameObject("wall" + i);
            wall.tag = "Wall";
            wall.AddComponent<MeshFilter>();
            wall.AddComponent<MeshRenderer>();
            wall.AddComponent<MeshCollider>();
            wall.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            wall.GetComponent<MeshRenderer>().enabled = false;
            CreatePolygon generateMesh2 = wall.AddComponent<CreatePolygon>();

            generateMesh2.points = wallPoints;

        }
    }

    public void SetHeight()
    {
        isHeightSet = true;
        SetHeightButton.SetActive(false);
    }

    public void Undo()
    {
        //Placing temporary objects out of signt.
        RemoveTempObjects();

        if (measurements.Count > 0)
        {
            Destroy(measurements[measurements.Count - 1]);
            Destroy(displayMeasurements[displayMeasurements.Count - 1]);
            measurements.Remove(measurements[measurements.Count - 1]);
            displayMeasurements.Remove(displayMeasurements[displayMeasurements.Count - 1]);
        }

        UpdatePositions();
    }

    private void RemoveTempObjects()
    {
        tempFloorLine.transform.position = new Vector3(1000, 1000, 1000);
        tempFloorLine.GetComponent<LineRenderer>().positionCount = 0;
        tempRoofLine.transform.position = new Vector3(1000, 1000, 1000);
        tempRoofLine.GetComponent<LineRenderer>().positionCount = 0;
        tempText.transform.position = new Vector3(1000, 1000, 1000);

    }

    public void RestartScene()
    {
        SceneManager.LoadScene("HomeScene");
    }





    //Helping Functions
    private Vector2 ScreenCenter()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
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

    private void DisplayMeasurement(Vector3 point1, Vector3 point2)
    {
        float distance = Vector3.Distance(point1, point2);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2, Quaternion.LookRotation(point1 - point2, Vector3.up) * Quaternion.Euler(0, 90, 0));
        displayMeasurements.Add(g);
        g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
    }

}
