using UnityEngine;

public class ToBaseGravity : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        playerMovement.baseGravity = 2;
    }
}
