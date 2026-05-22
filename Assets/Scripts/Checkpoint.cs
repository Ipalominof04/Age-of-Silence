using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            GameManager.Instance.lastDoorID = checkpointID;
            GameManager.Instance.GuardarPartida();
            Debug.Log("Checkpoint guardado: " + checkpointID);
        }
    }
}
