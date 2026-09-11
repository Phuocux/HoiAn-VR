using UnityEngine;

public class VRSimulationLeftHand : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform leftController;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Speed")]
    [SerializeField] private float moveXYSpeed = 1.5f;
    [SerializeField] private float moveZSpeed = 1.5f;

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

    public void ResetHandPosition()
    {
        if (leftController == null || cameraTransform == null)
            return;

        startLocalPosition =
            cameraTransform.InverseTransformPoint(
                leftController.position
            );
    }

    private void Update()
    {
        ReadButtons();

        if (!CustomMode)
            return;

        MoveHand();
    }

    private void MoveHand()
    {
        Vector2 xy =
            controls.LeftHand.MoveXY.ReadValue<Vector2>();

        float z =
            controls.LeftHand.MoveZ.ReadValue<float>();

        Vector3 current =
            cameraTransform.InverseTransformPoint(
                leftController.position
            );

        current.x += xy.x * moveXYSpeed * Time.deltaTime;
        current.y += xy.y * moveXYSpeed * Time.deltaTime;
        current.z += z * moveZSpeed * Time.deltaTime;

        current.x = Mathf.Clamp(
            current.x,
            startLocalPosition.x - maxX,
            startLocalPosition.x + maxX
        );

        current.y = Mathf.Clamp(
            current.y,
            startLocalPosition.y - maxY,
            startLocalPosition.y + maxY
        );

        current.z = Mathf.Clamp(
            current.z,
            minZ,
            maxZ
        );

        leftController.position =
            cameraTransform.TransformPoint(current);
    }

    private void ReadButtons()
    {
        GripPressed =
            controls.LeftHand.Grip.IsPressed();

        TriggerPressed =
            controls.LeftHand.Trigger.IsPressed();

        StickClickPressed =
            controls.LeftHand.StickClick.IsPressed();
    }
}