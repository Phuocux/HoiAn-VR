using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class LanternFloat : MonoBehaviour
{
    public float bobHeight = 0.02f;
    public float bobSpeed = 2f;
    public float floatOffset = 0.0f;

    Rigidbody rb;
    XRGrabInteractable grab;
    WaterZone water;
    WaterZone[] zones;
    float seed;
    Vector3 respawnPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        seed = Random.value * 10f;
    }

    void OnEnable()
    {
        if (grab != null) grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        if (grab != null) grab.selectExited.RemoveListener(OnReleased);
    }

    void Start()
    {
        zones = FindObjectsByType<WaterZone>(FindObjectsSortMode.None);
        respawnPos = transform.position;
    }

    // Thả tay: nếu chưa ở nước thì cho rơi tự do và vẫn nhặt lại được
    void OnReleased(SelectExitEventArgs args)
    {
        if (water != null) return;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    void Update()
    {
        // Đang cầm trên tay thì không làm gì
        if (grab != null && grab.isSelected) return;

        if (water == null)
        {
            // Rơi xuyên nền thì đưa về chỗ cũ
            if (transform.position.y < -20f)
            {
                transform.position = respawnPos;
                if (!rb.isKinematic) rb.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 p = transform.position;
            foreach (var z in zones)
            {
                if (z.surface == null) continue;
                Bounds b = z.Area;
                bool inXZ = p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;
                bool atSurface = p.y <= z.SurfaceY + 0.1f && p.y >= z.SurfaceY - 2f;
                if (inXZ && atSurface)
                {
                    water = z;
                    rb.useGravity = false;
                    rb.isKinematic = true;
                    Debug.Log("Đèn đã vào nước, bắt đầu trôi");
                    break;
                }
            }
            return;
        }

        // Đã ở nước: trôi và nhấp nhô
        Vector3 pos = transform.position;
        pos += water.Flow * Time.deltaTime;
        float bob = Mathf.Sin(Time.time * bobSpeed + seed) * bobHeight;
        pos.y = Mathf.Lerp(pos.y, water.SurfaceY + floatOffset + bob, Time.deltaTime * 5f);
        transform.position = pos;

        Quaternion target = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 4f);
    }
}