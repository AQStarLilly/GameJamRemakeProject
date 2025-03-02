using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClonePlate : MonoBehaviour
{
    private static bool hasCloned = false; // Tracks if a clone exists
    private Renderer rend; // Stores the renderer for color changes
    private Color originalColor; // Stores the plate's initial color

    private void Start()
    {
        rend = GetComponent<Renderer>(); // Get the Renderer component
        if (rend != null)
        {
            originalColor = rend.material.color; // Store the initial color
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isOriginalPlayer = other.CompareTag("Player");
        bool isClone = other.CompareTag("PlayerClone");

        //  Allow cloning ONLY if:
        // - No clone exists OR
        // - Only one **active** (not frozen) player remains (even if it's a clone)
        if (!hasCloned && (isOriginalPlayer || isClone && GetActiveNonFrozenPlayerCount() == 1))
        {
            hasCloned = true;  //  Mark cloning as used
            ChangeAllPlatesColor(Color.gray);  //  Turn all plates grey

            // Find all pressure plates in the scene
            ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
            Transform targetPlate = null;

            // Find the plate that is NOT the one the player/clone is currently on
            foreach (ClonePlate plate in allPlates)
            {
                if (plate.transform.position != this.transform.position)  // Ensure it's not the activated plate
                {
                    targetPlate = plate.transform;
                    break;  // Stop at the first valid plate
                }
            }

            // If a valid target plate exists, create the new player or clone
            if (targetPlate != null)
            {
                //  If the only remaining player is a clone, promote it to a real player
                if (isClone && GetActiveNonFrozenPlayerCount() == 1)
                {
                    other.tag = "Player";  //  Convert clone to player
                    PlayerController.UpdateActivePlayers();
                }

                // Create a new clone at the other pressure plate
                GameObject playerClone = Instantiate(other.gameObject, targetPlate.position, targetPlate.rotation);
                playerClone.name = "PlayerClone";  // Rename for debugging

                // Change tag so the clone never activates a pressure plate initially
                playerClone.tag = "PlayerClone";

                // Prevent the clone from having this script (so it never triggers the plate)
                Destroy(playerClone.GetComponent<ClonePlate>());

                //  Update the camera to track both player and clone
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

    // Reactivate the clone plates when only **one active (non-frozen)** player remains
    public static void ResetCloneAvailability()
    {
        if (GetActiveNonFrozenPlayerCount() == 1)
        {
            hasCloned = false;  //  Allow cloning again
            ChangeAllPlatesColorToOriginal();  //  Reset colors of all plates
        }
    }

    //  NEW: Get only **non-frozen** active players
    private static int GetActiveNonFrozenPlayerCount()
    {
        int count = 0;
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (!player.IsFrozen()) //  Ignores frozen players
            {
                count++;
            }
        }
        return count;
    }

    //  Change all plates to a specific color
    private static void ChangeAllPlatesColor(Color newColor)
    {
        ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
        foreach (ClonePlate plate in allPlates)
        {
            if (plate.rend != null)
            {
                plate.rend.material.color = newColor;
            }
        }
    }

    //  Reset all plates back to their original color
    private static void ChangeAllPlatesColorToOriginal()
    {
        ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
        foreach (ClonePlate plate in allPlates)
        {
            if (plate.rend != null)
            {
                plate.rend.material.color = plate.originalColor;
            }
        }
    }
}



