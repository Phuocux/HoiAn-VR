
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FallTriggerZone : MonoBehaviour
{
    [Header("Xe cần kích hoạt")]
    [SerializeField] private BikeMovement targetBike;

    [Header("Collider cần tắt")]
    [SerializeField] private BoxCollider bikeColliderToDisable;

    [Header("Cài đặt")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool activated;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ xử lý khi xe đi vào vùng.
        BikeMovement enteringBike =
            other.GetComponentInParent<BikeMovement>();

        if (enteringBike == null)
            return;

        // Nếu đã chỉ định xe, bỏ qua các xe khác.
        if (targetBike != null && enteringBike != targetBike)
            return;

        if (triggerOnlyOnce && activated)
            return;

        if (bikeColliderToDisable == null)
        {
            Debug.LogWarning(
                "FallTriggerZone: Chua gan BoxCollider cua xe.",
                this);
            return;
        }

        // Tắt collider vật lý của xe để xe rơi.
        activated = true;
        bikeColliderToDisable.enabled = false;
    }
}
