using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.UnloadSceneAsync(7);
        }
    }

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
