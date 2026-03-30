using UnityEngine;

public class TriggerScared : MonoBehaviour
{
    [SerializeField] private GameEventString OnAidenScared;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Aiden"))
        {
            OnAidenScared?.Raise(ScareType.Body);
            //gameObject.SetActive(false);
        }
    }
}
