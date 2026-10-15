using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LanternStall : MonoBehaviour
{
    public GameObject lanternPrefab;   // prefab đèn hoa đăng
    public Transform spawnPoint;       // vị trí đèn đặt trên sạp
    public float respawnTime = 20f;    // thời gian hồi
    public float range = 4f;           // khoảng cách để bấm L (chỉ để test)
    public float leaveDistance = 0.3f;

    GameObject current;                // đèn đang nằm trên sạp

    void Start()
    {
        SpawnLantern();
    }

    void SpawnLantern()
    {
        current = Instantiate(lanternPrefab, spawnPoint.position, spawnPoint.rotation);

        // Giữ đèn đứng yên trên bàn cho tới khi được cầm
        var rb = current.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Khi tay VR nắm đèn: thả cho vật lý hoạt động và bắt đầu đếm hồi
        var grab = current.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            GameObject lantern = current;
            grab.selectEntered.AddListener(_ =>
            {
                if (lantern == current) OnLanternTaken();
            });
        }
    }

    void Update()
    {
        // Phím L chỉ dùng để test trên máy: "lấy" đèn ngay tại chỗ
        var kb = Keyboard.current;
        if (kb == null || current == null || Camera.main == null) return;

        if (kb.lKey.wasPressedThisFrame)
        {
            float d = Vector3.Distance(Camera.main.transform.position, spawnPoint.position);
            if (d < range) OnLanternTaken();
        }
    }  

    void LateUpdate()
    {
        if (current == null) return;

        // đèn bị xóa/biến mất, hoặc bị đưa ra khỏi bàn
        if (!current || Vector3.Distance(current.transform.position, spawnPoint.position) > leaveDistance)
            OnLanternTaken();
    }
    void OnLanternTaken()
    {
        if (!current) { current = null; StartCoroutine(RespawnRoutine()); return; }
        if (current == null) return;

        // Cho đèn rơi/di chuyển bình thường (không dịch chuyển đi đâu)
        var rb = current.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        current = null;                 // sạp hết đèn
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnLantern();
    }
}