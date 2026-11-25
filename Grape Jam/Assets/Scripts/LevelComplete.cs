using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene(0);

    }
    public void LevelScene()
    {
        SceneManager.LoadScene(2);
        Debug.Log("Levels!");
    }

    public void QuitGame()
    {
        Application.Quit();


        Debug.Log("Quit Game");
    }
}
