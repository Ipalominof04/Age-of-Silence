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

    [Header("Cambio de escena")]
    [SerializeField] private string nombreEscena;
    [SerializeField] private string targetDoorID;

    [SerializeField] public PlayerController player;


    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

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
        {;
            ResolvePuzzle();
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
    private void ResolvePuzzle()
    {
        gameObject.SetActive(false);
        player.ClearTriggerPuzle();

        if (!string.IsNullOrEmpty(nombreEscena))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}