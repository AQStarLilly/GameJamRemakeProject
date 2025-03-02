using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    // Reference to target object
    public GameObject targetObject;

    // Duration over which the fade occurs, in seconds.
    public float fadeDuration = 1.5f;

    // Colors for different states.
    public Color activatedColor = Color.grey;
    public Color deactivatedColor = Color.grey; // Final state color after object disappears

    // Flags
    private bool isActivated = false;
    private bool isDeactivated = false; //  Track if the plate should be permanently inactive

    private Renderer plateRenderer;
    private Color originalColor; // Stores the plate's original color

    // Static dictionary to track all plates linked to each object
    private static Dictionary<GameObject, List<PressurePlate>> plateGroups = new Dictionary<GameObject, List<PressurePlate>>();

    private void Start()
    {
        plateRenderer = GetComponent<Renderer>();

        if (plateRenderer != null)
        {
            originalColor = plateRenderer.material.color; // Store the original color
        }

        //  Register this plate in the global dictionary based on the target object
        if (targetObject != null)
        {
            if (!plateGroups.ContainsKey(targetObject))
            {
                plateGroups[targetObject] = new List<PressurePlate>();
            }
            plateGroups[targetObject].Add(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDeactivated) return; //  Ignore activation if the plate has been used already

        if (other.CompareTag("Player") || other.CompareTag("PlayerClone"))
        {
            isActivated = true;
            ChangePlateColor(activatedColor);

            //  Check if all linked plates for this object are activated
            if (AreAllPlatesActivated(targetObject))
            {
                StartCoroutine(FadeOutCoroutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDeactivated) return; //  Ignore if deactivated

        if ((other.CompareTag("Player") || other.CompareTag("PlayerClone")) && isActivated)
        {
            isActivated = false;
            ChangePlateColor(originalColor);

            //  Reset all plates if any are deactivated
            ResetAllPlates(targetObject);
        }
    }

    //  Check if all plates linked to a target object are activated
    private bool AreAllPlatesActivated(GameObject target)
    {
        if (plateGroups.ContainsKey(target))
        {
            foreach (PressurePlate plate in plateGroups[target])
            {
                if (!plate.isActivated) // If any plate is NOT activated, return false
                {
                    return false;
                }
            }
            return true; // All plates are activated at the same time
        }
        return false;
    }

    //  Reset all plates if one is deactivated
    private void ResetAllPlates(GameObject target)
    {
        if (plateGroups.ContainsKey(target))
        {
            foreach (PressurePlate plate in plateGroups[target])
            {
                plate.isActivated = false;
                plate.ChangePlateColor(originalColor);
            }
        }
    }

    //  Coroutine that gradually fades out the target object and disables plates
    private IEnumerator FadeOutCoroutine()
    {
        if (targetObject == null) yield break;

        // Get the Renderer component from the target object.
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning("Target object does not have a Renderer component.");
            yield break;
        }

        Material targetMaterial = targetRenderer.material;
        Color initialColor = targetMaterial.color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            targetMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
            yield return null;
        }

        // Fully transparent
        targetMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        // Disable the target object
        targetObject.SetActive(false);

        //  Deactivate all linked plates permanently
        DeactivateAllPlates(targetObject);
    }

    //  Deactivate all plates linked to the target object
    private void DeactivateAllPlates(GameObject target)
    {
        if (plateGroups.ContainsKey(target))
        {
            foreach (PressurePlate plate in plateGroups[target])
            {
                plate.isDeactivated = true; //  Mark as permanently inactive
                plate.ChangePlateColor(deactivatedColor);
            }
        }
    }

    //  Change this plate's color
    private void ChangePlateColor(Color newColor)
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = newColor;
        }
    }
}