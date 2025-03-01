using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;  // The main player
    private Transform clone;  // The cloned player (if any)

    public float zoomOutMultiplier = 1.5f;  // Controls zoom
    public float followSpeed = 5f;          // Follow smoothness
    public float zoomSpeed = 5f;            // Zoom smoothness
    public Vector3 cameraOffset = new Vector3(0, 3, -5);  // Distance from the player

    private Camera cam;
    private float defaultZoom;
    private Vector3 initialOffset; // Stores initial offset from player
    private Quaternion initialRotation; // Stores the original rotation of the camera

    private void Start()
    {
        cam = GetComponent<Camera>();

        //  Save the initial camera offset & rotation from the scene
        if (player != null)
        {
            initialOffset = transform.position - player.position;
            initialRotation = transform.rotation;
        }

        if (cam.orthographic)
        {
            defaultZoom = cam.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return; // Ensure the player exists

        Vector3 targetPosition;

        if (clone != null)
        {
            //  Get the midpoint between the player & clone
            Vector3 midpoint = (player.position + clone.position) / 2f;

            //  Move the camera to follow the midpoint while keeping its original offset
            targetPosition = midpoint + initialOffset;

            //  Adjust zoom level based on distance
            float distance = Vector3.Distance(player.position, clone.position);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultZoom + distance * zoomOutMultiplier, Time.deltaTime * zoomSpeed);
        }
        else
        {
            //  Follow the player normally while keeping its original offset
            targetPosition = player.position + initialOffset;

            //  Reset zoom when no clone exists
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultZoom, Time.deltaTime * zoomSpeed);
            }
        }

        //  Move the camera smoothly to the target position while maintaining original rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = initialRotation; // Ensures the camera never rotates
    }

    //  Called when a clone is created
    public void SetClone(Transform newClone)
    {
        clone = newClone;
    }
}
