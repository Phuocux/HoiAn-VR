using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Forward acceleration, braking, and bicycle-style steering.</summary>
public class BikeMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BikeMode bikeMode;
    [SerializeField] private BikeHandlebar handlebar;

    [Header("Speed (metres/second if 1 Unity unit = 1 metre)")]
    [SerializeField, Min(0f)] private float maxSpeed = 4f;
    [SerializeField, Min(0f)] private float acceleration = 1.8f;
    [SerializeField, Min(0f)] private float coastingDeceleration = 1.2f;
    [SerializeField, Min(0f)] private float brakeDeceleration = 8f;

    [Header("Steering")]
    [SerializeField, Min(0.1f)] private float wheelbase = 1.2f;
    [SerializeField] private bool invertForward = false;
    [SerializeField] private bool invertSteering = false;

    public float CurrentSpeed => currentSpeed;
    private float currentSpeed;

    private void Update()
    {
        if (bikeMode == null || !bikeMode.IsOnBike)
        {
            currentSpeed = 0f;
            return;
        }

        Gamepad gamepad = Gamepad.current;
        bool accelerateHeld = gamepad != null && gamepad.buttonEast.isPressed;
        bool brakeHeld = gamepad != null && gamepad.selectButton.isPressed;

        float targetSpeed = accelerateHeld && !brakeHeld ? maxSpeed : 0f;
        float rate = brakeHeld ? brakeDeceleration :
            (accelerateHeld ? acceleration : coastingDeceleration);

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        float directionSign = invertForward ? -1f : 1f;
        // Cyclo model's forward direction is local +X (not Unity's default +Z).
        transform.position += transform.right * (currentSpeed * directionSign * Time.deltaTime);

        if (handlebar != null && currentSpeed > 0.001f)
        {
            float steeringAngle = handlebar.SteeringAngle;
            if (invertSteering)
                steeringAngle = -steeringAngle;

            // Steering pivot and vehicle yaw both rotate around Y. Since the
            // vehicle advances along local X, transform.right is its forward axis.
            // Bicycle kinematics: yaw rate = v / wheelbase * tan(steering angle).
            float yawRateRadians = (currentSpeed / Mathf.Max(0.1f, wheelbase)) *
                                   Mathf.Tan(steeringAngle * Mathf.Deg2Rad);
            transform.Rotate(Vector3.up, yawRateRadians * Mathf.Rad2Deg * Time.deltaTime, Space.World);
        }
    }

    public void ResetSpeed()
    {
        currentSpeed = 0f;
    }
}
