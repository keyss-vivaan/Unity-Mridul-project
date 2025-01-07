using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlacingObjectsState : ARState
{
    private GameObject spawnedObject;
    private LineRenderer tempFloorLine;
    private LineRenderer tempRoofLine;
    private GameObject tempText;

    public PlacingObjectsState(StateManager manager) : base(manager) { }

    public override void Enter()
    {
        arManager.UndoButton.SetActive(true);
        arManager.UndoButton.GetComponent<Button>().onClick.AddListener(OnUndo);
        arManager.FinishFloorButton.SetActive(true);
        arManager.FinishFloorButton.GetComponent<Button>().onClick.AddListener(OnFinishFloor);
        // Debug.Log("Started placing objects");

        tempFloorLine = GameObject.Instantiate(arManager.lineRenderer);
        tempRoofLine = GameObject.Instantiate(arManager.lineRenderer);

        tempText = GameObject.Instantiate(arManager.measurementTextPrefab);
        tempText.tag = "Untagged";

    }

    public override void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !arManager.TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(arManager.ScreenCenter());
        RaycastHit hit;
#if UNITY_EDITOR
        if (Physics.Raycast(ray, out hit) && Input.GetMouseButton(0)) // Check for left mouse button click (0 is left click)
        {
            var hitPose = hit.point;

            // Handle different mouse button states similar to touch phases
            if (Input.GetMouseButtonDown(0)) // Equivalent to TouchPhase.Began
            {
                spawnedObject = GameObject.Instantiate(arManager.placedPrefab, hitPose, Quaternion.identity);

                spawnedObject.transform.position = hitPose;

                if (arManager.Measurements.Count > 2 && Vector3.Distance(arManager.Measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                {
                    spawnedObject.transform.position = arManager.Measurements[0].transform.position;
                }

                if (arManager.Measurements.Count > 0)
                {
                    tempFloorLine.positionCount = 2;
                    tempRoofLine.positionCount = 2;

                    tempFloorLine.SetPosition(0, arManager.Measurements[arManager.Measurements.Count - 1].transform.position);
                    tempFloorLine.SetPosition(1, spawnedObject.transform.position);

                    tempRoofLine.SetPosition(0, arManager.Measurements[arManager.Measurements.Count - 1].transform.position + new Vector3(0, 0.5f, 0));
                    tempRoofLine.SetPosition(1, spawnedObject.transform.position + new Vector3(0, 0.5f, 0));

                    float distance = Vector3.Distance(arManager.Measurements[arManager.Measurements.Count - 1].transform.position, spawnedObject.transform.position);
                    tempText.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
                    tempText.transform.position = (arManager.Measurements[arManager.Measurements.Count - 1].transform.position + spawnedObject.transform.position) / 2;
                    tempText.transform.rotation = Quaternion.LookRotation(arManager.Measurements[arManager.Measurements.Count - 1].transform.position - spawnedObject.transform.position, Vector3.up) * Quaternion.Euler(0, 90, 0);
                }

                // spawnedObject.transform.position = hitPose;

                if (arManager.Measurements.Count > 2 && Vector3.Distance(arManager.Measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                {
                    spawnedObject.transform.position = arManager.Measurements[0].transform.position;
                    FinishAddingPoints();
                    arManager.SetState(new SettingHeightState(arManager));
                    return;
                }
                arManager.Measurements.Add(spawnedObject);
                RemoveTempObjects();
                UpdatePositions();
            }
        }

#else
        if (Physics.Raycast(ray, out hit) && Input.touchCount > 0)
        {
            var hitPose = hit.point;
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:

                    spawnedObject = GameObject.Instantiate(arManager.placedPrefab, hitPose, Quaternion.identity);
                    break;
                case TouchPhase.Moved:

                    spawnedObject.transform.position = hitPose;

                    if (arManager.Measurements.Count > 2 && Vector3.Distance(arManager.Measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                    {
                        spawnedObject.transform.position = arManager.Measurements[0].transform.position;
                    }

                    if (arManager.Measurements.Count > 0)
                    {
                        tempFloorLine.positionCount = 2;
                        tempRoofLine.positionCount = 2;

                        tempFloorLine.SetPosition(0, arManager.Measurements[arManager.Measurements.Count - 1].transform.position);
                        tempFloorLine.SetPosition(1, spawnedObject.transform.position);

                        tempRoofLine.SetPosition(0, arManager.Measurements[arManager.Measurements.Count - 1].transform.position + new Vector3(0, 0.5f, 0));
                        tempRoofLine.SetPosition(1, spawnedObject.transform.position + new Vector3(0, 0.5f, 0));

                        float distance = Vector3.Distance(arManager.Measurements[arManager.Measurements.Count - 1].transform.position, spawnedObject.transform.position);
                        tempText.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + "m";
                        tempText.transform.position = (arManager.Measurements[arManager.Measurements.Count - 1].transform.position + spawnedObject.transform.position) / 2;
                        tempText.transform.rotation = Quaternion.LookRotation(arManager.Measurements[arManager.Measurements.Count - 1].transform.position - spawnedObject.transform.position, Vector3.up) * Quaternion.Euler(0, 90, 0);
                    }

                    break;

                case TouchPhase.Ended:
                    spawnedObject.transform.position = hitPose;
                    if (arManager.Measurements.Count > 2 && Vector3.Distance(arManager.Measurements[0].transform.position, spawnedObject.transform.position) < 0.1f)
                    {
                        spawnedObject.transform.position = arManager.Measurements[0].transform.position;
                        FinishAddingPoints();
                        arManager.SetState(new SettingHeightState(arManager));
                        return;
                    }
                    arManager.Measurements.Add(spawnedObject);
                    RemoveTempObjects();
                    UpdatePositions();

                    break;
            }
        }

#endif
    }

    public override void Exit()
    {
        arManager.UndoButton.SetActive(false);
        arManager.FinishFloorButton.SetActive(false);
        RemoveTempObjects();
        // Debug.Log("Stopped placing objects");
    }

    public override void OnUndo()
    {
        // Debug.Log("Undoing last point...");
        if (arManager.Measurements.Count > 0)
        {
            GameObject lastPoint = arManager.Measurements[arManager.Measurements.Count - 1];
            GameObject.Destroy(lastPoint);
            arManager.Measurements.RemoveAt(arManager.Measurements.Count - 1);
            UpdatePositions();  // Refresh positions after undo
        }
    }

    public override void OnFinishFloor()
    {
        // Debug.Log("Finished placing objects");
        FinishAddingPoints();
        arManager.SetState(new SettingHeightState(arManager));
    }




    private void RemoveTempObjects()
    {
        tempFloorLine.transform.position = new Vector3(1000, 1000, 1000);
        tempFloorLine.GetComponent<LineRenderer>().positionCount = 0;
        tempRoofLine.transform.position = new Vector3(1000, 1000, 1000);
        tempRoofLine.GetComponent<LineRenderer>().positionCount = 0;
        tempText.transform.position = new Vector3(1000, 1000, 1000);

    }

    public void UpdatePositions()
    {
        arManager.floorLine.positionCount = arManager.Measurements.Count;
        arManager.roofLine.positionCount = arManager.Measurements.Count;
        // Debug.Log("Updating positions");
        for (int i = 0; i < arManager.Measurements.Count; i++)
        {
            arManager.floorLine.SetPosition(i, arManager.Measurements[i].transform.position);
            arManager.roofLine.SetPosition(i, arManager.Measurements[i].transform.position + new Vector3(0, 0.5f, 0));
        }

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
        {
            GameObject.Destroy(g);
        }
        for (int i = 0; i < arManager.Measurements.Count - 1; i++)
        {
            arManager.DisplayMeasurement(arManager.Measurements[i].transform.position, arManager.Measurements[i + 1].transform.position);
        }
    }

    public void FinishAddingPoints()
    {

        arManager.floorLine.positionCount = arManager.Measurements.Count + 1;
        arManager.roofLine.positionCount = arManager.Measurements.Count + 1;
        for (int i = 0; i < arManager.Measurements.Count; i++)
        {
            arManager.floorLine.SetPosition(i, arManager.Measurements[i].transform.position);
            arManager.roofLine.SetPosition(i, arManager.Measurements[i].transform.position + new Vector3(0, 0.5f, 0));
        }
        arManager.floorLine.SetPosition(arManager.Measurements.Count, arManager.Measurements[0].transform.position);
        arManager.roofLine.SetPosition(arManager.Measurements.Count, arManager.Measurements[0].transform.position + new Vector3(0, 0.5f, 0));
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("measurementDisplay"))
        {
            GameObject.Destroy(g);
        }
        for (int i = 0; i < arManager.Measurements.Count - 1; i++)
        {
            arManager.DisplayMeasurement(arManager.Measurements[i].transform.position, arManager.Measurements[i + 1].transform.position);
        }
        arManager.DisplayMeasurement(arManager.Measurements[arManager.Measurements.Count - 1].transform.position, arManager.Measurements[0].transform.position);

        CreateWallBase();

    }

    private void CreateWallBase()
    {
        for (int i = 0; i < arManager.Measurements.Count; i++)
        {
            int j = i + 1;
            List<Vector3> wallPoints = new List<Vector3>();
            if (i == arManager.Measurements.Count - 1)
            {
                j = 0;
            }
            wallPoints.Add(arManager.Measurements[j].transform.position);
            wallPoints.Add(arManager.Measurements[i].transform.position);
            wallPoints.Add(arManager.Measurements[i].transform.position + new Vector3(0, 10, 0));
            wallPoints.Add(arManager.Measurements[j].transform.position + new Vector3(0, 10, 0));


            GameObject wall = new GameObject("wall" + i);
            wall.tag = "Wall";
            wall.AddComponent<MeshFilter>();
            wall.AddComponent<MeshRenderer>();
            wall.AddComponent<MeshCollider>();
            wall.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            wall.GetComponent<MeshRenderer>().enabled = false;
            CreatePolygon generateMesh = wall.AddComponent<CreatePolygon>();

            generateMesh.points = wallPoints;

        }
    }

}
