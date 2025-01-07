using UnityEngine;

public class PerspectiveCameraControl : MonoBehaviour
{
    public Transform target;                    // The point the camera will look at and orbit around
    public float rotationSpeed = 100.0f;        // Speed of rotation
    public float panSpeed = 0.5f;               // Speed of panning
    public float zoomSpeed = 5.0f;              // Speed of zooming
    public float minZoomDistance = 2.0f;        // Minimum zoom distance
    public float maxZoomDistance = 50.0f;       // Maximum zoom distance
    public float minPitchAngle = 5.0f;          // Minimum pitch angle to prevent looking upwards
    public float maxPitchAngle = 85.0f;         // Maximum pitch angle to prevent overly steep views

    private Vector3 targetOffset;               // Offset from the target for panning
    private float currentZoomDistance;          // Current distance from the camera to the target
    private float yaw = 0.0f;                   // Horizontal angle around the target
    private float pitch = 45.0f;                // Vertical angle (clamped)
    private bool isPanning = false;             // To track if we are in panning mode

    void Start()
    {
        currentZoomDistance = Vector3.Distance(transform.position, target.position);
        targetOffset = target.position;

        // Initialize yaw and pitch based on the initial camera position relative to the target
        Vector3 direction = transform.position - target.position;
        yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        pitch = Vector3.Angle(Vector3.ProjectOnPlane(direction, Vector3.up), direction);
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
        UpdateCameraPosition();
    }

    void HandleMouseInput()
    {
        // Zoom with the mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        HandleZoom(scroll * zoomSpeed);

        // Rotate with left mouse button
        if (Input.GetMouseButton(0) && !isPanning)
        {
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            RotateAroundTarget(rotationX, rotationY);
        }

        // Pan with right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
        }
        if (Input.GetMouseButton(1))
        {
            float panX = -Input.GetAxis("Mouse X") * panSpeed;
            float panY = -Input.GetAxis("Mouse Y") * panSpeed;
            PanTarget(panX, panY);
        }
        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1 && !isPanning)
        {
            // Single touch for rotation
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float rotationX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime * 0.1f;
                float rotationY = touch.deltaPosition.y * rotationSpeed * Time.deltaTime * 0.1f;
                RotateAroundTarget(rotationX, rotationY);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Check if pinch-to-zoom or pan
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            HandleZoom(deltaMagnitudeDiff * 0.01f * zoomSpeed);

            // Pan with two fingers moving together
            if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
            {
                isPanning = true;
                Vector2 touchDelta = (touchZero.deltaPosition + touchOne.deltaPosition) / 2;
                float panX = -touchDelta.x * panSpeed * Time.deltaTime * 0.1f;
                float panY = -touchDelta.y * panSpeed * Time.deltaTime * 0.1f;
                PanTarget(panX, panY);
            }
            else
            {
                isPanning = false;
            }
        }
    }

    void RotateAroundTarget(float rotationX, float rotationY)
    {
        // Update yaw for horizontal rotation
        yaw += rotationX;

        // Update and clamp pitch for vertical rotation to stay above the horizon
        pitch = Mathf.Clamp(pitch - rotationY, minPitchAngle, maxPitchAngle);

        // Calculate the new position in spherical coordinates
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.forward * currentZoomDistance;

        transform.position = target.position - direction;
        transform.LookAt(target.position);
    }

    void PanTarget(float panX, float panY)
    {
        // Move both the target position and the camera to pan without rotating
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        Vector3 panMovement = right * panX + up * panY;
        targetOffset += panMovement;
        target.position += panMovement;
        transform.position += panMovement;
    }

    void HandleZoom(float increment)
    {
        // Adjust the camera's distance from the target for zooming
        currentZoomDistance = Mathf.Clamp(currentZoomDistance - increment, minZoomDistance, maxZoomDistance);
    }

    void UpdateCameraPosition()
    {
        // Update the camera's position based on the target offset and zoom distance
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.forward * currentZoomDistance;

        transform.position = targetOffset - direction;
        transform.LookAt(targetOffset);
    }
}
