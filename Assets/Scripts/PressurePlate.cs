using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
    //Reference to target object
    public GameObject targetObject;

    // Duration over which the fade occurs, in seconds.
    public float fadeDuration = 1.5f;

    // Color to indicate that the plate has been used.
    public Color usedColor = Color.grey;

    // Flags.
    private bool isFading = false;
    private bool isUsed = false;

    private Renderer plateRenderer;


    private void Start()
    {
        plateRenderer = GetComponent<Renderer>();
    }

    // Called when the player steps onto the pressure plate trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isUsed)
            {
                isUsed = true;
                if (plateRenderer != null)
                {
                    plateRenderer.material.color = usedColor;
                }
            }

            if (!isFading && targetObject != null)
            {
                StartCoroutine(FadeOutCoroutine());
            }
        }
    }

    // Coroutine that gradually fades out the target object's material.
    private IEnumerator FadeOutCoroutine()
    {
        isFading = true;

        // Get the Renderer component from the target object.
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning("Target object does not have a Renderer component.");
            yield break;
        }

        // Access the material. (If using multiple materials, adjust accordingly.)
        Material targetMaterial = targetRenderer.material;
        Color initialColor = targetMaterial.color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Interpolate alpha from 1 (fully opaque) to 0 (fully transparent).
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            targetMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
            yield return null;
        }

        // Ensure the material is fully transparent.
        targetMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        // Optionally disable the target object after the fade-out.
        targetObject.SetActive(false);
    }


}
