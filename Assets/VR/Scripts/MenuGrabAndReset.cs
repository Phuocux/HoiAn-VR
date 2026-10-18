using UnityEngine;


/// <summary>
/// Gắn lên GameObject gốc của menu, cùng Rigidbody và XRGrabInteractable.
/// Mỗi lần menu được bật, nó trở về vị trí trước camera và xóa vận tốc cũ.
/// Khi bị ném, menu không tự bám theo camera mà ở lại vị trí nó rơi xuống.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class MenuGrabAndReset : MonoBehaviour
{
    [Header("Camera người chơi")]
    [SerializeField] private Transform playerCamera;

    [Header("Vị trí menu khi mở")]
    [Min(0.1f)]
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float verticalOffset = -0.1f;
    [SerializeField] private bool faceCameraOnOpen = true;

    [Header("Vị trí bù theo camera")]
    [SerializeField] private float horizontalOffset = 0f;
    [SerializeField] private float verticalPositionOffset = 0f;

    [Header("Góc xoay bù khi mở menu")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 180f, 0f);
    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ResetInFrontOfCamera();
    }

    /// <summary>Cũng có thể gọi hàm này từ Button hoặc MenuFunctionManager.</summary>
    public void ResetInFrontOfCamera()
    {
        if (playerCamera == null)
        {
            Debug.LogError("[MenuGrabAndReset] Chưa gán Player Camera.", this);
            return;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;
        flatForward.Normalize();

        transform.position = playerCamera.position
        + flatForward * distanceFromCamera
        + playerCamera.right * horizontalOffset
        + playerCamera.up * verticalPositionOffset
        + Vector3.up * verticalOffset;

        if (faceCameraOnOpen)
        {
            Vector3 toCamera = playerCamera.position - transform.position;

            if (toCamera.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(toCamera.normalized, Vector3.up);

                transform.rotation =
                    lookRotation * Quaternion.Euler(rotationOffset);
            }
        }

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.Sleep();
        }
    }
}
