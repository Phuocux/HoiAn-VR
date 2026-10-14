using UnityEngine;

/// <summary>
/// Grip is a toggle: press once to latch a simulated hand to the handlebar,
/// press again to release. Relative hand extension along Cyclo local X steers
/// the front pivot around local Y.
/// </summary>
public class BikeHandlebar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BikeMode bikeMode;
    [SerializeField] private Transform frontSteeringPivot;
    [SerializeField] private Transform leftGripPoint;
    [SerializeField] private Transform rightGripPoint;
    [SerializeField] private Transform leftHandController;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private VRSimulationLeftHand leftHand;
    [SerializeField] private VRSimulationRightHand rightHand;

    [Header("Grip")]
    [SerializeField, Min(0.05f)] private float grabDistance = 0.4f;

    [Header("Steering")]
    [SerializeField, Range(1f, 60f)] private float maxSteerAngle = 20f;
    [SerializeField, Min(0.01f)] private float maxHandSteerOffset = 0.05f;
    [SerializeField] private bool invertSteering;

    public float SteeringAngle { get; private set; }
    public bool LeftHeld => leftHeld;
    public bool RightHeld => rightHeld;

    private bool leftHeld;
    private bool rightHeld;
    private bool leftGripWasPressed;
    private bool rightGripWasPressed;
    private Quaternion basePivotLocalRotation = Quaternion.identity;

    private void Awake()
    {
        if (frontSteeringPivot != null)
            basePivotLocalRotation = frontSteeringPivot.localRotation;
    }

    public void BeginBikeMode()
    {
        if (frontSteeringPivot != null)
        {
            basePivotLocalRotation = frontSteeringPivot.localRotation;
            frontSteeringPivot.localRotation = basePivotLocalRotation;
        }

        leftGripWasPressed = leftHand != null && leftHand.GripPressed;
        rightGripWasPressed = rightHand != null && rightHand.GripPressed;
        SteeringAngle = 0f;
    }

    public void EndBikeMode()
    {
        ReleaseLeft();
        ReleaseRight();
        leftGripWasPressed = false;
        rightGripWasPressed = false;
        SteeringAngle = 0f;
        if (frontSteeringPivot != null)
            frontSteeringPivot.localRotation = basePivotLocalRotation;
    }

    private void Update()
    {
        if (bikeMode == null || !bikeMode.IsOnBike || frontSteeringPivot == null)
            return;

        bool leftPressed = leftHand != null && leftHand.GripPressed;
        bool rightPressed = rightHand != null && rightHand.GripPressed;

        // Rising edge: press once to grab, press again to release.
        if (leftPressed && !leftGripWasPressed)
        {
            if (leftHeld)
                ReleaseLeft();
            else if (IsNearGrip(leftHandController, leftGripPoint))
                GrabLeft();
        }

        if (rightPressed && !rightGripWasPressed)
        {
            if (rightHeld)
                ReleaseRight();
            else if (IsNearGrip(rightHandController, rightGripPoint))
                GrabRight();
        }

        leftGripWasPressed = leftPressed;
        rightGripWasPressed = rightPressed;
    }

    private void LateUpdate()
    {
        if (bikeMode == null || !bikeMode.IsOnBike || frontSteeringPivot == null)
            return;

        float leftExtension = leftHeld && leftHand != null ? leftHand.BikeHoldExtension : 0f;
        float rightExtension = rightHeld && rightHand != null ? rightHand.BikeHoldExtension : 0f;
        float normalizedInput = 0f;

        if (leftHeld && rightHeld)
        {
            // Opposite extension (right forward, left backward) produces full steering.
            normalizedInput = (rightExtension - leftExtension) / (2f * maxHandSteerOffset);
        }
        else if (rightHeld)
        {
            normalizedInput = rightExtension / maxHandSteerOffset;
        }
        else if (leftHeld)
        {
            normalizedInput = -leftExtension / maxHandSteerOffset;
        }

        normalizedInput = Mathf.Clamp(normalizedInput, -1f, 1f);
        SteeringAngle = -normalizedInput * maxSteerAngle * (invertSteering ? -1f : 1f);

        // Only the front assembly turns around local Y.
        frontSteeringPivot.localRotation = basePivotLocalRotation *
            Quaternion.AngleAxis(SteeringAngle, Vector3.up);

        // Keep the visible hands locked to the moving grip points AFTER the
        // pivot has rotated. Extension values above are used only for steering.
        if (leftHeld && leftHandController != null && leftGripPoint != null)
            leftHandController.SetPositionAndRotation(leftGripPoint.position, leftGripPoint.rotation);

        if (rightHeld && rightHandController != null && rightGripPoint != null)
            rightHandController.SetPositionAndRotation(rightGripPoint.position, rightGripPoint.rotation);
    }

    private bool IsNearGrip(Transform hand, Transform gripPoint)
    {
        return hand != null && gripPoint != null &&
               Vector3.Distance(hand.position, gripPoint.position) <= grabDistance;
    }

    private void GrabLeft()
    {
        if (leftHandController == null || leftGripPoint == null ||
            frontSteeringPivot == null || leftHand == null || bikeMode == null)
            return;

        leftHandController.SetPositionAndRotation(leftGripPoint.position, leftGripPoint.rotation);
        leftHand.BeginBikeHold(leftGripPoint, bikeMode.transform);
        leftHeld = leftHand.IsBikeHeld;
    }

    private void GrabRight()
    {
        if (rightHandController == null || rightGripPoint == null ||
            frontSteeringPivot == null || rightHand == null || bikeMode == null)
            return;

        rightHandController.SetPositionAndRotation(rightGripPoint.position, rightGripPoint.rotation);
        rightHand.BeginBikeHold(rightGripPoint, bikeMode.transform);
        rightHeld = rightHand.IsBikeHeld;
    }

    private void ReleaseLeft()
    {
        if (leftHand != null)
            leftHand.EndBikeHold();
        leftHeld = false;
    }

    private void ReleaseRight()
    {
        if (rightHand != null)
            rightHand.EndBikeHold();
        rightHeld = false;
    }
}
