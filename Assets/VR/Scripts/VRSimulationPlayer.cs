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

    [Header("acceleratrion & speed")]
    [SerializeField] private float maxMoveSpeed = 3f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;

    private float currentMoveSpeed = 0f;
    [Header("Collision")]
    [SerializeField] private bool useGravity = true;
    private CharacterController cc;
    private float verticalVelocity;

    private VRSimulationControls controls;
    private bool dpadLookMode;
    public bool LeftGripPressed { get; private set; }
    public bool RightGripPressed { get; private set; }
    private void Awake()
    {
        controls = new VRSimulationControls();

        // Tránh ghi localRotation trực tiếp lên Camera vì XR tracking có thể ghi đè mỗi frame.
        if (lookPitchPivot == null && head != null)
            lookPitchPivot = head.parent;

        if (lookPitchPivot != null)
            pitchPivotBaseRotation = lookPitchPivot.localRotation;
        if (xrOrigin != null) cc = xrOrigin.GetComponent<CharacterController>();
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
        ReadInteractionButtons();
        HandleDPadMode();
        Move();
        Look();
        HandlePointingMovement();
    }

    private void MoveOrigin(Vector3 delta)
    {
        if (cc != null && cc.enabled) cc.Move(delta);
        else xrOrigin.transform.position += delta;
    }

    private void ReadInteractionButtons()
    {
        LeftGripPressed = controls.LeftHand.Grip.IsPressed();
        RightGripPressed = controls.RightHand.Grip.IsPressed();
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
        // xrOrigin.transform.position += direction * pointingMoveDistance;
        MoveOrigin(direction * pointingMoveDistance);
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

        // --------------------------------------------------
        // XÁC ĐỊNH HƯỚNG DI CHUYỂN
        // --------------------------------------------------

        Vector3 forward = head.forward;
        Vector3 right = head.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 horizontalMovement =
            forward * moveInput.y +
            right * moveInput.x;

        bool wantsToMove =
            horizontalMovement.sqrMagnitude > 0.0001f;

        // --------------------------------------------------
        // TĂNG / GIẢM TỐC
        // --------------------------------------------------

        float targetSpeed = wantsToMove
            ? maxMoveSpeed
            : 0f;

        float rate = targetSpeed > currentMoveSpeed
            ? acceleration
            : deceleration;

        currentMoveSpeed = Mathf.MoveTowards(
            currentMoveSpeed,
            targetSpeed,
            rate * Time.deltaTime
        );

        // --------------------------------------------------
        // DI CHUYỂN NGANG + LEO DỐC
        // --------------------------------------------------

        if (wantsToMove && currentMoveSpeed > 0.0001f)
        {
            horizontalMovement.Normalize();

            Vector3 movementDirection = horizontalMovement;

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
                        movementDirection = slopeMovement;
                    }
                }
                else
                {
                    // Dốc quá cao → không cho đi lên.
                    movementDirection = Vector3.zero;
                }
            }

            if (movementDirection.sqrMagnitude > 0.0001f)
            {
                //xrOrigin.transform.position +=
                //    movementDirection *
                //    currentMoveSpeed *
                 //   Time.deltaTime;
                MoveOrigin(movementDirection * currentMoveSpeed * Time.deltaTime);
            }
        }

        // --------------------------------------------------
        // CTRL / SPACE / GAMEPAD → DI CHUYỂN DỌC RIÊNG
        // --------------------------------------------------

        //xrOrigin.transform.position +=
        //    Vector3.up *
        //    verticalInput *
        //    verticalSpeed *
        //    Time.deltaTime;
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            // Đang bay lên bằng Space/Ctrl/nút gamepad: bỏ trọng lực
            verticalVelocity = 0f;
            MoveOrigin(Vector3.up * verticalInput * verticalSpeed * Time.deltaTime);
        }
        else if (useGravity)
        {
            if (cc != null && cc.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            MoveOrigin(Vector3.up * verticalVelocity * Time.deltaTime);
        }
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
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("Chạm: " + hit.collider.name + " | normal: " + hit.normal);
    }
}