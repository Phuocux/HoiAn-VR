using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    public Light candleLight;
    public float baseIntensity = 2f;
    public float flickerAmount = 0.4f;
    public float speed = 3f;

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        candleLight.intensity = baseIntensity + (noise - 0.5f) * flickerAmount * 2f;
    }
}