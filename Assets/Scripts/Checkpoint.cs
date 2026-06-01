using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID; // id

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            GameManager.Instance.lastDoorID = checkpointID; // guarda id al GM
            GameManager.Instance.GuardarPartida(); // crida a la funcio per guardar la partida
        }
    }
}
