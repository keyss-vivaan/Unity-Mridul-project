using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Creating3DModelState : ARState
{
    public Creating3DModelState(StateManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("Creating 3D Model Now");

        arManager.VisualizeButton.SetActive(true);
        arManager.VisualizeButton.GetComponent<Button>().onClick.AddListener(Visualize);

        Create3DModel();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        arManager.VisualizeButton.SetActive(false);

    }

    public override void Visualize()
    {
        // arManager.SetState(new VisualizeState(arManager));
        foreach (GameObject g in arManager.WallObjects)
        {
            GameObject.Destroy(g.GetComponent<CreatePolygon>());
            GameObject.Destroy(g.GetComponent<MeshCollider>());
            GameObject.Destroy(g.GetComponent<MeshRenderer>());
            GameObject.Destroy(g.GetComponent<MeshFilter>());
            GameObject.DontDestroyOnLoad(g);
        }
        foreach (GameObject g in arManager.WindowsObjects)
        {
            GameObject.Destroy(g.GetComponent<LineRenderer>());
            foreach (Transform child in g.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            GameObject.DontDestroyOnLoad(g);
        }
        foreach (GameObject g in arManager.Measurements)
        {
            foreach (Transform child in g.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            g.GetComponent<PlaceObject>().position = g.transform.position;
            GameObject.DontDestroyOnLoad(g);

        }
        SceneManager.LoadScene("VisualizeScene");

    }

    private void Create3DModel()
    {
        // Debug.Log(arManager.WindowsObjects.Count);
        // Debug.Log(arManager.WallObjects.Count);

        for (int j = 0; j < arManager.WallObjects.Count; j++)
        {
            List<Vector3> wallPoints = new List<Vector3>();
            for (int i = 0; i < arManager.WallObjects[j].GetComponent<wallClass>().points.Length; i++)
            {
                wallPoints.Add(arManager.WallObjects[j].GetComponent<wallClass>().points[i]);
            }
            GameObject wall = GameObject.Instantiate(arManager.createPolygonWithHolesPrefab);
            wall.GetComponent<CreatePolygonWithHoles>().outerPoints = wallPoints;

            for (int i = 0; i < arManager.WallObjects[j].GetComponent<wallClass>().windows.Count; i++)
            {
                windowClass window = arManager.WallObjects[j].GetComponent<wallClass>().windows[i].GetComponent<windowClass>();

                windowClass updatedWindow = SortWindowPoints(arManager.WallObjects[j].GetComponent<wallClass>(), window);
                if (window != null)
                {
                    List<Vector3> windowPoints = new List<Vector3>();
                    windowPoints = updatedWindow.points.ToList();
                    Hole hole = new Hole();
                    hole.points = windowPoints;
                    wall.GetComponent<CreatePolygonWithHoles>().holes.holes.Add(hole);
                }
            }

            wall.GetComponent<CreatePolygonWithHoles>().CreateCustomPolygonWithHoles(true);
        }
    }

    windowClass SortWindowPoints(wallClass wall, windowClass window)
    {
        // Create a new window class to store the sorted points
        windowClass updatedWindow = new windowClass();

        // Create a list to track which points have already been used
        List<int> usedIndices = new List<int>();

        // Iterate over each point in the wall
        for (int j = 0; j < wall.points.Length; j++)
        {
            float minDistance = float.MaxValue;
            int closestPointIndex = -1;

            // Find the closest window point to the current wall point
            for (int i = 0; i < window.points.Length; i++)
            {
                if (usedIndices.Contains(i))
                    continue; // Skip if this window point has already been used

                float distance = Vector3.Distance(wall.points[j], window.points[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPointIndex = i;
                }
            }

            // Add the closest point to the updated window and mark it as used
            if (closestPointIndex != -1)
            {
                updatedWindow.points[j] = window.points[closestPointIndex];
                usedIndices.Add(closestPointIndex); // Mark this point as used
            }
        }


        // Reverse the order of the points in the updated window
        Array.Reverse(updatedWindow.points);

        // Return the updated window with sorted points
        return updatedWindow;
    }
}
