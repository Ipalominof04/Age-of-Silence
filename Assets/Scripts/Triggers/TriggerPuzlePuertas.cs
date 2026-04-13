using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzlePuertas : MonoBehaviour, IPuzle
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

    [SerializeField] public PlayerController player;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (!requiereObjeto)
        {
            OnPuzzleSolved?.Invoke();
            gameObject.SetActive(false);
            player.ClearTriggerPuzle();

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
            gameObject.SetActive(false);
            player.ClearTriggerPuzle();


        }
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        objetoMano.UsarObjeto();
    }

    public void OnEnterRange()
    {
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
        }
    }
}