using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void ContinueLevel()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync(7);
    }

    public void Settings()
    {
        Debug.Log("Settings!");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        
    }
}
