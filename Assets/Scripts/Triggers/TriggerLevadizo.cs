using UnityEngine;

public class TriggerLevadizo : MonoBehaviour
{
    [Header("Animacion placa")]
    [SerializeField] private Animator animatorPlaca;
    [Header("Animacion puente")]
    [SerializeField] private Animator animator;
    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente;

    //compartido, para que se mantenga abierto el puente si hay algo en alguna placa
    static int objetosDentro = 0;
    //contador propio de cada placa, pq este script lo tienen ambas placas,
    //el static cuenta para ambos como un solo contador, este cuenta indivualmente para cada uno
    private int objetosEnEstaPlaca = 0; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;

        objetosDentro++;
        objetosEnEstaPlaca++;

        animator.SetBool("Abierto", false);
        bloqueoPuente.SetActive(false);
        //baja la placa
        animatorPlaca.SetBool("Bajar", true); 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;

        objetosDentro--;
        objetosEnEstaPlaca--;

        if (objetosEnEstaPlaca <= 0)
        {
            //sube la placa
            animatorPlaca.SetBool("Bajar", false); 
        }

        if (objetosDentro <= 0)
        {
            //el puente solo sube si no hay nadie en ninguna placa
            animator.SetBool("Abierto", true); 
            bloqueoPuente.SetActive(true);
        }
    }
}