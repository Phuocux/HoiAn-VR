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

    [Header("Slope Movement")]
    [SerializeField, Range(0f, 89f)]
    private float maxSlopeAngle = 60f;

    [SerializeField]
    private float groundCheckDistance = 2f;

    [SerializeField]
    private float groundCheckStartHeight = 0.2f;

    [SerializeField]
    private LayerMask groundLayers = ~0;

    private VRSimulationControls controls;
    private bool dpadLookMode;
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
        HandleDPadMode();
        Move();
        Look();
        HandlePointingMovement();
    }

    private void HandleDPadMode()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null && gamepad.selectButton.wasPressedThisFrame)
        {
            dpadLookMode = !dpadLookMode;

            Debug.Log(
                dpadLookMode
                    ? "D-Pad: LOOK MODE"
                    : "D-Pad: MOVE MODE"
            );
        }
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
        // WASD / Player.Move vẫn luôn hoạt động.
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();

        // Chỉ dùng D-Pad để di chuyển khi KHÔNG ở Look Mode.
        if (!dpadLookMode)
        {
            Vector2 dpadInput = Vector2.zero;

            if (controls.DPad.Left.IsPressed())
                dpadInput.x -= 1f;

            if (controls.DPad.Right.IsPressed())
                dpadInput.x += 1f;

            if (controls.DPad.Down.IsPressed())
                dpadInput.y -= 1f;

            if (controls.DPad.Up.IsPressed())
                dpadInput.y += 1f;

            if (dpadInput != Vector2.zero)
                moveInput += dpadInput;

            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }

        float verticalInput =
    controls.Player.VerticalMove.ReadValue<float>();

        Gamepad gamepad = Gamepad.current;

        if (gamepad != null && gamepad.buttonEast.isPressed)
            verticalInput = 1f;

        Vector3 forward = head.forward;
        Vector3 right = head.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 horizontalMovement =
    forward * moveInput.y +
    right * moveInput.x;

        if (horizontalMovement.sqrMagnitude > 0.0001f)
        {
            horizontalMovement.Normalize();

            if (TryGetGround(out RaycastHit hit))
            {
                float slopeAngle =
                    Vector3.Angle(hit.normal, Vector3.up);

                if (slopeAngle <= maxSlopeAngle)
                {
                    Vector3 slopeMovement =
                        Vector3.ProjectOnPlane(
                            horizontalMovement,
                            hit.normal
                        );

                    if (slopeMovement.sqrMagnitude > 0.0001f)
                    {
                        slopeMovement.Normalize();

                        xrOrigin.transform.position +=
                            slopeMovement *
                            moveSpeed *
                            Time.deltaTime;
                    }
                }
            }
            else
            {
                xrOrigin.transform.position +=
                    horizontalMovement *
                    moveSpeed *
                    Time.deltaTime;
            }
        }

        // Ctrl/Space vẫn là VerticalMove riêng.
        xrOrigin.transform.position +=
            Vector3.up *
            verticalInput *
            verticalSpeed *
            Time.deltaTime;
    }

    private bool TryGetGround(out RaycastHit hit)
    {
        Vector3 origin = xrOrigin.transform.position;
        origin.y += groundCheckStartHeight;

        return Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            groundCheckDistance + groundCheckStartHeight,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }
    private void Look()
    {
        // Chuột vẫn luôn nhìn được.
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        Vector2 dpadLookInput = Vector2.zero;

        // Khi Select đã bật, D-Pad chuyển sang điều khiển camera.
        if (dpadLookMode)
        {
            if (controls.DPad.Left.IsPressed())
                dpadLookInput.x -= 1f;

            if (controls.DPad.Right.IsPressed())
                dpadLookInput.x += 1f;

            if (controls.DPad.Down.IsPressed())
                dpadLookInput.y -= 1f;

            if (controls.DPad.Up.IsPressed())
                dpadLookInput.y += 1f;
        }

        float yaw =
            lookInput.x * lookSensitivity +
            dpadLookInput.x * dpadLookSpeed * Time.deltaTime;

        float pitchDelta =
            -lookInput.y * lookSensitivity -
            dpadLookInput.y * dpadLookSpeed * Time.deltaTime;

        if (xrOrigin != null && Mathf.Abs(yaw) > 0f)
            xrOrigin.transform.Rotate(Vector3.up, yaw, Space.World);

        if (lookPitchPivot != null)
        {
            currentLookPitch = Mathf.Clamp(
                currentLookPitch + pitchDelta,
                -80f,
                80f
            );

            lookPitchPivot.localRotation =
                pitchPivotBaseRotation *
                Quaternion.Euler(currentLookPitch, 0f, 0f);
        }
    }
}