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
    [SerializeField, Min(1f)] private float dpadLookSpeed = 60f;
    [Tooltip("Parent pivot của Camera để xoay nhìn lên/xuống; với XR Origin thường là Camera Offset.")]
    [SerializeField] private Transform lookPitchPivot;

    private Quaternion pitchPivotBaseRotation;
    private float currentLookPitch;

    [Header("Pointing Movement")]
    [SerializeField] private Transform leftHandController;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;
    [SerializeField, Min(0.1f)] private float pointingMoveDistance = 1.5f;

    private VRSimulationControls controls;

    private void Awake()
    {
        controls = new VRSimulationControls();

        // Tránh ghi localRotation trực tiếp lên Camera vì XR tracking có thể ghi đè mỗi frame.
        if (lookPitchPivot == null && head != null)
            lookPitchPivot = head.parent;

        if (lookPitchPivot != null)
            pitchPivotBaseRotation = lookPitchPivot.localRotation;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void Update()
    {
        Move();
        Look();
        HandlePointingMovement();
    }

    private void HandlePointingMovement()
    {
        Gamepad gamepad = Gamepad.current;

        // Shoulder buttons move the player in the direction the corresponding hand points,
        // but only while that hand's stick is held to enter wrist-rotation mode.
        if (gamepad != null)
        {
            if (leftHand != null && leftHand.StickClickPressed &&
                gamepad.leftShoulder.wasPressedThisFrame)
            {
                MoveInPointingDirection(leftHandController);
            }

            if (rightHand != null && rightHand.StickClickPressed &&
                gamepad.rightShoulder.wasPressedThisFrame)
            {
                MoveInPointingDirection(rightHandController);
            }
        }

        // Preserve the original keyboard shortcut: I moves in the right hand's pointing direction.
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            MoveInPointingDirection(rightHandController);
    }

    private void MoveInPointingDirection(Transform hand)
    {
        if (xrOrigin == null || hand == null)
            return;

        Vector3 direction = Vector3.ProjectOnPlane(hand.forward, Vector3.up);
        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        xrOrigin.transform.position += direction * pointingMoveDistance;
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

        Vector2 dpadInput = Vector2.zero;
        if (controls.DPad.Left.IsPressed()) dpadInput.x -= 1f;
        if (controls.DPad.Right.IsPressed()) dpadInput.x += 1f;
        if (controls.DPad.Down.IsPressed()) dpadInput.y -= 1f;
        if (controls.DPad.Up.IsPressed()) dpadInput.y += 1f;

        float yaw = lookInput.x * lookSensitivity
                    + dpadInput.x * dpadLookSpeed * Time.deltaTime;
        float pitchDelta = -lookInput.y * lookSensitivity
                           - dpadInput.y * dpadLookSpeed * Time.deltaTime;

        if (xrOrigin != null && Mathf.Abs(yaw) > 0f)
            xrOrigin.transform.Rotate(Vector3.up, yaw, Space.World);

        // XR tracking may overwrite the Camera's own localRotation every frame.
        // Apply pitch to the parent pivot instead (usually Camera Offset).
        if (lookPitchPivot != null)
        {
            currentLookPitch = Mathf.Clamp(currentLookPitch + pitchDelta, -80f, 80f);
            lookPitchPivot.localRotation =
                pitchPivotBaseRotation * Quaternion.Euler(currentLookPitch, 0f, 0f);
        }
    }
}