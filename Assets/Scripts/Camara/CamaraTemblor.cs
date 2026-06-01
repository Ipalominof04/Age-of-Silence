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

    // inicia un shake continuo durant un temps determinat
    public void ShakeContinuo(float duracion, float fuerza)
    {
        StartCoroutine(ShakeLoop(duracion, fuerza));
    }

    //corrutina que genera petits impulsos aleatoris
    IEnumerator ShakeLoop(float duracion, float fuerza)
    {
        float t = 0f;
        while (t < duracion)
        {
            // calcula una direccio aleatoria als eixos X i Y
            Vector3 direccionAleatoria = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0f
            );
            // genera un impuls en la direccio aleatoria calculada
            impulseSource.GenerateImpulseWithVelocity(direccionAleatoria * fuerza);
            t += Time.deltaTime; // actualitza el temps transcorregut
            yield return null; // espera al seguent fotograma abans de generar un altre impuls
        }
    }
}
