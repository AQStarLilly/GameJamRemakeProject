using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClonePlate : MonoBehaviour
{
    private bool isActivated = false; // Prevents multiple activations

    private void OnTriggerEnter(Collider other)
    {
        // Ensure only the original player triggers the plate (not clones)
        if (!isActivated && other.CompareTag("Player"))
        {
            isActivated = true;  // Mark plate as used

            // Find all pressure plates in the scene
            ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
            Transform targetPlate = null;

            // Find the plate that is NOT the one the player is currently on
            foreach (ClonePlate plate in allPlates)
            {
                if (plate.transform.position != this.transform.position)  // Ensure it's not the activated plate
                {
                    targetPlate = plate.transform;
                    break;  // Stop at the first valid plate
                }
            }

            // If a valid target plate exists, create the clone there
            if (targetPlate != null)
            {
                // Create a clone at the new pressure plate
                GameObject playerClone = Instantiate(other.gameObject, targetPlate.position, targetPlate.rotation);
                playerClone.name = "PlayerClone";  // Rename for debugging

                // Change tag so the clone never activates a pressure plate
                playerClone.tag = "PlayerClone";

                // Prevent the clone from having this script (so it never triggers the plate)
                Destroy(playerClone.GetComponent<ClonePlate>());


                DynamicCamera cameraScript = Camera.main.GetComponent<DynamicCamera>();
                if (cameraScript != null)
                {
                    cameraScript.SetClone(playerClone.transform);
                }
            }
            else
            {
                Debug.LogWarning("No other pressure plate available for cloning!");
            }
        }
    }
}
