using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinishGoal : MonoBehaviour
{
    public float waitTime = 1.5f;

    private bool isUsed = false;

    private void OnTriggerEnter(Collider other)
    {       
        if (other.CompareTag("Player") && !isUsed)
        {
            isUsed = true;

            Camera mainCamera = Camera.main;
            if(mainCamera != null && mainCamera.transform.IsChildOf(other.transform))
            {
                mainCamera.transform.parent = null;
            }

            other.gameObject.SetActive(false);

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
