using TMPro;
using UnityEngine;

public class windowClass : MonoBehaviour
{
    public Vector3[] points = new Vector3[4];
    public windowType type;

    public float width = 0.0f;
    public float height = 0.0f;
    public float margin_from_left = 0.0f;
    public float margin_from_right = 0.0f;
    public float margin_from_bottom = 0.0f;
    public float margin_from_top = 0.0f;

}


public enum windowType
{
    Window,
    Door,
    AC
}

