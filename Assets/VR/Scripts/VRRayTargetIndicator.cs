using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRRayTargetIndicator : MonoBehaviour
{
    [Header("XR Ray")]
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("Target Indicator")]
    [SerializeField] private Transform targetIndicator;

    [Header("Surface Offset")]
    [SerializeField] private float surfaceOffset = 0.005f;

    [Header("Indicator Size")]
    [SerializeField] private float indicatorSize = 0.025f;

    private void Awake()
    {
        if (targetIndicator != null)
        {
            targetIndicator.localScale =
                Vector3.one * indicatorSize;

            targetIndicator.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (rayInteractor == null || targetIndicator == null)
            return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            ShowIndicator(hit);
        }
        else
        {
            HideIndicator();
        }
    }

    private void ShowIndicator(RaycastHit hit)
    {
        targetIndicator.gameObject.SetActive(true);

        // Đặt chấm đúng tại điểm ray chạm vật thể.
        targetIndicator.position =
            hit.point + hit.normal * surfaceOffset;

        // Cho chấm quay ra khỏi bề mặt,
        // đồng thời hướng về phía người chơi.
        targetIndicator.rotation =
            Quaternion.LookRotation(hit.normal);

        targetIndicator.localScale =
            Vector3.one * indicatorSize;
    }

    private void HideIndicator()
    {
        targetIndicator.gameObject.SetActive(false);
    }
}