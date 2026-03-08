using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    public float movementSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 12f;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 10f;
    public float fallSpeedMultiplier = 2f;

    [Header("Water")]
    public bool inWater = false;
    float verticalMovement;

    [Header("Ice")]
    public bool onIce = false;
    public float iceRatio;
    private Vector2 oldMovement;

    // Update is called once per frame
    void Update()
    {
        if (onIce)
        {
            Vector2 movement = Vector2.MoveTowards(oldMovement, new Vector2(horizontalMovement * movementSpeed, 0)
            + (inWater ? new Vector2(0, verticalMovement * movementSpeed) : new Vector2(0, rigidBody.linearVelocity.y)), iceRatio);
            rigidBody.linearVelocity = movement;
            oldMovement = movement;
        } else
        {
            Vector2 movement = new Vector2(horizontalMovement * movementSpeed, 0)
            + (inWater ? new Vector2(0, verticalMovement * movementSpeed) : new Vector2(0, rigidBody.linearVelocity.y));
            rigidBody.linearVelocity = movement;
            oldMovement = movement;
        }
        Gravity();
        Flip();

        animator.SetFloat("yVelocity", rigidBody.linearVelocity.y);
        animator.SetFloat("xVelocity", Mathf.Abs(rigidBody.linearVelocity.x));
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
                animator.SetTrigger("Jump");
            }
            else if (context.canceled && rigidBody.linearVelocity.y > 0)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpPower * 0.5f);
                animator.SetTrigger("Jump");
            }
        }
    }

    public void Float(InputAction.CallbackContext context)
    {
        if (inWater)
        {
            verticalMovement = context.ReadValue<Vector2>().y;
        }
        else
        {
            verticalMovement = 0;
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

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheck.position, groundCheckSize);
    }
}
