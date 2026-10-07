using UnityEngine;

/// Giữ nguyên hệ di chuyển cũ của XRI (Move, Teleport, Gravity...).
/// Script này KHÔNG đọc input, chỉ giúp nhân vật:
///  - bước lên bậc thang khi tiến (nhờ Step Offset)
///  - bám sát mặt bậc khi lùi / đi xuống (không bị "bay lơ lửng" rồi rơi)
/// Gắn vào XR Origin (cùng object có Character Controller).
[RequireComponent(typeof(CharacterController))]
public class VRStairHelper : MonoBehaviour
{
    [Tooltip("Chiều cao tối đa của 1 bậc thang (mét). Phải >= chiều cao bậc.")]
    public float stepHeight = 0.35f;

    [Tooltip("Bám xuống bậc bên dưới khi đi xuống")]
    public bool snapDown = true;

    CharacterController cc;
    bool wasGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.stepOffset = stepHeight;
        cc.slopeLimit = 60f;
        cc.skinWidth = 0.02f;
    }

    void LateUpdate()
    {
        if (!snapDown) { wasGrounded = cc.isGrounded; return; }

        bool grounded = cc.isGrounded;

        // Vừa rời mặt đất mà không phải đang nhảy lên -> tìm bậc ngay bên dưới
        if (wasGrounded && !grounded && cc.velocity.y <= 0.1f)
        {
            Bounds b = cc.bounds;
            Vector3 origin = new Vector3(b.center.x, b.min.y + 0.05f, b.center.z);
            float maxDist = stepHeight + 0.05f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist,
                                ~0, QueryTriggerInteraction.Ignore) && hit.distance > 0.01f)
            {
                cc.Move(Vector3.down * (hit.distance - 0.05f));
                grounded = cc.isGrounded;
            }
        }

        wasGrounded = grounded;
    }
}
