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
        if (!requiereObjeto)
        {
            ResolvePuzzle();
            return;
        }
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }

        if (objetoMano.nombreObjeto == objetoRequerido)
            ResolvePuzzle();
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        objetoMano.ClearObject();
    }

    private void ResolvePuzzle()
    {
        gameObject.SetActive(false);
        if (cadena != null)
            Destroy(cadena);
        DialogueManager.Instance.StartDialogue(graphDialogue);
        PlayerController.Instance.ClearTriggerPuzle();
    }

    public void OnEnterRange()
    {
        PlayerController.Instance.ForceIdle();
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            canvasworld.SetActive(false);
        }
    }
}
