using UnityEngine;

public class TriggerDropArma : MonoBehaviour
{
    // Referencia al controlador de l'arma que volem deixar anar.
    [SerializeField] private ArmaController armaController;

    private void OnTriggerEnter(Collider other)
    {
        // Comprovem si l'objecte que ha entrat al trigger es el jugador.
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            // Si hi ha una arma assignada, la deixem anar.
            armaController?.Soltar();
        }
    }
}