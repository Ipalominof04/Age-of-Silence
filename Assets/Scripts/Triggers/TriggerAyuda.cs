using UnityEngine;
using UnityEngine.UIElements;

public class TriggerAyuda : MonoBehaviour
{
    [Header("Que puzle es")]
    [SerializeField] private PuzleHelpType tipo;

    [Header("Dialogos")]
    [SerializeField] private RuntimeDialogueGraph graphSiAyuda;
    [SerializeField] private RuntimeDialogueGraph graphNoAyuda;

    [Header("Ayuda")]
    [SerializeField] private AidenController aiden;
    [SerializeField] private Transform puntoDestino;
    [SerializeField] private GameEvent onAyudaCompletada;
    [SerializeField] private GameEvent onDialogueEnded;

    [SerializeField] PlayerController player;

    [SerializeField] GameObject canvasworld;

    private void Awake()
    {
        if (canvasworld != null)
        {
            canvasworld.SetActive(false);
        }
    }

    public void PedirAyuda()
    {
        if (tipo == PuzleHelpType.DOOR)
        {
            if (GameManager.Instance.confianza >= 50)
            {
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                onDialogueEnded.Subscribe(HandleDialogoAcabado);
                this.gameObject.SetActive(false);
            }
            else
                DialogueManager.Instance.StartInfo(graphNoAyuda);
                GameManager.Instance.ResolverPuzzle("AidenNiega");
                this.gameObject.SetActive(false);
        }
        else if (tipo == PuzleHelpType.BRIDGE)
        {
            if (GameManager.Instance.confianza >= 80)
            {
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                onDialogueEnded.Subscribe(HandleDialogoAcabado);
                this.gameObject.SetActive(false);
            }
            else
                DialogueManager.Instance.StartInfo(graphNoAyuda);
        }
       
    }

    private void HandleDialogoAcabado()
    {
        onDialogueEnded.Unsubscribe(HandleDialogoAcabado);
        player.ClearTriggerAyuda();
        bool esperarEnDestino = (tipo == PuzleHelpType.BRIDGE);
        aiden.Help(puntoDestino.position, onAyudaCompletada, esperarEnDestino);
    }

    public void MostrarUI()
    {
        if(canvasworld != null)
        {
            canvasworld.SetActive(true);
        }
    }
    public void OcultarUI()
    {
        if (canvasworld != null)
        {
            canvasworld.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            MostrarUI();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            OcultarUI();
        }
    }
}