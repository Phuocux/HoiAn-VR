using UnityEngine;

public class LanternFloat : MonoBehaviour
{
    public Vector3 driftDirection = new Vector3(1f, 0f, 0.3f);
    public float driftSpeed = 0.3f;
    public float bobHeight = 0.03f;
    public float bobSpeed = 1.5f;
    public float swayAngle = 3f;

    Vector3 startPos;
    float offset;

    void Start()
    {
        startPos = transform.position;
        offset = Random.value * 10f;
    }

    void Update()
    {
        float t = Time.time + offset;
        Vector3 pos = startPos + driftDirection.normalized * driftSpeed * Time.time;
        pos.y = startPos.y + Mathf.Sin(t * bobSpeed) * bobHeight;
        transform.position = pos;

        float sway = Mathf.Sin(t * bobSpeed * 0.7f) * swayAngle;
        transform.rotation = Quaternion.Euler(sway, transform.eulerAngles.y, sway * 0.5f);
    }
}
