using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public float movementSpeed = 5f;

    float horizontalMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newVelocity = new Vector2(horizontalMovement * movementSpeed, rigidBody.linearVelocity.y);
        rigidBody.linearVelocity = newVelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
}
