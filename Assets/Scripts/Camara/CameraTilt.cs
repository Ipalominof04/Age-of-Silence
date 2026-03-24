using Unity.Cinemachine;
using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    public Transform player;
    public float minDistance = 3f; // distancia minima -> angulo bajo
    public float maxDistance = 10f;  // distancia maxima -> angulo alto
    public float minAngle = 5f;      // angulo cuando está cerca (mas abajo)
    public float maxAngle = 20f;     // angulo cuando está lejos (mas arriba)
    public float minOffsetY = 0f;    // offset Y cuando está cerca
    public float maxOffsetY = 2f;    // offset Y cuando está lejos
    public float smoothSpeed = 2f;   // suavidad del cambio

    private CinemachineCamera vcam;
    private CinemachinePositionComposer composer;

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        composer = GetComponent<CinemachinePositionComposer>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Ángulo
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, t);
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = Mathf.LerpAngle(currentRotation.x, targetAngle, Time.deltaTime * smoothSpeed);
        transform.eulerAngles = currentRotation;

        // Target Offset Y
        if (composer != null)
        {
            float targetOffsetY = Mathf.Lerp(minOffsetY, maxOffsetY, t);
            Vector3 offset = composer.TargetOffset;
            offset.y = Mathf.Lerp(offset.y, targetOffsetY, Time.deltaTime * smoothSpeed);
            composer.TargetOffset = offset;
        }
    }
}
