using UnityEngine;

public class TriggerReanudarAiden : MonoBehaviour
{
    [SerializeField] private GameEvent onAidenReanudar;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            onAidenReanudar.Raise();
            gameObject.SetActive(false);
        }
    }
}
