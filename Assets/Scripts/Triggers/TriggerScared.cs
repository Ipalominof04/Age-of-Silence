using UnityEngine;

public class TriggerScared : MonoBehaviour
{
    [SerializeField] private GameEventScareType OnAidenScared;

    private void OnTriggerEnter(Collider other)
    {
        // Comprovem que l'objecte que ha entrat pertany a la capa d'Aiden.
        if (other.gameObject.layer == LayerMask.NameToLayer("Aiden"))
        {
            // Llanquem l'event indicant el tipus d'ensurt.
            OnAidenScared?.Raise(ScareType.Body);

            // Desactivem el trigger perque nomes s'activi una vegada.
            gameObject.SetActive(false);
        }
    }
}