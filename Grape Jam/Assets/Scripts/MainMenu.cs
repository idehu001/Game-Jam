using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void LevelScene()
    {
        Debug.Log("Levels!");
    }

    public void QuitGame()
    {
        Application.Quit();

         
        Debug.Log("Quit Game");
    }
}