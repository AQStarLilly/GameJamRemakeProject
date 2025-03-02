using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinishGoal : MonoBehaviour
{
    public float waitTime = 1.5f;

    private bool isUsed = false;

    private void OnTriggerEnter(Collider other)
    {       
        //  Allow both player and clone to activate the finish goal
        if ((other.CompareTag("Player") || other.CompareTag("PlayerClone")) && !isUsed)
        {
            isUsed = true;

            // Detach the camera from the finishing player/clone
            Camera mainCamera = Camera.main;
            if(mainCamera != null && mainCamera.transform.IsChildOf(other.transform))
            {
                mainCamera.transform.parent = null;
            }

            //  Disable the finishing player/clone
            other.gameObject.SetActive(false);

            //  Remove them from the active players list (so game logic knows they're gone)
            PlayerController.RemovePlayer(other.gameObject);

            StartCoroutine(LoadNextSceneAfterDelay());
        }                          
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitTime);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }
}
