using UnityEngine;

/// <summary>
/// Measures the angular movement of a paddle around its OarPivot.
/// One complete revolution produces one stroke. Small movements accumulate,
/// so the hand does not need to follow a perfect circle.
/// </summary>
public class BoatCircularStrokeTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatPaddle paddle;
    [SerializeField] private Transform oarPivot;
    [SerializeField] private Transform gripPoint;

    [Header("Direction")]
    [SerializeField] private float directionSign = 1f;
    [SerializeField] private bool requirePaddleHeld = true;

    [Header("Stroke")]
    [SerializeField, Min(1f)] private float degreesPerStroke = 360f;
    [SerializeField, Min(0f)] private float minimumDegreesToCount = 8f;
    [SerializeField, Min(0f)] private float maxInputPerSecond = 3f;

    [Header("Debug")]
    [SerializeField] private bool logValue = true;
    [SerializeField, Min(0.05f)] private float logInterval = 0.25f;

    public float StrokeValue { get; private set; }
    public float AccumulatedDegrees { get; private set; }
    public float Revolutions { get; private set; }

    private Vector3 previousDirection;
    private bool initialized;
    private float logTimer;

    private void Awake()
    {
        if (paddle == null)
            paddle = GetComponent<BoatPaddle>();

        if (gripPoint == null && paddle != null)
        {
            // The tracker is intentionally allowed to use the paddle root
            // if GripPoint is not exposed on BoatPaddle.
            gripPoint = transform;
        }

        ResetTracker();
    }

    private void Update()
    {
        if (paddle == null || oarPivot == null || gripPoint == null)
        {
            StrokeValue = 0f;
            return;
        }

        if (requirePaddleHeld && !paddle.IsHeld)
        {
            StrokeValue = 0f;
            initialized = false;
            return;
        }

        Vector3 current = Vector3.ProjectOnPlane(
            gripPoint.position - oarPivot.position,
            oarPivot.up);

        if (current.sqrMagnitude < 0.000001f)
        {
            StrokeValue = 0f;
            return;
        }

        current.Normalize();

        if (!initialized)
        {
            previousDirection = current;
            initialized = true;
            StrokeValue = 0f;
            return;
        }

        float deltaAngle = Vector3.SignedAngle(
            previousDirection,
            current,
            oarPivot.up);

        previousDirection = current;

        if (Mathf.Abs(deltaAngle) >= minimumDegreesToCount)
        {
            float signedDegrees = deltaAngle * directionSign;
            AccumulatedDegrees += signedDegrees;
        }

        float targetRevolutions = AccumulatedDegrees / degreesPerStroke;
        Revolutions = targetRevolutions;

        // Use the signed angular movement this frame as the rowing input.
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float input = (deltaAngle * directionSign) / degreesPerStroke / dt;
        StrokeValue = Mathf.Clamp(input, -maxInputPerSecond, maxInputPerSecond);

        if (logValue)
        {
            logTimer -= Time.deltaTime;
            if (logTimer <= 0f)
            {
                logTimer = logInterval;
                Debug.Log(
                    $"[STROKE {name}] Held={paddle.IsHeld} " +
                    $"Delta={deltaAngle * directionSign:F2}deg " +
                    $"Accum={AccumulatedDegrees:F2}deg " +
                    $"Rev={Revolutions:F3} " +
                    $"Value={StrokeValue:F3}",
                    this);
            }
        }
    }

    public float GetStrokeValue()
    {
        return StrokeValue;
    }

    public void ResetTracker()
    {
        StrokeValue = 0f;
        AccumulatedDegrees = 0f;
        Revolutions = 0f;
        initialized = false;
        logTimer = 0f;
    }
}
