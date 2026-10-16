using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LanternStall : MonoBehaviour
{
    public GameObject lanternPrefab;
    public Transform spawnPoint;
    public float respawnTime = 20f;
    public float leaveDistance = 0.3f;

    GameObject current;               // object cha (prefab gốc)
    Transform lanternBody;            // object con thật sự được cầm (HoaDangGiay)
    XRGrabInteractable currentGrab;
    Rigidbody currentRb;
    bool waiting;
    float timer;

    void Start()
    {
        Debug.Log("Sạp: Start chạy, respawnTime=" + respawnTime);
        SpawnLantern();
    }

    void SpawnLantern()
    {
        waiting = false;
        current = Instantiate(lanternPrefab, spawnPoint.position, spawnPoint.rotation);

        currentGrab = current.GetComponentInChildren<XRGrabInteractable>();
        currentRb = current.GetComponentInChildren<Rigidbody>();
        lanternBody = currentGrab != null ? currentGrab.transform : current.transform;

        if (currentGrab == null)
            Debug.LogWarning("Sạp: prefab đèn KHÔNG có XRGrabInteractable ở object nào");

        if (currentRb != null)
        {
            currentRb.isKinematic = true;
            currentRb.useGravity = true;
        }
    }

    void Update()
    {
        if (waiting)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                Debug.Log("Sạp: sinh đèn mới");
                SpawnLantern();
            }
            return;
        }

        bool destroyed = current == null || lanternBody == null;
        bool grabbed = currentGrab != null && currentGrab.isSelected;
        bool movedAway = !destroyed &&
            Vector3.Distance(lanternBody.position, spawnPoint.position) > leaveDistance;

        if (destroyed || grabbed || movedAway)
        {
            Debug.Log("Sạp: đèn đã bị lấy, hồi sau " + respawnTime + " giây");

            if (!destroyed && currentRb != null)
            {
                currentRb.isKinematic = false;
                currentRb.useGravity = true;
            }

            current = null;
            lanternBody = null;
            currentGrab = null;
            currentRb = null;
            waiting = true;
            timer = respawnTime;
        }
    }
}