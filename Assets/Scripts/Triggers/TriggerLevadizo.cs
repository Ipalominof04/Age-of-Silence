using UnityEngine;
public class TriggerLevadizo : MonoBehaviour
{
    [Header("Animacion placa")]
    [SerializeField] private Animator animatorPlaca; // animator placa de pressio (puja/baixa)
    [Header("Animacion puente")]
    [SerializeField] private Animator animator; // animator del pont llevadis (obert/tancat)
    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente; // collider que impedeix el pas quan el pont esta tancat
    // comptador compartit entre totes les instancies d'aquest script
    // si qualsevol de les dues plaques te alguna cosa a sobre, el pont roman obert
    static int objetosDentro = 0;
    // comptador propi de cada placa, perque cada una gestioni la seva propia animacio independentment de l'altra
    private int objetosEnEstaPlaca = 0;
    // L'OnTriggerEnter detecta quan el player, l'Aiden o una caixa movible entra a la placa i baixa el pont
    private void OnTriggerEnter(Collider other)
    {
        // nomes reacciona al player, l'Aiden o la caixa movible
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;
        objetosDentro++; // suma el comptador global, afectant ambdues plaques
        objetosEnEstaPlaca++; // suma el comptador local (nomes aquesta placa)
        animator.SetBool("Abierto", false); // baixa el pont
        bloqueoPuente.SetActive(false); // treu el bloqueig perque el player pugui passar
        animatorPlaca.SetBool("Bajar", true); // baixa la placa
    }
    // L'OnTriggerExit detecta quan alguna cosa surt de la placa i puja el pont si ambdues plaques estan buides
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;
        objetosDentro--; // resta el comptador global
        objetosEnEstaPlaca--; // resta el comptador local
        // la placa nomes puja quan ja no queda res a sobre d'AQUESTA placa
        if (objetosEnEstaPlaca <= 0)
        {
            animatorPlaca.SetBool("Bajar", false); // puja la placa (no hi ha pes)
        }
        // el pont nomes es tanca si AMBDUES plaques estan buides (comptador global a 0)
        // evita que el pont pugi si el player surt d'una placa pero l'Aiden segueix a l'altra
        if (objetosDentro <= 0)
        {
            animator.SetBool("Abierto", true); // tanca el pont
            bloqueoPuente.SetActive(true); // reactiva el bloqueig
        }
    }
}