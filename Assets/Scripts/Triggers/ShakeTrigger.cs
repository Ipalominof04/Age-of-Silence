using UnityEngine;
public class ShakeTrigger : MonoBehaviour
{
    [SerializeField] private CamaraTemblor cameraShake;
    [SerializeField] private float duracion = 3f;
    [SerializeField] private float fuerza = 0.5f;
    private bool triggered = false; // evita que el shake es repeteixi si el player surt i torna a entrar
    // L'OnTriggerEnter activa el tremolor de camera quan el player entra al trigger, nomes una vegada
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider") && !triggered)
        {;
            triggered = true;
            // truquem a la funcio de ShakeContiuo on passem per parametre una duracio y una forca
            cameraShake.ShakeContinuo(duracion, fuerza);
        }
    }
}