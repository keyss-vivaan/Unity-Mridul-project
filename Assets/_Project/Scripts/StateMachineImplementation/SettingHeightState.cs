using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;


public class SettingHeightState : ARState
{
    public SettingHeightState(StateManager manager) : base(manager) { }

    private float heightMark = 0f;

    public override void Enter()
    {
        // Debug.Log("Setting height");
        arManager.SetHeightButton.SetActive(true);
        arManager.SetHeightButton.GetComponent<Button>().onClick.AddListener(OnSetHeight);
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            arManager.heightTextInMeter.SetActive(true);
        }
        else
        {
            arManager.heightTextInFeet.SetActive(true);
        }
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

        if (Physics.Raycast(ray, out hit) && Input.GetMouseButton(0)) // Check for left mouse button click
        {
            var hitPose = hit.point;

            // Handle different mouse button states similar to touch phases
            if (Input.GetMouseButtonDown(0)) // Equivalent to TouchPhase.Began
            {
                // Add logic for when the mouse button is first pressed, if necessary
            }
            else if (Input.GetMouseButton(0)) // Equivalent to TouchPhase.Moved
            {
                // If the hit point's Y-coordinate is greater than the base measurement plus 0.5, update the height
                if (hitPose.y > arManager.Measurements[0].transform.position.y + 0.5f)
                {
                    heightMark = hitPose.y;
                }
                else
                {
                    heightMark = arManager.Measurements[0].transform.position.y + 0.5f;
                }

                // Update height text display
                arManager.heightTextInMeter.GetComponentInChildren<TMP_Text>().text = "Height : " + (heightMark - arManager.Measurements[0].transform.position.y).ToString("F2") + " m";
                arManager.heightTextInFeet.GetComponentInChildren<TMP_Text>().text = "Height : " + ((heightMark - arManager.Measurements[0].transform.position.y) * 3.28084f).ToString("F2") + " ft";

                // Update positions of roof line vertices to the new height
                for (int i = 0; i < arManager.roofLine.positionCount; i++)
                {
                    arManager.roofLine.SetPosition(i, new Vector3(arManager.roofLine.GetPosition(i).x, heightMark, arManager.roofLine.GetPosition(i).z));
                }

                // Adjust height for each measurement point's associated FollowPin component
                for (int i = 0; i < arManager.Measurements.Count; i++)
                {
                    arManager.Measurements[i].GetComponentInChildren<FollowPin>().height = heightMark - arManager.Measurements[0].transform.position.y;
                }
            }
            else if (Input.GetMouseButtonUp(0)) // Equivalent to TouchPhase.Ended
            {
                // Add logic for when the mouse button is released, if necessary
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
                    break;
                case TouchPhase.Moved:
                    if (hitPose.y > arManager.Measurements[0].transform.position.y + 0.5f)
                    {
                        heightMark = hitPose.y;
                    }
                    else
                    {
                        heightMark = arManager.Measurements[0].transform.position.y + 0.5f;
                    }
                    
                    arManager.heightTextInMeter.GetComponentInChildren<TMP_Text>().text = "Height : " + (heightMark - arManager.Measurements[0].transform.position.y).ToString("F2") + " m";
                    arManager.heightTextInFeet.GetComponentInChildren<TMP_Text>().text = "Height : " + ((heightMark - arManager.Measurements[0].transform.position.y) * 3.28084f).ToString("F2") + " ft";

                    for (int i = 0; i < arManager.roofLine.positionCount; i++)
                    {
                        arManager.roofLine.SetPosition(i, new Vector3(arManager.roofLine.GetPosition(i).x, heightMark, arManager.roofLine.GetPosition(i).z));
                    }

                    for (int i = 0; i < arManager.Measurements.Count; i++)
                    {
                        arManager.Measurements[i].GetComponentInChildren<FollowPin>().height = heightMark - arManager.Measurements[0].transform.position.y;
                    }
                    break;
                case TouchPhase.Ended:
                    break;
            }

        }
#endif
    }

    public override void Exit()
    {
        arManager.SetHeightButton.SetActive(false);
    }

    public override void OnSetHeight()
    {
        FinishSettingHeight(heightMark - arManager.Measurements[0].transform.position.y);
        arManager.SetState(new CreatingWindowsState(arManager));
    }


    public void FinishSettingHeight(float height)
    {
        Debug.Log("Finish setting the height up.");
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Wall"))
        {
            GameObject.Destroy(g);
        }

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
            wallPoints.Add(arManager.Measurements[i].transform.position + new Vector3(0, height, 0));
            wallPoints.Add(arManager.Measurements[j].transform.position + new Vector3(0, height, 0));


            GameObject wall = new GameObject("wall" + i);
            wall.tag = "Wall";
            wall.AddComponent<MeshFilter>();
            wall.AddComponent<MeshRenderer>();
            wall.AddComponent<MeshCollider>();
            wall.AddComponent<wallClass>();
            wall.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            wall.GetComponent<MeshRenderer>().enabled = false;
            CreatePolygon generateMesh = wall.AddComponent<CreatePolygon>();
            wall.GetComponent<wallClass>().points[0] = arManager.Measurements[i].transform.position;
            wall.GetComponent<wallClass>().points[1] = arManager.Measurements[j].transform.position;
            wall.GetComponent<wallClass>().points[2] = arManager.Measurements[j].transform.position + new Vector3(0, height, 0);
            wall.GetComponent<wallClass>().points[3] = arManager.Measurements[i].transform.position + new Vector3(0, height, 0);
            wall.GetComponent<wallClass>().wallIndex = i;
            arManager.WallObjects.Add(wall);
            generateMesh.points = wallPoints;

        }


    }
}