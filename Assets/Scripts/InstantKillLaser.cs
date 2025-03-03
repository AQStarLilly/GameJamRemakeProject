using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillLaser : MonoBehaviour
{
    public LineRenderer lineRenderer;  //  LineRenderer for the laser visual
    public float maxDistance = 100f;   //  Maximum laser length
    public LayerMask collisionLayers;  //  Determines what the laser can hit

    private void Update()
    {
        FireLaser();
    }

    private void FireLaser()
    {
        if (lineRenderer == null) return;

        Vector3 startPoint = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, maxDistance, collisionLayers))
        {
            //  If the laser hits something, stop at the hit point
            DrawLaser(startPoint, hit.point);

            //  Check if the hit object is a player or clone
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("PlayerClone"))
            {
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.HandleInstantDeath(); //  Call the instant death handler
                }
            }
        }
        else
        {
            //  If the laser hits nothing, extend to max distance
            DrawLaser(startPoint, startPoint + direction * maxDistance);
        }
    }

    private void DrawLaser(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
