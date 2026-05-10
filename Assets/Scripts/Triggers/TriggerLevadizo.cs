using UnityEngine;
public class TriggerLevadizo : MonoBehaviour
{
    [Header("Animacion placa")]
    [SerializeField] private Animator animatorPlaca;
    [Header("Animacion puente")]
    [SerializeField] private Animator animator;
    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente;
    static int objetosDentro = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;
        objetosDentro++;
        animator.SetBool("Abierto", false);
        bloqueoPuente.SetActive(false);
        animatorPlaca.SetBool("Bajar", true);
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
            animatorPlaca.SetBool("Bajar", false);
        }
    }
}