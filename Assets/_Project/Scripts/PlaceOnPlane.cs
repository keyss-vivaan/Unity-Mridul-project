using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Screen = UnityEngine.Device.Screen;
using EasyUI.Toast;
using System.Linq;
using System.Threading;


[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField] GameObject m_PlacedPrefab;
    [SerializeField] private GameObject ScanFloorInstruction;
    [SerializeField] private GameObject lineRenderer;
    [SerializeField] private GameObject measurementTextPrefab;
    [SerializeField] private GameObject largePlane;
    [SerializeField] private GameObject screenCenter;
    [SerializeField] private GameObject heightText;

    [SerializeField] private GameObject FinishFloorButton;

    public GameObject spawnedObject { get; private set; }

    private List<Vector3> points = new List<Vector3>();
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private ARRaycastManager m_RaycastManager;
    private Vector3 screenCenterScale;

    private LineRenderer floorLine = new LineRenderer();
    private LineRenderer roofLine = new LineRenderer();
    private List<LineRenderer> wallLines = new List<LineRenderer>();


    private bool instruction = true;
    private bool objectSpawned = false;
    private bool firstPin = true;
    private bool isFloorDrawn = false;
    private bool isHeightMeasured = false;



    private GameObject tempText;
    private Plane plane;

    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        tempText = Instantiate(measurementTextPrefab);
        tempText.SetActive(false);
        screenCenterScale = screenCenter.transform.localScale;


        GameObject floorLineObject = Instantiate(lineRenderer);
        floorLine = floorLineObject.GetComponent<LineRenderer>();

        GameObject roofLineObject = Instantiate(lineRenderer);
        roofLine = roofLineObject.GetComponent<LineRenderer>();

    }





    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("ARPlane").Length > 0)
        {
            ScanFloorInstruction.SetActive(false);
            instruction = false;
            screenCenter.SetActive(true);
        }

        if (EventSystem.current.IsPointerOverGameObject() || !TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }

        if (m_RaycastManager.Raycast(ScreenCenter(), s_Hits, TrackableType.PlaneWithinPolygon) && !objectSpawned && !isFloorDrawn)
        {
            var hitPose = s_Hits[0].pose;
            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);

            if (firstPin)
            {
                GameObject largePlane1 = Instantiate(largePlane, hitPose.position, hitPose.rotation);
                plane = CreatePlane(largePlane1);
                firstPin = false;
            }
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("ARPlane"))
            {
                g.SetActive(false);
            }
            gameObject.GetComponent<ARPlaneManager>().enabled = false;
            objectSpawned = true;

        }

        Ray ray = Camera.main.ScreenPointToRay(ScreenCenter());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && !isFloorDrawn)
        {
            var hitPose = hit.point;
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:

                    if (!objectSpawned)
                    {
                        spawnedObject = Instantiate(m_PlacedPrefab, hitPose, Quaternion.identity);
                        objectSpawned = true;
                    }
                    break;

                case TouchPhase.Moved:
                    if (objectSpawned && spawnedObject != null)
                    {
                        tempText.SetActive(true);
                        spawnedObject.transform.position = hitPose;
                        if (Vector3.Distance(spawnedObject.transform.position, points[0]) < 0.1f)
                        {
                            spawnedObject.transform.position = points[0];
                        }
                        tempText.GetComponentInChildren<TMP_Text>().text = Vector3.Distance(spawnedObject.transform.position, points[points.Count - 1]).ToString("F2") + "m";
                        tempText.transform.position = (spawnedObject.transform.position + points[points.Count - 1]) / 2;
                        tempText.transform.rotation = Quaternion.LookRotation(points[points.Count - 1] - spawnedObject.transform.position, Vector3.up) * Quaternion.Euler(0, 90, 0);

                        floorLine.positionCount = points.Count + 1;
                        floorLine.SetPosition(points.Count, spawnedObject.transform.position);

                        roofLine.positionCount = points.Count + 1;
                        roofLine.SetPosition(points.Count, new Vector3(spawnedObject.transform.position.x, spawnedObject.transform.position.y + 0.5f, spawnedObject.transform.position.z));

                        spawnedObject.GetComponentInChildren<LineRenderer>().SetPosition(0, spawnedObject.transform.position);
                        spawnedObject.GetComponentInChildren<LineRenderer>().SetPosition(1, spawnedObject.transform.position + new Vector3(0, 0.5f, 0));

                        screenCenter.transform.localScale = screenCenterScale * 1.2f;

                    }
                    break;

                case TouchPhase.Ended:
                    if (objectSpawned && spawnedObject != null)
                    {
                        tempText.SetActive(false);
                        spawnedObject.transform.position = hitPose;
                        points.Add(spawnedObject.transform.position);
                        objectSpawned = false;

                        if (Vector3.Distance(spawnedObject.transform.position, points[0]) < 0.1f && points.Count > 1)
                        {
                            spawnedObject.transform.position = points[0];
                            points.RemoveAt(points.Count - 1);
                            FinishAddingPoints();
                            return;
                        }

                        floorLine.positionCount = points.Count;
                        floorLine.SetPosition(points.Count - 1, spawnedObject.transform.position);

                        roofLine.positionCount = points.Count;
                        roofLine.SetPosition(points.Count - 1, new Vector3(spawnedObject.transform.position.x, spawnedObject.transform.position.y + 0.5f, spawnedObject.transform.position.z));

                        spawnedObject.GetComponentInChildren<LineRenderer>().SetPosition(0, spawnedObject.transform.position);
                        spawnedObject.GetComponentInChildren<LineRenderer>().SetPosition(1, spawnedObject.transform.position + new Vector3(0, 0.5f, 0));


                        screenCenter.transform.localScale = screenCenterScale;



                        if (points.Count > 1)
                            DisplayMeasurement(spawnedObject.transform.position, points[points.Count - 2]);

                    }
                    break;
            }
        }
        if (Physics.Raycast(ray, out hit) && isFloorDrawn && !isHeightMeasured)
        {

            var hitPose = hit.point;
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:

                    for (int i = 0; i < wallLines.Count; i++)
                    {
                        wallLines[i].positionCount = 2;
                        wallLines[i].SetPosition(0, points[i]);
                    }
                    roofLine.positionCount = points.Count + 1;
                    float heightMark = 0;

                    break;

                case TouchPhase.Moved:
                    for (int i = 0; i < wallLines.Count; i++)
                    {
                        wallLines[i].SetPosition(1, new Vector3(points[i].x, hitPose.y, points[i].z));
                    }

                    if (hitPose.y > points[0].y + 0.5f)
                    {
                        heightMark = hitPose.y;
                    }
                    else
                    {
                        heightMark = points[0].y + 0.5f;
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        roofLine.SetPosition(i, new Vector3(points[i].x, heightMark, points[i].z));
                    }
                    roofLine.SetPosition(points.Count, new Vector3(points[0].x, heightMark, points[0].z));
                    heightText.GetComponentInChildren<TMP_Text>().text = "Height : " + (heightMark - points[0].y).ToString("F2") + "m";

                    break;

                case TouchPhase.Ended:
                    for (int i = 0; i < wallLines.Count; i++)
                    {
                        wallLines[i].SetPosition(1, new Vector3(points[i].x, hitPose.y, points[i].z));
                    }

                    if (hitPose.y > points[0].y + 0.5f)
                    {
                        heightMark = hitPose.y;
                    }
                    else
                    {
                        heightMark = points[0].y + 0.5f;
                    }


                    for (int i = 0; i < points.Count; i++)
                    {
                        roofLine.SetPosition(i, new Vector3(points[i].x, heightMark, points[i].z));
                    }
                    roofLine.SetPosition(points.Count, new Vector3(points[0].x, heightMark, points[0].z));

                    heightText.GetComponentInChildren<TMP_Text>().text = "Height : " + (heightMark - points[0].y).ToString("F2") + "m";
                    isHeightMeasured = true;
                    break;
            }
        }
    }

    private void DisplayWallLines(float height)
    {

    }

    public void FinishAddingPoints()
    {
        isFloorDrawn = true;
        heightText.SetActive(true);
        points.Add(new Vector3(0, 0, 2));
        points.Add(new Vector3(1, 0, 1.5f));
        points.Add(new Vector3(2, 0, 2));
        points.Add(new Vector3(1.5f, 0, 1f));
        points.Add(new Vector3(2, 0, 0));
        points.Add(new Vector3(0, 0, 0));

        for (int i = 0; i < points.Count; i++)
        {
            GameObject line = Instantiate(lineRenderer);
            wallLines.Add(line.GetComponent<LineRenderer>());

        }

        FinishFloorButton.SetActive(false);
        DisplayMeasurement(points[0], points[points.Count - 1]);


        List<Vector3> pointsReversed = new List<Vector3>();
        for (int i = points.Count - 1; i >= 0; i--)
        {
            pointsReversed.Add(points[i]);
        }


        //This is where we create support walls
        for (int i = 0; i < points.Count; i++)
        {
            int j = i + 1;
            List<Vector3> wallPoints = new List<Vector3>();
            if (i == points.Count - 1)
            {
                j = 0;
            }
            wallPoints.Add(points[j]);
            wallPoints.Add(points[i]);
            wallPoints.Add(points[i] + new Vector3(0, 5, 0));
            wallPoints.Add(points[j] + new Vector3(0, 5, 0));


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

    Plane CreatePlane(GameObject largePlane)
    {
        var filter = largePlane.GetComponent<MeshFilter>();
        Vector3 normal = new Vector3();

        if (filter && filter.mesh.normals.Length > 0)
            normal = filter.transform.TransformDirection(filter.mesh.normals[0]);

        var plane = new Plane(normal, transform.position);
        return plane;
    }

    private void DisplayMeasurement(Vector3 point1, Vector3 point2)
    {
        float distance = Vector3.Distance(point1, point2);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2, Quaternion.LookRotation(point2 - point1, Vector3.up) * Quaternion.Euler(0, 90, 0));
        g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
    }

    private Vector2 ScreenCenter()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }
}