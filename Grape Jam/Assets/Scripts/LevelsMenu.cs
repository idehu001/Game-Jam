using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsMenu : MonoBehaviour
{
    public void StartLevel1()
    {
        SceneManager.LoadScene(1);
        Debug.Log("Level start!");
    }

    public void Test1()
    {
        SceneManager.LoadScene(3);
        Debug.Log("Test 1");
    }

    public void Test2()
    {
        SceneManager.LoadScene(4);
        Debug.Log("Test 2");
    }
    public void Test3()
    {
        SceneManager.LoadScene(5);
        Debug.Log("Test 3");
    }
    public void Test4()
    {
        SceneManager.LoadScene(6);
        Debug.Log("Test 4");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        
    }
}
