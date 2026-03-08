using UnityEngine;

public class Cracks : MonoBehaviour
{
    public PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
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
