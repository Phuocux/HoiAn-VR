using UnityEngine;
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
    Quaternion floatRotation;
    float seed;

    void Awake()
    {
        floatRotation = Quaternion.identity;
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        seed = Random.value * 10f;
    }

    void Start()
    {
        zones = FindObjectsByType<WaterZone>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // Đang cầm trên tay thì không làm gì
        if (grab != null && grab.isSelected) return;
        if (water == null && grab != null && !grab.enabled)
        {
            grab.enabled = true;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (water == null)
        {
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
                    if (grab != null) grab.enabled = false;
                    Debug.Log("Đèn đã vào nước, bắt đầu trôi");
                    break;
                }
            }
            return;
        }

        Vector3 pos = transform.position;
        pos += water.Flow * Time.deltaTime;
        float bob = Mathf.Sin(Time.time * bobSpeed + seed) * bobHeight;
        pos.y = Mathf.Lerp(pos.y, water.SurfaceY + floatOffset + bob, Time.deltaTime * 5f);
        transform.position = pos;
        Quaternion target = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 4f);
    }
}