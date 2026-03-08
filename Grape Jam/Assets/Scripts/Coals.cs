using UnityEngine;

public class Coals : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        playerMovement.baseGravity = -2;
        playerMovement.animator.SetTrigger("Steam");
    }
}
