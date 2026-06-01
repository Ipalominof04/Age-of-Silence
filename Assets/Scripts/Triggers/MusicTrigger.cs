using UnityEngine;
public class MusicTrigger : MonoBehaviour
{
    [Header("Evento")]
    [SerializeField] private GameEventString musicEvent;
    [SerializeField] private string musicName;
    // L'OnTriggerEnter llanca l'event de musica quan el player entra al trigger i desactiva l'objecte perque no es repeteixi
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            musicEvent.Raise(musicName);
            this.gameObject.SetActive(false);
        }
    }
}