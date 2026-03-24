using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzle : MonoBehaviour
{
    [SerializeField] private string objetoRequerido;
    [SerializeField] private bool requiereObjeto = false;
    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;
    public RuntimeDialogueGraph grapSinObjeto;
    public RuntimeDialogueGraph graphObjetoIncorrecto;

    [Header("Acción al resolver el puzzle")]
    public UnityEvent OnPuzzleSolved;

    public void TryOpen(ObjetoAgarrado objetoMano)
    {
        if (!requiereObjeto)
        {
            OnPuzzleSolved?.Invoke();
            return;
        }

        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }
        if (objetoMano.nombreObjeto == objetoRequerido)
        {
            OnPuzzleSolved?.Invoke();
        }
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        objetoMano.UsarObjeto();
    }
}