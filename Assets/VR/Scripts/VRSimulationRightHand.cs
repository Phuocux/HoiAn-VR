using UnityEngine;

public class VRSimulationRightHand : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform rightController;

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
        if (rightController == null || cameraTransform == null)
            return;

        startLocalPosition =
            cameraTransform.InverseTransformPoint(
                rightController.position
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
            controls.RightHand.MoveXY.ReadValue<Vector2>();

        float z =
            controls.RightHand.MoveZ.ReadValue<float>();

        Vector3 current =
            cameraTransform.InverseTransformPoint(
                rightController.position
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

        rightController.position =
            cameraTransform.TransformPoint(current);
    }

    private void ReadButtons()
    {
        GripPressed =
            controls.RightHand.Grip.IsPressed();

        TriggerPressed =
            controls.RightHand.Trigger.IsPressed();

        StickClickPressed =
            controls.RightHand.StickClick.IsPressed();
    }
}