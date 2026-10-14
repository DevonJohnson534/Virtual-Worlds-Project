using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public PlayerController CharacterController;
    private InputAction _moveAction, _LookAction, _jumpAction, _sprintAction;
    void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _LookAction = InputSystem.actions.FindAction("Look");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _sprintAction = InputSystem.actions.FindAction("Sprint");

        _jumpAction.performed += OnJumpPerformed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementVector = _moveAction.ReadValue<Vector2>();
        bool isSprinting = _sprintAction.IsPressed();
        CharacterController.Move(movementVector, isSprinting);

        Vector2 lookVector = _LookAction.ReadValue<Vector2>();
        CharacterController.Rotate(lookVector);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        CharacterController.Jump();
    }
}
