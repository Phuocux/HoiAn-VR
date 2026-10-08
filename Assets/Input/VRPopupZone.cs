using UnityEngine;

public class VRPopupZone : MonoBehaviour
{
    public GameObject popup;
    public GameObject dimSphere;
    public Transform playerCamera;
    public float distance = 1.0f;
    public float heightOffset = 0f;
    public float rotateSpeed = 8f;

    Renderer cubeRenderer;
    bool isInside;

    bool IsPlayer(Collider other) =>
        other.CompareTag("Player") || other.CompareTag("MainCamera");

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();   // Mesh Renderer của chính Cube
        popup.SetActive(false);
        if (dimSphere) dimSphere.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || isInside) return;
        isInside = true;

        if (cubeRenderer) cubeRenderer.enabled = false;   // ẩn Cube khi chạm

        Vector3 fwd = playerCamera.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 pos = playerCamera.position + fwd * distance;
        pos.y = playerCamera.position.y + heightOffset;
        popup.transform.position = pos;

        popup.SetActive(true);
        FaceCamera(true);

        if (dimSphere)
        {
            dimSphere.transform.position = playerCamera.position;
            dimSphere.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        isInside = false;

        if (cubeRenderer) cubeRenderer.enabled = true;    // hiện lại Cube khi ra ngoài

        popup.SetActive(false);
        if (dimSphere) dimSphere.SetActive(false);
    }

    void LateUpdate()
    {
        if (!isInside) return;
        if (dimSphere) dimSphere.transform.position = playerCamera.position;
        FaceCamera(false);
    }

    void FaceCamera(bool instant)
    {
        Vector3 dir = popup.transform.position - playerCamera.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        popup.transform.rotation = (instant || rotateSpeed <= 0f)
            ? target
            : Quaternion.Slerp(popup.transform.rotation, target, rotateSpeed * Time.deltaTime);
    }
}