using UnityEngine;

public class Ice : MonoBehaviour
{
    public PlayerMovement player;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            player.onIce = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            player.onIce = false;
        }
    }
}
