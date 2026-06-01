using UnityEngine;

public class TriggerReanudarAiden : MonoBehaviour
{
    [SerializeField] private GameEvent onAidenReanudar;

    private void OnTriggerEnter(Collider other)
    {
        // Comprovem que l'objecte que entra sigui el jugador.
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            // Executem l'event per reprendre el comportament de l'Aiden.
            onAidenReanudar.Raise();

            // Desactivem el trigger per evitar noves activacions.
            gameObject.SetActive(false);
        }
    }
}
