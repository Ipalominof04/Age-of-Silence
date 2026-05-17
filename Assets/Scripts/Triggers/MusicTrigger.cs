using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("Evento")]
    [SerializeField] private GameEventString musicEvent;

    [SerializeField] private string musicName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            musicEvent.Raise(musicName);
            this.gameObject.SetActive(false);
        }
    }
}
