using UnityEngine;

public class Water : MonoBehaviour
{
    public PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enterd");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player");
            player.inWater = true;
            player.fallSpeedMultiplier = 0f;
            player.baseGravity = 0f;
        }  
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.inWater = false;
            player.fallSpeedMultiplier = 2f;
            player.baseGravity = 2f;
        }
    }
}
