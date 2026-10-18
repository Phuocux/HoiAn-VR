using UnityEngine;

/// <summary>
/// Boat Movement - Easy Rowing.
///
/// Boat model axes:
/// X = left/right
/// Y = forward/back
/// Z = height
///
/// Cơ chế:
/// - Có chèo: tăng tốc dần tới tốc độ mục tiêu.
/// - Ngừng chèo: giảm tốc nhanh về 0.
/// - Không có input: không thể tiếp tục tăng tốc.
/// </summary>
public class BoatMovement : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] private float waterHeight = 0f;
    [SerializeField] private bool lockHeight = true;

    [Header("Paddles")]
    [SerializeField] private BoatPaddle leftPaddle;
    [SerializeField] private BoatPaddle rightPaddle;

    [Header("Forward Movement")]
    [SerializeField, Min(0f)]
    private float maxForwardSpeed = 3f;

    [SerializeField, Min(0f)]
    private float forwardAcceleration = 4f;

    [SerializeField, Min(0f)]
    private float forwardDeceleration = 10f;

    [Header("Turning")]
    [SerializeField, Min(0f)]
    private float maxTurnSpeed = 90f;

    [SerializeField, Min(0f)]
    private float turnAcceleration = 8f;

    [SerializeField, Min(0f)]
    private float turnDeceleration = 15f;

    [SerializeField, Min(0f)]
    private float turnDifferenceMultiplier = 2f;

    [Header("Input")]
    [SerializeField, Min(0f)]
    private float strokeDeadzone = 0.05f;

    [SerializeField, Min(0f)]
    private float strokeResponse = 2f;

    [Header("Direction")]
    [SerializeField]
    private bool invertForwardDirection = true;

    [SerializeField]
    private bool invertTurnDirection = false;

    private float currentForwardSpeed;
    private float currentTurnSpeed;

    [Header("Collision")]
    [SerializeField] private BoxCollider collisionBox;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField, Min(0f)]
    private float collisionSkin = 0.03f;
    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void LateUpdate()
    {
        float left = ReadStroke(leftPaddle);
        float right = ReadStroke(rightPaddle);

        // ==================================================
        // FORWARD
        // ==================================================

        float forwardInput =
            (Mathf.Abs(left) + Mathf.Abs(right)) * 0.5f;

        forwardInput = Mathf.Clamp01(forwardInput);

        // Không chèo = mục tiêu vận tốc bằng 0.
        float targetForwardSpeed =
            forwardInput * maxForwardSpeed;

        if (invertForwardDirection)
            targetForwardSpeed = -targetForwardSpeed;

        // Có input → tăng tốc.
        // Không có input → giảm tốc.
        float forwardRate;

        if (Mathf.Abs(targetForwardSpeed) >
            Mathf.Abs(currentForwardSpeed))
        {
            forwardRate = forwardAcceleration;
        }
        else
        {
            forwardRate = forwardDeceleration;
        }

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            targetForwardSpeed,
            forwardRate * Time.deltaTime
        );

        // Nếu target đã là 0 thì ép về 0 khi đủ nhỏ.
        if (Mathf.Abs(targetForwardSpeed) <= 0.0001f &&
            Mathf.Abs(currentForwardSpeed) <= 0.02f)
        {
            currentForwardSpeed = 0f;
        }

        // ==================================================
        // TURN
        // ==================================================

        // Dùng độ lớn của từng mái chèo.
        // Mái chèo bên nào mạnh hơn thì quay về bên đó.
        float turnInput =
            Mathf.Abs(left) - Mathf.Abs(right);

        turnInput = Mathf.Clamp(
            turnInput * turnDifferenceMultiplier,
            -1f,
            1f
        );

        if (invertTurnDirection)
            turnInput = -turnInput;

        float targetTurnSpeed =
            turnInput * maxTurnSpeed;

        // Có chênh lệch → tăng tốc xoay.
        // Không còn chênh lệch → giảm xoay về 0.
        float turnRate;

        if (Mathf.Abs(targetTurnSpeed) >
            Mathf.Abs(currentTurnSpeed))
        {
            turnRate = turnAcceleration;
        }
        else
        {
            turnRate = turnDeceleration;
        }

        currentTurnSpeed = Mathf.MoveTowards(
            currentTurnSpeed,
            targetTurnSpeed,
            turnRate * Time.deltaTime
        );

        if (Mathf.Abs(targetTurnSpeed) <= 0.0001f &&
            Mathf.Abs(currentTurnSpeed) <= 0.02f)
        {
            currentTurnSpeed = 0f;
        }

        // ==================================================
        // APPLY MOVEMENT
        // ==================================================

        transform.Rotate(
            Vector3.forward,
            currentTurnSpeed * Time.deltaTime,
            Space.Self
        );

        Vector3 movement =
    transform.up *
    currentForwardSpeed *
    Time.deltaTime;

        MoveWithCollision(movement);

        // ==================================================
        // LOCK HEIGHT
        // ==================================================

        if (lockHeight)
        {
            Vector3 position = transform.position;
            position.z = waterHeight;
            transform.position = position;
        }
    }

    private float ReadStroke(BoatPaddle paddle)
    {
        if (paddle == null || !paddle.IsHeld)
            return 0f;

        float stroke = paddle.StrokeInput;

        if (Mathf.Abs(stroke) <= strokeDeadzone)
            return 0f;

        return Mathf.Clamp(
            stroke * strokeResponse,
            -1f,
            1f
        );
    }

    public float GetForwardSpeed()
    {
        return currentForwardSpeed;
    }

    public float GetTurnSpeed()
    {
        return currentTurnSpeed;
    }

    private void MoveWithCollision(Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.000001f)
            return;

        if (collisionBox == null)
        {
            transform.position += movement;
            return;
        }

        float distance = movement.magnitude;
        Vector3 direction = movement / distance;

        Transform boxTransform = collisionBox.transform;

        Vector3 scale = boxTransform.lossyScale;

        Vector3 halfExtents = Vector3.Scale(
            collisionBox.size * 0.5f,
            new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            )
        );

        Vector3 center =
            boxTransform.TransformPoint(collisionBox.center);

        Quaternion boxRotation = boxTransform.rotation;

        Physics.SyncTransforms();

        // 1. Kiểm tra đường di chuyển phía trước.
        bool hitObstacle = Physics.BoxCast(
            center,
            halfExtents,
            direction,
            out RaycastHit hit,
            boxRotation,
            distance + collisionSkin,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (hitObstacle)
        {
            float allowedDistance = Mathf.Clamp(
                hit.distance - collisionSkin,
                0f,
                distance
            );

            transform.position += direction * allowedDistance;

            // Ngừng đà chèo khi va chạm.
            currentForwardSpeed = 0f;
            return;
        }

        // 2. Kiểm tra vị trí dự kiến sau khi di chuyển.
        Vector3 targetCenter = center + movement;

        bool targetBlocked = Physics.CheckBox(
            targetCenter,
            halfExtents,
            boxRotation,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (targetBlocked)
        {
            currentForwardSpeed = 0f;
            return;
        }

        // 3. Không có vật cản thì mới di chuyển.
        transform.position += movement;
    }
}