using UnityEngine;

public class NPC : MonoBehaviour, IPuzle
{
    public bool infoMostrada = false;

    [SerializeField] GameEvent misionComidaNPC;
    [SerializeField] GameEvent amenazaNPC;

    [Header("Dialogos")]
    [SerializeField] private RuntimeDialogueGraph graphComida;
    [SerializeField] private RuntimeDialogueGraph graphTraigoComida;

    private void OnEnable()
    {
        amenazaNPC.Subscribe(AnadirPuzleAmenaza);
        misionComidaNPC.Subscribe(AnadirPuzleComida);
    }

    private void OnDisable()
    {
        amenazaNPC.Unsubscribe(AnadirPuzleAmenaza);
        misionComidaNPC.Unsubscribe(AnadirPuzleComida);
    }
    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto)) return;
        string objeto = objetoMano.nombreObjeto;

        if (objeto == "ComidaNPC" && GameManager.Instance.PuzzleResuelto("ComidaNPC"))
        {
            objetoMano.UsarObjeto();
            this.gameObject.SetActive(false);
            if (graphComida != null)
                DialogueManager.Instance.StartInfo(graphComida);

            GameManager.Instance.ResolverPuzzle("PuzleNPC");
        }
    }

    private void AnadirPuzleComida()
    {
        GameManager.Instance.ResolverPuzzle("MisionComidaNPC");
    }

    private void AnadirPuzleAmenaza()
    {
        GameManager.Instance.ResolverPuzzle("PuzleNPC");
    }

    public void OnEnterRange()
    {
        if (!infoMostrada && graphTraigoComida != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphTraigoComida);
        }
    }
}