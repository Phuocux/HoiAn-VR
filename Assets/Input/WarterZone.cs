using UnityEngine;

public class WaterZone : MonoBehaviour
{
    public Vector3 flowDirection = new Vector3(1, 0, 0);
    public float flowSpeed = 0.5f;
    public Renderer surface;   // kéo object "Water" vào đây (ô Renderer)

    public Bounds Area => surface.bounds;
    public float SurfaceY => surface.bounds.center.y;
    public Vector3 Flow => flowDirection.normalized * flowSpeed;
}