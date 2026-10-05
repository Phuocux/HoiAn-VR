using UnityEngine;

public class HeadBlocker : MonoBehaviour
{
    [SerializeField] Transform xrOrigin;      // kéo XR Origin vào
    [SerializeField] Transform head;          // kéo Main Camera vào
    [SerializeField] CharacterController characterController;
    [SerializeField] LayerMask obstacleMask;  // chọn layer Obstacle
    [SerializeField] float headRadius = 0.15f;

    Vector3 lastValidHeadPos;

    void Start()
    {
        lastValidHeadPos = head.position;
    }

    void LateUpdate()
    {
        bool blocked = Physics.CheckSphere(head.position, headRadius,
                         obstacleMask, QueryTriggerInteraction.Ignore);

        if (blocked)
        {
            Vector3 offset = lastValidHeadPos - head.position;
            offset.y = 0f;

            // Tắt CC tạm thời để việc dịch chuyển transform không bị ghi đè
            characterController.enabled = false;
            xrOrigin.position += offset;
            characterController.enabled = true;
        }
        else
        {
            lastValidHeadPos = head.position;
        }
    }
}