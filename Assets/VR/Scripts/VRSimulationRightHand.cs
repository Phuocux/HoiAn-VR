using UnityEngine;
using UnityEngine.InputSystem;

public class VRSimulationRightHand : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform rightController;

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

    private VRSimulationControls controls;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation = Quaternion.identity;
    private bool resetComboActive;

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
        if (rightController == null || cameraTransform == null)
            return;

        startLocalPosition = cameraTransform.InverseTransformPoint(rightController.position);
        startLocalRotation = Quaternion.Inverse(cameraTransform.rotation) * rightController.rotation;
    }

    public void ResetHandPose()
    {
        if (rightController == null || cameraTransform == null)
            return;

        rightController.position = cameraTransform.TransformPoint(startLocalPosition);
        rightController.rotation = cameraTransform.rotation * startLocalRotation;
    }

    private void Update()
    {
        ReadButtons();

        if (!CustomMode || rightController == null || cameraTransform == null)
            return;

        Gamepad gamepad = Gamepad.current;

        // LT/RT resets only when held together with the matching stick click.
        // On its own, LT/RT remains MoveZ (move the hand backward).
        bool resetCombo = StickClickPressed && gamepad != null && gamepad.rightTrigger.isPressed;
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

    private void RotateWrist()
    {
        Vector2 input = controls.RightHand.MoveXY.ReadValue<Vector2>();
        float delta = wristRotationSpeed * Time.deltaTime;

        // Rotate around camera-relative yaw and pitch axes without changing position.
        Quaternion yaw = Quaternion.AngleAxis(input.x * delta, cameraTransform.up);
        Quaternion pitch = Quaternion.AngleAxis(-input.y * delta, cameraTransform.right);
        rightController.rotation = pitch * yaw * rightController.rotation;
    }

    private void MoveHand()
    {
        Vector2 xy = controls.RightHand.MoveXY.ReadValue<Vector2>();
        float z = controls.RightHand.MoveZ.ReadValue<float>();

        Vector3 current = cameraTransform.InverseTransformPoint(rightController.position);
        current.x += xy.x * moveXYSpeed * Time.deltaTime;
        current.y += xy.y * moveXYSpeed * Time.deltaTime;
        current.z += z * moveZSpeed * Time.deltaTime;

        current.x = Mathf.Clamp(current.x, startLocalPosition.x - maxX, startLocalPosition.x + maxX);
        current.y = Mathf.Clamp(current.y, startLocalPosition.y - maxY, startLocalPosition.y + maxY);
        current.z = Mathf.Clamp(current.z, minZ, maxZ);

        rightController.position = cameraTransform.TransformPoint(current);
    }

    private void ReadButtons()
    {
        GripPressed = controls.RightHand.Grip.IsPressed();
        TriggerPressed = controls.RightHand.Trigger.IsPressed();
        StickClickPressed = controls.RightHand.StickClick.IsPressed();
    }
}
