using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;
    public Transform CameraTransform;
    public float MovementSpeed = 10f, sprintMultiplier = 1.6f, RotationSpeed = 0.1f, JumpForce = 10f, Gravity = -30f;
    public float MinPitch = -85f, MaxPitch = 85f;

    // The time in seconds it takes to reach target movement speed. Smaller = snappier
    public float AccelTime = 0.1f;

    private float _rotationY;
    private float _rotationX;
    private float _vertVelocity;

    private Vector3 _horizontalVelocity;
    private Vector3 _smoothDampRef;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 movementVector, bool isSprinting)
    {
        if (_characterController.isGrounded && _vertVelocity < 0f)
        {
            _vertVelocity = -2f; // small push keeps the controller grounded
        }

        _vertVelocity += Gravity * Time.deltaTime;

        float speed = isSprinting ? MovementSpeed * sprintMultiplier : MovementSpeed;

        Vector2 input = Vector2.ClampMagnitude(movementVector, 1f);
        Vector3 targetVelocity = (transform.forward * input.y + transform.right * input.x) * speed;

        _horizontalVelocity = Vector3.SmoothDamp(_horizontalVelocity, targetVelocity, ref _smoothDampRef, AccelTime);

        Vector3 move = _horizontalVelocity;
        move.y = _vertVelocity;

        _characterController.Move(move * Time.deltaTime);
    }

    public void Rotate(Vector2 rotationVector)
    {
        _rotationY += rotationVector.x * RotationSpeed;
        transform.localRotation = Quaternion.Euler(0, _rotationY, 0);

        _rotationX -= rotationVector.y * RotationSpeed;
        _rotationX = Mathf.Clamp(_rotationX, MinPitch, MaxPitch);
        CameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
    }

    public void Jump()
    {
        if (_characterController.isGrounded)
        {
            _vertVelocity = JumpForce;
        }
    }
}
