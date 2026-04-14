using UnityEngine;

public class TriggerPuente : MonoBehaviour
{
    [SerializeReference] private Animator animator;
    static int objetosDentro = 0; // static para que los dos triggers tengan un contador compartido

    private bool EsObjetoValido(Collider other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("Aiden") ||
               other.CompareTag("Movible");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EsObjetoValido(other))
        {
            objetosDentro++;
            animator.SetBool("Abierto", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (EsObjetoValido(other))
        {
            objetosDentro--;
            if (objetosDentro <= 0)
            {
                animator.SetBool("Abierto", true);
                objetosDentro = 0;
            }
        }
    }
}

