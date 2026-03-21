using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerCambioEscena : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] string nombreEscena;

    [Header("Spawn")]
    [SerializeField] string miDoorID;      // ID de este trigger
    [SerializeField] string targetDoorID;  // ID del SpawnPoint destino en la otra escena


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}
