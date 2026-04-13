using UnityEngine;

public class TriggerPuente : MonoBehaviour
{
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            animator.SetBool("Abierto", false);

        else if (other.CompareTag("Aiden"))        
            animator.SetBool("Abierto", false);

        else if (other.CompareTag("Movible"))
            animator.SetBool("Abierto", false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            animator.SetBool("Abierto", true);

        else if (other.CompareTag("Aiden"))
            animator.SetBool("Abierto", true);

        else if (other.CompareTag("Movible"))
            animator.SetBool("Abierto", true);
    }
}
