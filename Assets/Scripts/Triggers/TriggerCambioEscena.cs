using UnityEngine;
public class TriggerCambioEscena : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] string nombreEscena;
    [Header("Spawn")]
    [SerializeField] string miDoorID;
    [SerializeField] string targetDoorID;

    [Header("Requiere puzzle")]
    [SerializeField] private GameEvent onPuzzleResuelto;

    private void OnEnable()
    {
        if (onPuzzleResuelto != null)
        {
            gameObject.SetActive(false);
            onPuzzleResuelto.Subscribe(Activar);
        }
    }

    private void OnDisable()
    {
        if (onPuzzleResuelto != null)
            onPuzzleResuelto.Unsubscribe(Activar);
    }

    private void Activar()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerCambioEscena: " + other.gameObject.name + " layer: " + other.gameObject.layer);
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}
