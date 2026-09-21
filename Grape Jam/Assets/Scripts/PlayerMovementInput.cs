using UnityEngine;
using UnityEngine.InputSystem;

    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerMovementInput : MonoBehaviour
    {
    PlayerMovement movement;

        void Awake() 
        {
            movement = GetComponent<PlayerMovement>();
        }

        public void Move(InputAction.CallbackContext context)
        {
            movement.SetMoveInput(context.ReadValue<Vector2>().x);
        }

        public void Float(InputAction.CallbackContext context)
        {
            movement.SetVerticalInput(context.ReadValue<Vector2>().y);
        }
    }
