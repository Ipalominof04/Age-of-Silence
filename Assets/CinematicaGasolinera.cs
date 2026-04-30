using UnityEngine;

public class CinematicaGasolinera : MonoBehaviour
{

    [SerializeField] private Transform[] puntos; // puntos por donde pasa el pj
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private Animator animator;

    private int puntoActual = 0;
    private bool activo = false;

    public void Iniciar()
    {
        activo = true;
    }

    void Update()
    {
        if (!activo || puntoActual >= puntos.Length) return;

        // Mueve hacia el siguiente punto
        Transform objetivo = puntos[puntoActual];
        transform.position = Vector3.MoveTowards(transform.position, objetivo.position, velocidad * Time.deltaTime);
        animator.Play("run");

        // Cuando llega al punto pasa al siguiente
        if (Vector3.Distance(transform.position, objetivo.position) < 0.1f)
        {
            puntoActual++;
            if (puntoActual >= puntos.Length)
            {
                animator.Play("idle");
                activo = false;
            }
        }
    }

}
