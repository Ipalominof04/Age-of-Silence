using UnityEngine;

public class TriggerPuzleComida : MonoBehaviour, IPuzle
{
    // Controla que la informacio nomes es mostri una vegada.
    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Meshes")]
    [SerializeField] private GameObject meshComida;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        // Amaguem el mesh del objecte de la comida.
        meshComida.SetActive(false);

        // Guardem l'objecte a la ma
        objetoMano.SaveObject(this.gameObject);

        // Avisem el GameManager i l'afegim a la llista de puzzles resolts.
        GameManager.Instance.ResolverPuzzle("ComidaNPC");
    }

    public void OnEnterRange()
    {
        // Si encara no s'ha mostrat la informacio i hi ha un dialeg assignat,
        // l'iniciem i marquem que ja s'ha ensenyat.
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
        }
    }
}