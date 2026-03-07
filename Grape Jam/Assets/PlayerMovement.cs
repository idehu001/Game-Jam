using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rigidBody;

    [Header("Movement")]
    public float movementSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    // Update is called once per frame
    void Update()
    {
        Vector2 newVelocity = new Vector2(horizontalMovement * movementSpeed, rigidBody.linearVelocity.y);
        rigidBody.linearVelocity = newVelocity;
        Gravity();
    }

    private void Gravity()
    {
        if (rigidBody.linearVelocity.y < 0)
        {
            rigidBody.gravityScale = baseGravity * fallSpeedMultiplier;
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, Mathf.Max(rigidBody.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rigidBody.gravityScale = baseGravity;
        }

    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded())
        {
            if (context.performed)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpPower);
            }
            else if (context.canceled)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpPower * 0.5f);
            }
        }
    }

    private bool isGrounded()
    {
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer))
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheck.position, groundCheckSize);
    }
}
