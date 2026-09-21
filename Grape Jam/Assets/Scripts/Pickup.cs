using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private int scoreToAdd = 2;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            Scoremanager manager = FindFirstObjectByType<Scoremanager>();
            manager.AddScore(scoreToAdd);
            Destroy(gameObject);
        }
    }
}
