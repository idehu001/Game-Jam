using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class SpriteFlip : MonoBehaviour
{
    private bool artFacesRight = true;

    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {

        int facing = movement.Facing;
        if (facing == 0) return;

        bool facingLeft = facing < 0;
        spriteRenderer.flipX = artFacesRight ? facingLeft : !facingLeft;
    }
}
