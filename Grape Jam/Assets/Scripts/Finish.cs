using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(EndOfLevel(2f));
        }
    }

    private IEnumerator EndOfLevel(float delay)
    {
        // Debug.Log("You're done! FINISHED! PLEASE STOP NOW");
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(8);
    }
}
