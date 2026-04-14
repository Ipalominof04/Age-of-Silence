using UnityEngine;
public class TriggerDropArma : MonoBehaviour
{
    [SerializeField] private ArmaController armaController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            armaController?.Soltar();
        }
    }
}