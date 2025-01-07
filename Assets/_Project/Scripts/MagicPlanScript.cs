using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MagicPlanScript : MonoBehaviour
{
    [SerializeField] private GameObject ScanFloorInstruction;
    [SerializeField] private GameObject lineRenderer;
    [SerializeField] private GameObject largePlane;
    [SerializeField] private GameObject screenCenter;
    [SerializeField] private GameObject measurementTextPrefab;
    [SerializeField] private GameObject heightText;
    [SerializeField] private GameObject placedPrefab;
    [SerializeField] private GameObject lightWarningText;

    [Space(25)]

    [SerializeField] private GameObject AddPointButton;
    [SerializeField] private GameObject UndoButton;
    [SerializeField] private GameObject FinishFloorButton;
    [SerializeField] private GameObject SetHeightButton;



    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private ARRaycastManager m_RaycastManager;
    private GameObject tempMeasurement;
    private List<GameObject> measurements = new List<GameObject>();
    private List<GameObject> displayMeasurements = new List<GameObject>();
    private LineRenderer floorLine;
    private LineRenderer roofLine;
    private float heightMark = 0;
    private Light scene_light;


    private LineRenderer tempFloorLine;
    private LineRenderer tempRoofLine;
    private GameObject tempText;

    private bool isFloorDrawn = false;
    private bool isHeightSet = false;
    private bool instruction = true;



    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        scene_light = GameObject.Find("Directional Light").GetComponent<Light>();

        GameObject floorLineObject = Instantiate(lineRenderer);
        floorLine = floorLineObject.GetComponent<LineRenderer>();

        GameObject tempFloorLineObject = Instantiate(lineRenderer);
        tempFloorLine = tempFloorLineObject.GetComponent<LineRenderer>();
        tempFloorLine.positionCount = 2;

        GameObject tempRoofLineObject = Instantiate(lineRenderer);
        tempRoofLine = tempRoofLineObject.GetComponent<LineRenderer>();
        tempRoofLine.positionCount = 2;

        GameObject roofLineObject = Instantiate(lineRenderer);
        roofLine = roofLineObject.GetComponent<LineRenderer>();

        tempText = Instantiate(measurementTextPrefab);
        tempText.tag = "Untagged";

        SetHeightButton.SetActive(false);
        heightText.SetActive(false);

    }

    void Update()
    {
        if (scene_light.intensity < 0.3f)
        {
            lightWarningText.SetActive(true);
        }
        else
        {
            lightWarningText.SetActive(false);
        }


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
            tempMeasurement = Instantiate(placedPrefab, floorPos, Quaternion.identity);

        }

        Ray ray = Camera.main.ScreenPointToRay(ScreenCenter());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && tempMeasurement != null)
        {
            tempMeasurement.transform.position = hit.point;

            if (measurements.Count > 2 && Vector3.Distance(measurements[0].transform.position, tempMeasurement.transform.position) < 0.1f)
            {
                tempMeasurement.transform.position = measurements[0].transform.position;
            }
            if (measurements.Count > 0)
            {
                tempFloorLine.SetPosition(0, measurements[measurements.Count - 1].transform.position);
                tempFloorLine.SetPosition(1, tempMeasurement.transform.position);

                tempRoofLine.SetPosition(0, measurements[measurements.Count - 1].transform.position + new Vector3(0, 0.5f, 0));
                tempRoofLine.SetPosition(1, tempMeasurement.transform.position + new Vector3(0, 0.5f, 0));

                float distance = Vector3.Distance(measurements[measurements.Count - 1].transform.position, tempMeasurement.transform.position);
                tempText.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
                tempText.transform.position = (measurements[measurements.Count - 1].transform.position + tempMeasurement.transform.position) / 2;
                tempText.transform.rotation = Quaternion.LookRotation(measurements[measurements.Count - 1].transform.position - tempMeasurement.transform.position, Vector3.up) * Quaternion.Euler(0, 90, 0);

            }
            else
            {
                tempFloorLine.SetPosition(0, tempMeasurement.transform.position);
                tempFloorLine.SetPosition(1, tempMeasurement.transform.position);

                tempRoofLine.SetPosition(0, tempMeasurement.transform.position + new Vector3(0, 0.5f, 0));
                tempRoofLine.SetPosition(1, tempMeasurement.transform.position + new Vector3(0, 0.5f, 0));

                tempText.GetComponentInChildren<TMP_Text>().text = "";

            }

        }

        if (Physics.Raycast(ray, out hit) && isFloorDrawn && !isHeightSet)
        {
            var hitPose = hit.point;
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
        }


    }

    public void AddPoint()
    {
        if (measurements.Count > 2 && Vector3.Distance(measurements[0].transform.position, tempMeasurement.transform.position) < 0.1f)
        {
            tempMeasurement.transform.position = measurements[0].transform.position;
            FinishAddingPoints();
            return;
        }
        Vector3 point = tempMeasurement.transform.position;
        measurements.Add(Instantiate(placedPrefab, point, Quaternion.identity));
        UpdatePositions();


    }

    public void FinishAddingPoints()
    {
        Destroy(tempMeasurement);

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


        SetHeightButton.SetActive(true);
        UndoButton.SetActive(false);
        AddPointButton.SetActive(false);
        FinishFloorButton.SetActive(false);

        CreateWallBase();
    }

    public void Undo()
    {
        Destroy(measurements[measurements.Count - 1]);
        Destroy(displayMeasurements[displayMeasurements.Count - 1]);
        measurements.Remove(measurements[measurements.Count - 1]);
        displayMeasurements.Remove(displayMeasurements[displayMeasurements.Count - 1]);

        UpdatePositions();
    }

    public void SetHeight()
    {
        isHeightSet = true;
        SetHeightButton.SetActive(false);
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

    private void DisplayMeasurement(Vector3 point1, Vector3 point2)
    {
        float distance = Vector3.Distance(point1, point2);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2, Quaternion.LookRotation(point1 - point2, Vector3.up) * Quaternion.Euler(0, 90, 0));
        displayMeasurements.Add(g);
        g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
    }

    private Vector2 ScreenCenter()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("HomeScene");
    }
}

