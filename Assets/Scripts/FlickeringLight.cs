using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    Light lt;
    public float minIntensity = 0f;
    public float maxIntensity = 1f;
    public float flickerSpeed = 10f;

    void Start() => lt = GetComponent<Light>();

    void Update()
    {
        lt.intensity = Mathf.Lerp(minIntensity, maxIntensity,
            Mathf.PerlinNoise(Time.time * flickerSpeed, 0f));
    }
}
