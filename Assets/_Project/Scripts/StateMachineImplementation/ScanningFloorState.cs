using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScanningFloorState : ARState
{
    private ARRaycastManager m_RaycastManager;
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private GameObject spawnedObject;

    public ScanningFloorState(StateManager manager) : base(manager) { }

    public override void Enter()
    {
        // Debug.Log("Scanning for floor...");
        arManager.scanFloorInstruction.SetActive(true);  // Show scanning instructions

        m_RaycastManager = arManager.GetComponent<ARRaycastManager>();
    }

    public override void Update()
    {
        if (GameObject.FindGameObjectsWithTag("ARPlane").Length > 0)
        {
            arManager.scanFloorInstruction.SetActive(false);
        }

        if (EventSystem.current.IsPointerOverGameObject() || !arManager.TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }




        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;



            GameObject largePlane1 = GameObject.Instantiate(arManager.largePlane, hitPose.position, hitPose.rotation);

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("ARPlane"))
            {
                g.SetActive(false);
            }
            arManager.GetComponent<ARPlaneManager>().enabled = false;
            arManager.SetState(new PlacingObjectsState(arManager));

        }

        // Automatically find the floor
        // if (GameObject.FindGameObjectsWithTag("ARPlane").Length > 0)
        // {
        //     Vector3 floorPos = GameObject.FindGameObjectsWithTag("ARPlane")[0].transform.position;
        //     GameObject.Instantiate(arManager.largePlane, floorPos, Quaternion.identity);


        //     // Disable ARPlaneManager after finding the floor
        //     foreach (GameObject g in GameObject.FindGameObjectsWithTag("ARPlane"))
        //     {
        //         g.SetActive(false);
        //     }
        //     arManager.GetComponent<ARPlaneManager>().enabled = false;

        //     // Transition to placing objects state
        //     arManager.SetState(new PlacingObjectsState(arManager));
        // }
    }

    public override void Exit()
    {
        arManager.scanFloorInstruction.SetActive(false);
        arManager.screenCenter.SetActive(true);
        // Debug.Log("Scanning finished.");
    }
}
