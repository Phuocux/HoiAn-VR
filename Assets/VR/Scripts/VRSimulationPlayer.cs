using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class VRSimulationPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform head;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float verticalSpeed = 3f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;

    private VRSimulationControls controls;

    private void Awake()
    {
        controls = new VRSimulationControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        Move();
        Look();
    }

    private void Move()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        float verticalInput = controls.Player.VerticalMove.ReadValue<float>();

        // Di chuyển theo hướng của đầu nhưng giữ chuyển động trên mặt phẳng.
        Vector3 forward = head.forward;
        Vector3 right = head.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement =
            forward * moveInput.y +
            right * moveInput.x;

        movement *= moveSpeed;

        movement.y = verticalInput * verticalSpeed;

        xrOrigin.transform.position += movement * Time.deltaTime;
    }

    private void Look()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float yaw = lookInput.x * lookSensitivity;
        float pitch = -lookInput.y * lookSensitivity;

        // Xoay ngang toàn bộ XR Origin.
        xrOrigin.transform.Rotate(
            Vector3.up,
            yaw,
            Space.World
        );

        // Xoay dọc chỉ Camera.
        Vector3 currentRotation = head.localEulerAngles;

        float currentPitch = currentRotation.x;

        if (currentPitch > 180f)
            currentPitch -= 360f;

        currentPitch += pitch;
        currentPitch = Mathf.Clamp(currentPitch, -89f, 89f);

        head.localRotation = Quaternion.Euler(
            currentPitch,
            0f,
            0f
        );
    }
}