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
    }

    private void Update()
    {
        ReadButtons();

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

    private void RotateWrist()
    {
        Vector2 input = controls.LeftHand.MoveXY.ReadValue<Vector2>();
        float delta = wristRotationSpeed * Time.deltaTime;

        // Rotate around camera-relative yaw and pitch axes without changing position.
        Quaternion yaw = Quaternion.AngleAxis(input.x * delta, cameraTransform.up);
        Quaternion pitch = Quaternion.AngleAxis(-input.y * delta, cameraTransform.right);
        leftController.rotation = pitch * yaw * leftController.rotation;
    }

    private void MoveHand()
    {
        Vector2 xy = controls.LeftHand.MoveXY.ReadValue<Vector2>();
        float z = controls.LeftHand.MoveZ.ReadValue<float>();

        Vector3 current = cameraTransform.InverseTransformPoint(leftController.position);
        current.x += xy.x * moveXYSpeed * Time.deltaTime;
        current.y += xy.y * moveXYSpeed * Time.deltaTime;
        current.z += z * moveZSpeed * Time.deltaTime;

        current.x = Mathf.Clamp(current.x, startLocalPosition.x - maxX, startLocalPosition.x + maxX);
        current.y = Mathf.Clamp(current.y, startLocalPosition.y - maxY, startLocalPosition.y + maxY);
        current.z = Mathf.Clamp(current.z, minZ, maxZ);

        leftController.position = cameraTransform.TransformPoint(current);
    }

    private void ReadButtons()
    {
        GripPressed = controls.LeftHand.Grip.IsPressed();
        TriggerPressed = controls.LeftHand.Trigger.IsPressed();
        StickClickPressed = controls.LeftHand.StickClick.IsPressed();
    }
}
