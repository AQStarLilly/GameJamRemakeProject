using UnityEngine;
using System.Collections.Generic;

public class DynamicCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;  // The main player
    private List<Transform> clones = new List<Transform>(); //  List to track multiple clones

    public float zoomOutMultiplier = 1.5f;
    public float followSpeed = 5f;
    public float zoomSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0, 3, -5);

    private Camera cam;
    private float defaultZoom;
    private Vector3 initialOffset;
    private Quaternion initialRotation;

    private void Start()
    {
        cam = GetComponent<Camera>();

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
        if (player == null) return;

        //  If the current player is frozen, switch to another active one
        if (player.GetComponent<PlayerController>()?.IsFrozen() == true)
        {
            SwitchToNewTarget(GetRemainingActivePlayer());
        }

        Vector3 targetPosition;
        float distance = 0f;

        if (clones.Count > 0)
        {
            Vector3 averagePosition = player.position;

            //  Calculate midpoint between player & clones
            foreach (Transform clone in clones)
            {
                if (clone != null)
                {
                    averagePosition += clone.position;
                }
            }
            averagePosition /= (clones.Count + 1); //  Average position between all players

            targetPosition = averagePosition + initialOffset;

            //  Adjust zoom based on distance to farthest clone
            foreach (Transform clone in clones)
            {
                if (clone != null)
                {
                    float cloneDistance = Vector3.Distance(player.position, clone.position);
                    if (cloneDistance > distance)
                    {
                        distance = cloneDistance;
                    }
                }
            }

            float targetZoom = Mathf.Clamp(defaultZoom + (distance * zoomOutMultiplier), 7f, 20f);

            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
            }
        }
        else
        {
            targetPosition = player.position + initialOffset;

            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultZoom, Time.deltaTime * zoomSpeed);
            }
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = initialRotation;
    }

    public void SwitchToNewTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            player = newTarget;
            clones.Clear();
        }
    }

    private Transform GetRemainingActivePlayer()
    {
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (!pc.IsFrozen())
            {
                return pc.transform;
            }
        }
        return null;
    }

    //  Called when a clone is created
    public void SetClone(Transform newClone)
    {
        if (!clones.Contains(newClone))
        {
            clones.Add(newClone);
        }
    }

    //  NEW: Called when multiple clones are created
    public void UpdateCloneTracking()
    {
        clones.Clear(); //  Reset the clone list

        //  Find all active clones
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (pc.CompareTag("PlayerClone") && !pc.IsFrozen())
            {
                clones.Add(pc.transform);
            }
        }
    }
}

