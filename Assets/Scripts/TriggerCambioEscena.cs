using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerCambioEscena : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] string nombreEscena;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            print("entro");
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}
