using UnityEngine;

public class VRPopupZone : MonoBehaviour
{
    public GameObject popup;          // Canvas ảnh (World Space)
    public Transform playerCamera;    // Main Camera
    public float distance = 1.5f;     // khoảng cách trước mặt
    public float heightOffset = 0f;   // lệch cao so với tầm mắt (vd -0.1)

    public bool followPosition = true; // true: ảnh đi theo người chơi
    public float followSpeed = 4f;     // độ mượt khi đi theo
    public float rotateSpeed = 8f;     // độ mượt khi xoay

    bool IsPlayer(Collider other) => other.CompareTag("Player");

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        popup.SetActive(true);
        Place(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other)) popup.SetActive(false);
    }

    void LateUpdate()
    {
        if (popup.activeSelf) Place(false);
    }

    void Place(bool instant)
    {
        Transform t = popup.transform;
        Vector3 pos = t.position;

        // Vị trí: trước mặt người chơi, chỉ theo hướng ngang
        if (instant || followPosition)
        {
            Vector3 fwd = playerCamera.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = t.forward; // phòng khi nhìn thẳng lên/xuống
            fwd.Normalize();

            Vector3 target = playerCamera.position + fwd * distance;
            target.y = playerCamera.position.y + heightOffset;

            pos = instant ? target : Vector3.Lerp(t.position, target, followSpeed * Time.deltaTime);
        }

        // Xoay: quay mặt về người chơi, giữ thẳng đứng (bỏ thành phần Y)
        Vector3 dir = pos - playerCamera.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) { t.position = pos; return; }

        Quaternion rot = Quaternion.LookRotation(dir);
        if (!instant) rot = Quaternion.Slerp(t.rotation, rot, rotateSpeed * Time.deltaTime);

        t.SetPositionAndRotation(pos, rot);
    }
}