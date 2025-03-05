using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClonePlate : MonoBehaviour
{
    private static Dictionary<GameObject, ClonePlate> cloneOrigins = new Dictionary<GameObject, ClonePlate>();
    private static int maxClones = 1; //  Tracks how many clones should exist at max
    private static int activeClones = 0; //  Current active clones
    private static int missingClones = 0; //  Tracks how many players/clones need replacing
    private static bool cloningAllowed = true; //  Allow cloning initially if clones exist in the level

    private Renderer rend;
    private Color originalColor;
    private bool isActivated = false; // Tracks if this plate has been used

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }

        //  Set max clones if not already set
        if (maxClones <= 1)
        {
            maxClones = FindObjectsOfType<ClonePlate>().Length; //  Match max clones to number of clone plates
        }

        //  Allow cloning if there are clone plates in the scene
        cloningAllowed = maxClones > 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isOriginalPlayer = other.CompareTag("Player");
        bool isClone = other.CompareTag("PlayerClone");

        if (cloningAllowed && !isActivated && (isOriginalPlayer || isClone))
        {
            Debug.Log($"Clone Plate {gameObject.name} Activated by {other.name}");

            isActivated = true;
            ChangePlateColor(Color.gray);

            //  Select a valid, non-frozen player for cloning
            GameObject validCloneSource = FindActivePlayerForCloning();

            if (validCloneSource != null)
            {
                bool isRecloning = (missingClones > 0);

                if (isRecloning)
                {
                    Debug.Log($"Recloning: {gameObject.name} is now spawning {missingClones} new clones.");
                    RequestCloneFromAvailablePlates(validCloneSource, this, true);
                }
                else
                {
                    Debug.Log($"First-Time Cloning: All available plates will spawn clones.");
                    RequestCloneFromAvailablePlates(validCloneSource, this, false);
                }

                SetAllPlatesToGrey();
            }
            else
            {
                Debug.LogError("No active players/clones available for cloning!");
            }
        }
    }

    //  Called when a clone dies (fall or laser)
    public static void HandleCloneDeath(GameObject deadPlayer)
    {
        activeClones--; //  Reduce active clone count

        //  Remove the dead player/clone from tracking
        if (cloneOrigins.ContainsKey(deadPlayer))
        {
            cloneOrigins.Remove(deadPlayer);
        }

        //  Ensure the dead object is properly destroyed
        if (deadPlayer != null)
        {
            Destroy(deadPlayer);
        }

        //  Increase missing clone count ONLY if we're under the max clone limit
        if (missingClones < maxClones)
        {
            missingClones++;
        }

        //  Reset clone plates so they can be used again
        ResetCloneAvailability();

        //  Allow cloning again, but WAIT for the player to activate a pad
        cloningAllowed = true;

        Debug.Log($"Clone pads are now reactivated. Waiting for player to step on a plate. Missing clones: {missingClones}");
    }


    //  Reactivate clone plates for recloning
    public static void ResetCloneAvailability()
    {
        ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
        foreach (ClonePlate plate in allPlates)
        {
            if (plate.isActivated) //  Only reset previously activated plates
            {
                plate.isActivated = false;
                plate.ChangePlateColor(plate.originalColor);
            }
        }
    }

    private static GameObject FindActivePlayerForCloning()
    {
        //  Check for any active, non-frozen Player first
        GameObject activePlayer = GameObject.FindGameObjectWithTag("Player");
        if (activePlayer != null && !activePlayer.GetComponent<PlayerController>().IsFrozen())
        {
            return activePlayer;
        }

        //  If no valid Player, check for active, non-frozen Clones
        GameObject[] clones = GameObject.FindGameObjectsWithTag("PlayerClone");
        foreach (GameObject clone in clones)
        {
            if (!clone.GetComponent<PlayerController>().IsFrozen())
            {
                return clone; //  Use the first available non-frozen clone
            }
        }

        return null; //  No active, non-frozen players/clones found
    }


    private static void RequestCloneFromAvailablePlates(GameObject originalPlayer, ClonePlate activatedPlate, bool isRecloning)
    {
        if (originalPlayer == null)
        {
            Debug.LogError("RequestCloneFromAvailablePlates: originalPlayer is null! Cannot proceed with cloning.");
            return; //  Prevents trying to instantiate a null object
        }

        ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();
        List<ClonePlate> availablePlates = new List<ClonePlate>();

        //  Find all plates that aren’t activated and are NOT the plate the player activated
        foreach (ClonePlate plate in allPlates)
        {
            if (!plate.isActivated && plate != activatedPlate)
            {
                availablePlates.Add(plate);
            }
        }

        if (availablePlates.Count == 0)
        {
            Debug.Log("No available plates for cloning.");
            return;
        }

        Debug.Log($"Available clone plates: {availablePlates.Count}, Clones needed: {missingClones}");

        if (isRecloning)
        {
            //  Recloning case: Only spawn the EXACT number of missing clones
            int clonesToSpawn = Mathf.Min(missingClones, availablePlates.Count);

            for (int i = 0; i < clonesToSpawn; i++)
            {
                ClonePlate chosenPlate = availablePlates[i];
                Debug.Log($"Recloning at: {chosenPlate.name}, avoiding activated plate: {activatedPlate?.name}");
                GameObject newClone = chosenPlate.ActivateClone(originalPlayer);
            }

            //  Reset missing clone count after replacing them
            missingClones -= clonesToSpawn;
        }
        else
        {
            //  First-time cloning: Spawn clones at ALL available plates
            Debug.Log($"First-time cloning: Spawning clones at ALL available plates.");

            foreach (ClonePlate plate in availablePlates)
            {
                Debug.Log($"Spawning a clone at: {plate.name}");
                plate.ActivateClone(originalPlayer);
            }
        }

        SetAllPlatesToGrey();
    }



    private GameObject ActivateClone(GameObject originalPlayer)
    {
        if (originalPlayer == null)
        {
            Debug.LogError("ActivateClone: originalPlayer is null! Cannot create clone.");
            return null; //  Prevents trying to instantiate a null object
        }

        this.isActivated = true;
        this.ChangePlateColor(Color.gray);

        Debug.Log($"Activating Clone on: {gameObject.name}");

        //  Create a clone at this plate’s position
        GameObject newClone = Instantiate(originalPlayer, transform.position, transform.rotation);
        newClone.name = "PlayerClone";
        newClone.tag = "PlayerClone";

        //  Ensure the clone isn't frozen when created
        PlayerController cloneController = newClone.GetComponent<PlayerController>();
        if (cloneController != null)
        {
            cloneController.ResetFrozenState(); //  Ensure the clone is fully functional
        }

        //  Track where this clone came from
        if (!cloneOrigins.ContainsKey(newClone))
        {
            cloneOrigins[newClone] = this;
        }

        //  Remove ClonePlate script from clones to prevent infinite cloning
        Destroy(newClone.GetComponent<ClonePlate>());

        activeClones++;

        int maxPlayersAllowed = maxClones + 1;
        if (GetActiveNonFrozenPlayerCount() == maxPlayersAllowed)
        {
            cloningAllowed = false;
        }

        //  Update Camera Tracking
        DynamicCamera cameraScript = Camera.main.GetComponent<DynamicCamera>();
        if (cameraScript != null)
        {
            cameraScript.UpdateCloneTracking();
        }

        return newClone; //  Return the new clone object
    }


    //  Get only **non-frozen** active players
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

    private static void SetAllPlatesToGrey()
    {
        ClonePlate[] allPlates = FindObjectsOfType<ClonePlate>();

        foreach (ClonePlate plate in allPlates)
        {
            plate.isActivated = true; //  Ensure the plate is marked as used
            plate.ChangePlateColor(Color.gray); //  Change color to grey
        }

        Debug.Log("All clone plates have been set to grey.");
    }

    //  Change the color of this plate
    private void ChangePlateColor(Color newColor)
    {
        if (rend != null)
        {
            rend.material.color = newColor;
        }
    }
}







