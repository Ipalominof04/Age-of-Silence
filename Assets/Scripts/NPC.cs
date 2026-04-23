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
        amenazaNPC.Subscribe(AñadirPuzleAmenaza);
        misionComidaNPC.Subscribe(AñadirPuzleComida);
    }

    private void OnDisable()
    {
        amenazaNPC.Unsubscribe(AñadirPuzleAmenaza);
        misionComidaNPC.Unsubscribe(AñadirPuzleComida);
    }
    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto)) return;

        string objeto = objetoMano.nombreObjeto;

        if (objeto == "ComidaNPC" && GameManager.Instance.PuzzleResuelto("MisionComidaNPC"))
        {
                    objetoMano.UsarObjeto();
                    this.gameObject.SetActive(false);
            if (graphComida != null)
                DialogueManager.Instance.StartInfo(graphComida);
        }
    }

    private void AñadirPuzleComida()
    {
        GameManager.Instance.datos.puzzlesResueltos.Add("MisionComidaNPC");
    }

    private void AñadirPuzleAmenaza()
    {
        GameManager.Instance.datos.puzzlesResueltos.Add("AmenazadoNPC");
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