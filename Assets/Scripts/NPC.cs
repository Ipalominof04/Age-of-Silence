using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] GameEvent misionComidaNPC;
    [SerializeField] GameEvent amenazaNPC;

    [Header("Dialogos")]
    [SerializeField] private RuntimeDialogueGraph graphComida;

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

    void Start() { }
    void Update() { }

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto)) return;

        string objeto = objetoMano.nombreObjeto;

        if (objeto == "Comida" && GameManager.Instance.PuzzleResuelto("MisionComidaNPC"))
        {
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

    private void OnTriggerEnter(Collider other) { }
}