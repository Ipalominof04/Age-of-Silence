using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzleNevera : MonoBehaviour, IPuzle
{
    [SerializeField] private string objetoRequerido;
    [SerializeField] private bool requiereObjeto = false;

    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;
    public RuntimeDialogueGraph grapSinObjeto;
    public RuntimeDialogueGraph graphObjetoIncorrecto;
    public RuntimeDialogueGraph graphDialogue;

    [Header("Meshes")]
    [SerializeField] private GameObject cadena;

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        // Si el puzle no necessita cap objecte, es resol directament.
        if (!requiereObjeto)
        {
            ResolvePuzzle();
            return;
        }

        // Mostrem un missatge si el jugador no porta res.
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }

        // Comprovem si l'objecte es el correcte.
        if (objetoMano.nombreObjeto == objetoRequerido)
            ResolvePuzzle();
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        objetoMano.ClearObject();
    }

    private void ResolvePuzzle()
    {
        // Desactivem el trigger i eliminem la cadena que bloqueja la nevera.
        gameObject.SetActive(false);

        if (cadena != null)
            cadena.SetActive(false);

        // Iniciem el dialeg associat al puzle resolt.
        DialogueManager.Instance.StartDialogue(graphDialogue);

        PlayerController.Instance.ClearTriggerPuzle();
    }

    public void OnEnterRange()
    {
        // Forcem l'estat idle del jugador en apropar-se al puzle.
        PlayerController.Instance.ForceIdle();

        // Mostrem la informacio inicial nomes una vegada.
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);

            canvasworld.SetActive(false);
        }
    }
}