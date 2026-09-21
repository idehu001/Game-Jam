using UnityEngine;

public class Coals : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public float upwardSpeed = 2f;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        playerMovement.oldGravityScale = playerMovement._rb.gravityScale;
        playerMovement._rb.gravityScale = -upwardSpeed;
        playerMovement._rb.linearVelocity = new Vector2(0,0);
        playerMovement.animator.SetFloat("yVelocity", 0f);
        playerMovement.animator.SetTrigger("Steam");
    }
}
