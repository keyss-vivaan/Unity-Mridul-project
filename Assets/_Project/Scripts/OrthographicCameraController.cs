using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthographicCameraController : MonoBehaviour
{
    public float zoomSpeed = 5.0f;            // Speed of zooming
    public float panSpeed = 0.5f;             // Speed of panning
    public float minOrthographicSize = 5.0f;  // Minimum camera size
    public float maxOrthographicSize = 20.0f; // Maximum camera size

    private Camera cam;
    private Vector3 dragOrigin;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        HandleZoom(Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += difference * panSpeed;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            // Single finger drag (panning)
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = cam.ScreenToWorldPoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(touch.position);
                cam.transform.position += difference * panSpeed;
            }
        }
        else if (Input.touchCount == 2)
        {
            // Two-finger pinch zoom
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            HandleZoom(deltaMagnitudeDiff * 0.01f * zoomSpeed); // Adjust sensitivity as needed
        }
    }

    void HandleZoom(float increment)
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - increment, minOrthographicSize, maxOrthographicSize);
    }
}
