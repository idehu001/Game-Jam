using TMPro;
using UnityEngine;

public class Scoremanager : MonoBehaviour
{
    public int Score;

    [SerializeField] private TMP_Text scoreUI;
    public void AddScore(int scoreToAdd)
    {
        Score += scoreToAdd;

        scoreUI.text = $"Score: {Score}";
    }
}
