using UnityEngine;

public class ShakeTrigger : MonoBehaviour
{
    [SerializeField] private CamaraTemblor cameraShake;
    [SerializeField] private float duracion = 3f;
    [SerializeField] private float fuerza = 0.5f;

    private bool triggered = false;

    public void OnTriggerEnter(Collider other)
    {
        print("entro");
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider") && !triggered)
        {
            print("entro2");
            triggered = true;
            cameraShake.ShakeContinuo(duracion, fuerza);
        }
    }
}
