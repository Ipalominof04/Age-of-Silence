using UnityEngine;

public class TriggerPuente : MonoBehaviour
{
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { }
            animator.SetBool("Abierto", false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            animator.SetBool("Abierto", true);
    }
}
