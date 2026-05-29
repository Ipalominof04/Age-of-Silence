using UnityEngine;

public class TriggerLevadizo : MonoBehaviour
{
    [Header("Animacion placa")]
    [SerializeField] private Animator animatorPlaca; //animator placa de presion (sube/baja)
    [Header("Animacion puente")]
    [SerializeField] private Animator animator; //animator del puente levadizo (abierto/cerrado)
    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente; // collider impide el paso cuando el puente esta cerrado

    //contador compartido entre todas las instancias de este script
    // si cualquiera de las dos placas tiene algo encima, el puente permanece abierto
    static int objetosDentro = 0;

    //contador propio de cada placa, para q cada una gestione su propia anim independientemente de la otra placa
    private int objetosEnEstaPlaca = 0; 

    private void OnTriggerEnter(Collider other)
    {
        // solo reacciona al player, aiden o la caja movible
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;

        objetosDentro++; // suma el cont global, afectando a ambas placas
        objetosEnEstaPlaca++; // suma el cont local (solo esta placa)

        animator.SetBool("Abierto", false);// baja el puente
        bloqueoPuente.SetActive(false); // quita el bloqueo para q el player pueda pasar
        animatorPlaca.SetBool("Bajar", true); // baja la placa
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("PlayerCollider") && !other.CompareTag("Aiden") && !other.CompareTag("Movible"))
            return;

        objetosDentro--; // resetea al cont global
        objetosEnEstaPlaca--; // resetea al cont local

        // la placa solo sube cuando ya no queda nada encima de ESTA placa
        if (objetosEnEstaPlaca <= 0)
        {
            animatorPlaca.SetBool("Bajar", false); // sube la placa (no hay peso)
        }
        // el puente solo se cierra si AMBAS placas estan vacias (cont global al 0)
        // evita puente suba si player sale de una placa pero Aiden sigue en la otra
        if (objetosDentro <= 0)
        {
            animator.SetBool("Abierto", true); // abre el puente
            bloqueoPuente.SetActive(true); // reactiva el bloqueo
        }
    }
}