using UnityEngine;

public class TriggerLevadizo : MonoBehaviour
{
    [Header("Animacion puente")]
    [SerializeField] private Animator animator;

    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente;
    private static int objetosDentro = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible")) 
            return;

        objetosDentro++;
        animator.SetBool("Abierto", false);
        bloqueoPuente.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible")) 
            return;

        objetosDentro--;
        if (objetosDentro <= 0)
        {
            animator.SetBool("Abierto", true);
            bloqueoPuente.SetActive(true);
        }

    }
}