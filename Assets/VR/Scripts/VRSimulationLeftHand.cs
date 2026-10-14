using UnityEngine;
using UnityEngine.InputSystem;

public class VRSimulationLeftHand : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform leftController;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Speed")]
    [SerializeField] private float moveXYSpeed = 1.5f;
    [SerializeField] private float moveZSpeed = 1.5f;

    [Header("Wrist Rotation")]
    [SerializeField, Min(1f)] private float wristRotationSpeed = 90f;

    [Header("Movement Limits")]
    [SerializeField] private float maxX = 0.60f;
    [SerializeField] private float maxY = 0.50f;
    [SerializeField] private float minZ = 0.20f;
    [SerializeField] private float maxZ = 1.20f;

    public bool CustomMode { get; set; }

    public bool GripPressed { get; private set; }
    public bool TriggerPressed { get; private set; }
    public bool StickClickPressed { get; private set; }

    public float BikeSteeringInput =>
    controls != null
        ? controls.LeftHand.MoveXY.ReadValue<Vector2>().x
        : 0f;

    private VRSimulationControls controls;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation = Quaternion.identity;
    private bool resetComboActive;

    [Header("Paddle Hold")]
    [SerializeField, Min(0.05f)] private float paddleHoldRange = 0.35f;

    private bool paddleHoldMode;
    private Vector3 paddleHoldLocalPosition;

    [Header("Boat Follow")]
    private Transform boatTransform;
    private Vector3 boatLocalPosition;
    private Quaternion boatLocalRotation;
    private bool boatFollowMode;

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

    private void OnDestroy()
    {
        controls.Dispose();
    }

    public void ResetHandPosition()
    {
        if (leftController == null || cameraTransform == null)
            return;

        startLocalPosition = cameraTransform.InverseTransformPoint(leftController.position);
        startLocalRotation = Quaternion.Inverse(cameraTransform.rotation) * leftController.rotation;
    }

    public void ResetHandPose()
    {
        if (leftController == null || cameraTransform == null)
            return;

        leftController.position = cameraTransform.TransformPoint(startLocalPosition);
        leftController.rotation = cameraTransform.rotation * startLocalRotation;
        SaveBoatLocalPose();
    }


    /// <summary>
    /// Switches this hand to a local hold area around the current paddle grip.
    /// The center is captured once at grab time, so camera movement does not
    /// move the hand's working area while the paddle is held.
    /// </summary>
    public void BeginPaddleHold()
    {
        if (leftController == null)
            return;

        paddleHoldLocalPosition =
            cameraTransform.InverseTransformPoint(leftController.position);
        paddleHoldMode = true;
    }

    /// <summary>
    /// Returns the hand to normal camera-relative movement without changing
    /// its current world position.
    /// </summary>
    public void EndPaddleHold()
    {
        paddleHoldMode = false;
    }

    public void BeginBoatFollow(Transform boat)
    {
        if (leftController == null || boat == null)
            return;

        boatTransform = boat;
        boatLocalPosition = boatTransform.InverseTransformPoint(leftController.position);
        boatLocalRotation = Quaternion.Inverse(boatTransform.rotation) * leftController.rotation;
        boatFollowMode = true;
    }

    public void EndBoatFollow()
    {
        boatFollowMode = false;
        boatTransform = null;
    }

    private void ApplyBoatFollowBeforeInput()
    {
        if (!boatFollowMode || boatTransform == null || leftController == null)
            return;

        leftController.position = boatTransform.TransformPoint(boatLocalPosition);
        leftController.rotation = boatTransform.rotation * boatLocalRotation;
    }

    private void SaveBoatLocalPose()
    {
        if (!boatFollowMode || boatTransform == null || leftController == null)
            return;

        boatLocalPosition = boatTransform.InverseTransformPoint(leftController.position);
        boatLocalRotation = Quaternion.Inverse(boatTransform.rotation) * leftController.rotation;
    }

    public void ApplyBoatFollow()
    {
        // XR Origin is parented to the boat while seated, so the controller
        // already follows boat motion. Never overwrite an actively held
        // paddle with a stale boat-local snapshot.
        if (paddleHoldMode || bikeHoldMode)
            return;

        if (!boatFollowMode || boatTransform == null || leftController == null)
            return;

        leftController.position = boatTransform.TransformPoint(boatLocalPosition);
        leftController.rotation = boatTransform.rotation * boatLocalRotation;
    }

    [Header("Bike Hold")]
    [SerializeField, Min(0.01f)] private float bikeHoldRangeX = 0.05f;
    private bool bikeHoldMode;
    private Transform bikeHoldSpace;
    private Transform bikeHoldAxis;
    private float bikeHoldExtension;
    private Quaternion bikeHoldLocalRotation = Quaternion.identity;

    /// <summary>Signed extension along the Cyclo's local X axis while this hand is latched.</summary>
    public float BikeHoldExtension => bikeHoldExtension;
    public bool IsBikeHeld => bikeHoldMode;

    public void BeginBikeHold(Transform holdSpace, Transform movementAxis)
    {
        if (leftController == null || holdSpace == null || movementAxis == null)
            return;

        bikeHoldSpace = holdSpace;
        bikeHoldAxis = movementAxis;
        bikeHoldExtension = 0f;
        bikeHoldLocalRotation = Quaternion.Inverse(holdSpace.rotation) * leftController.rotation;
        bikeHoldMode = true;
        paddleHoldMode = false;
    }

    public void EndBikeHold()
    {
        bikeHoldMode = false;
        bikeHoldSpace = null;
        bikeHoldAxis = null;
        bikeHoldExtension = 0f;
    }

    private void Update()
    {
        ReadButtons();

        // While gripping the bike, hand movement is relative to the steering pivot, not the camera.
        if (bikeHoldMode && bikeHoldSpace != null && leftController != null)
        {
            MoveBikeHeldHand();
            return;
        }

        if (!CustomMode || leftController == null || cameraTransform == null)
            return;

        Gamepad gamepad = Gamepad.current;

        // LT/RT resets only when held together with the matching stick click.
        // On its own, LT/RT remains MoveZ (move the hand backward).
        bool resetCombo = StickClickPressed && gamepad != null && gamepad.leftTrigger.isPressed;
        if (resetCombo)
        {
            if (!resetComboActive)
                ResetHandPose();

            resetComboActive = true;
            return;
        }

        resetComboActive = false;

        // Hold StickClick to suspend positional movement and rotate the wrist instead.
        if (StickClickPressed)
            RotateWrist();
        else
            MoveHand();
    }

    private void MoveBikeHeldHand()
    {
        if (leftController == null || bikeHoldSpace == null)
            return;

        // LB extends / LT retracts the hidden steering value. This value is
        // intentionally NOT applied to the visible controller transform.
        float input = controls.LeftHand.MoveZ.ReadValue<float>();
        bikeHoldExtension = Mathf.Clamp(
            bikeHoldExtension + input * moveZSpeed * Time.deltaTime,
            -bikeHoldRangeX,
            bikeHoldRangeX
        );

        // Visual hand stays exactly at the grip point. BikeHandlebar also
        // reapplies this pose in LateUpdate after the steering pivot rotates.
        leftController.SetPositionAndRotation(
            bikeHoldSpace.position,
            bikeHoldSpace.rotation * bikeHoldLocalRotation
        );
    }

    private void RotateWrist()
    {
        Vector2 input = controls.LeftHand.MoveXY.ReadValue<Vector2>();
        float delta = wristRotationSpeed * Time.deltaTime;

        // Rotate around camera-relative yaw and pitch axes without changing position.
        Quaternion yaw = Quaternion.AngleAxis(input.x * delta, cameraTransform.up);
        Quaternion pitch = Quaternion.AngleAxis(-input.y * delta, cameraTransform.right);
        leftController.rotation = pitch * yaw * leftController.rotation;
        SaveBoatLocalPose();
    }

    private void MoveHand()
    {
        Vector2 xy = controls.LeftHand.MoveXY.ReadValue<Vector2>();
        float z = controls.LeftHand.MoveZ.ReadValue<float>();

        if (paddleHoldMode)
        {
            // While holding the paddle, keep the hand camera-relative.
            // The hold position is stored in camera-local space, so moving
            // or rotating the camera also carries the hand with it.
            // Keep the CURRENT hand position in camera-local space.
            // The previous version rebuilt from the original grab position
            // every frame, which made the hand appear frozen.
            Vector3 holdLocal =
                cameraTransform.InverseTransformPoint(leftController.position);

            holdLocal.x +=
                xy.x * moveXYSpeed * Time.deltaTime;

            holdLocal.y +=
                xy.y * moveXYSpeed * Time.deltaTime;

            holdLocal.z +=
                z * moveZSpeed * Time.deltaTime;

            Vector3 startHoldLocal =
                paddleHoldLocalPosition;

            holdLocal.x = Mathf.Clamp(
                holdLocal.x,
                startHoldLocal.x - paddleHoldRange,
                startHoldLocal.x + paddleHoldRange);

            holdLocal.y = Mathf.Clamp(
                holdLocal.y,
                startHoldLocal.y - paddleHoldRange,
                startHoldLocal.y + paddleHoldRange);

            holdLocal.z = Mathf.Clamp(
                holdLocal.z,
                startHoldLocal.z - paddleHoldRange,
                startHoldLocal.z + paddleHoldRange);

            leftController.position =
                cameraTransform.TransformPoint(holdLocal);

            // Do NOT overwrite this with a stale boat-local pose here.
            return;
        }

        Vector3 normalCurrent =
            cameraTransform.InverseTransformPoint(
                leftController.position);

        normalCurrent.x +=
            xy.x * moveXYSpeed * Time.deltaTime;

        normalCurrent.y +=
            xy.y * moveXYSpeed * Time.deltaTime;

        normalCurrent.z +=
            z * moveZSpeed * Time.deltaTime;

        normalCurrent.x = Mathf.Clamp(
            normalCurrent.x,
            startLocalPosition.x - maxX,
            startLocalPosition.x + maxX);

        normalCurrent.y = Mathf.Clamp(
            normalCurrent.y,
            startLocalPosition.y - maxY,
            startLocalPosition.y + maxY);

        normalCurrent.z = Mathf.Clamp(
            normalCurrent.z,
            minZ,
            maxZ);

        leftController.position =
            cameraTransform.TransformPoint(
                normalCurrent);

        SaveBoatLocalPose();
    }

    private void ReadButtons()
    {
        GripPressed = controls.LeftHand.Grip.IsPressed();
        TriggerPressed = controls.LeftHand.Trigger.IsPressed();
        StickClickPressed = controls.LeftHand.StickClick.IsPressed();
    }
}
