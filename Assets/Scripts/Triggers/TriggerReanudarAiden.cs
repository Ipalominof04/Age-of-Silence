using UnityEngine;

public class TriggerReanudarAiden : MonoBehaviour
{
    [SerializeField] private GameEvent onAidenReanudar;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            onAidenReanudar.Raise();
            gameObject.SetActive(false);
        }
    }
}
