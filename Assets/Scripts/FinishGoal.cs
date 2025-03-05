using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinishGoal : MonoBehaviour
{
    public float waitTime = 1.5f;
    private bool isUsed = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("PlayerClone")) && !isUsed)
        {
            isUsed = true;

            //  Prevent the scene from resetting when the player disappears
            Debug.Log("Finish triggered! Preventing scene reset.");
            PlayerController.SetSceneChanging(true);

            // Detach the camera from the finishing player/clone
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.IsChildOf(other.transform))
            {
                mainCamera.transform.parent = null;
            }

            // Disable the finishing player/clone
            other.gameObject.SetActive(false);

            //  Remove from active players list **without triggering a scene reset**
            PlayerController.RemovePlayer(other.gameObject);

            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitTime);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Loading next scene: " + nextSceneIndex);
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Restarting at level 1.");
            SceneManager.LoadScene(0);
        }
    }
}
