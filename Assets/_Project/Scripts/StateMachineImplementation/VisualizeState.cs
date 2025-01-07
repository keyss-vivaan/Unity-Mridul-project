using Unity.XR.CoreUtils;
using UnityEngine;

public class VisualizeState : ARState
{
    public VisualizeState(StateManager manager) : base(manager) { }

    public override void Enter()
    {

        arManager.visualizationCamera.enabled = true;
        arManager.GetComponent<XROrigin>().Camera.enabled = false;
        Debug.Log("Visualizing");

    }
}
