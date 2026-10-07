using UnityEngine;
using UnityEngine.InputSystem;

/// Gắn vào XR Origin (cùng với CharacterController).
/// Đẩy cần analog (hoặc bóp trigger) về phía trước -> tiến, tự bước lên bậc thang.
/// Kéo cần xuống (hoặc bóp grip) -> lùi, tự đi xuống bậc thang.
[RequireComponent(typeof(CharacterController))]
public class VRStairWalker : MonoBehaviour
{
    [Header("Input (kéo action từ XRI Default Input Actions)")]
    public InputActionProperty moveAction;     // XRI LeftHand Locomotion/Move (Vector2)
    public InputActionProperty triggerAction;  // XRI RightHand Interaction/Activate Value (float) -> TIẾN
    public InputActionProperty backAction;     // XRI RightHand Interaction/Select Value (grip, float) -> LÙI

    [Header("Di chuyển")]
    public Transform head;                     // Main Camera trong XR Origin
    public float speed = 1.6f;
    public float gravity = -9.81f;

    CharacterController cc;
    float vy;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.stepOffset = 0.35f;   // phải >= chiều cao 1 bậc thang
        cc.slopeLimit = 60f;
        cc.skinWidth = 0.02f;
        cc.minMoveDistance = 0f;
    }

    void OnEnable()
    {
        moveAction.action?.Enable();
        triggerAction.action?.Enable();
        backAction.action?.Enable();
    }

    void Update()
    {
        // Đặt capsule theo vị trí đầu (khi người chơi đi lại trong phòng)
        float h = Mathf.Clamp(head.localPosition.y, 1f, 2.2f);
        cc.height = h;
        cc.center = new Vector3(head.localPosition.x, h * 0.5f + cc.skinWidth, head.localPosition.z);

        // Hướng tiến = hướng nhìn, bỏ độ cao
        Vector3 fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;

        Vector2 stick = moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        float trig = triggerAction.action != null ? triggerAction.action.ReadValue<float>() : 0f;

        float back = backAction.action != null ? backAction.action.ReadValue<float>() : 0f;

        // Tiến: cần đẩy lên hoặc bóp trigger. Lùi: kéo cần xuống hoặc bóp grip.
        // Lùi trên cầu thang = đi xuống.
        float forward = Mathf.Clamp(stick.y + trig - back, -1f, 1f);
        Vector3 move = (fwd * forward + right * stick.x) * speed;

        // Trọng lực giữ chân dính mặt bậc, để đi xuống không bị nảy
        vy = cc.isGrounded ? -4f : vy + gravity * Time.deltaTime;
        move.y = vy;

        cc.Move(move * Time.deltaTime);
    }
}