using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CamaraTemblor : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float fuerza)
    {
        impulseSource.GenerateImpulse(fuerza);
    }

    public void ShakeContinuo(float duracion, float fuerza)
    {
        StartCoroutine(ShakeLoop(duracion, fuerza));
    }

    IEnumerator ShakeLoop(float duracion, float fuerza)
    {
        float t = 0f;
        while (t < duracion)
        {
            // Dirección aleatoria en X e Y en cada impulso
            Vector3 direccionAleatoria = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0f
            );
            impulseSource.GenerateImpulseWithVelocity(direccionAleatoria * fuerza);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
