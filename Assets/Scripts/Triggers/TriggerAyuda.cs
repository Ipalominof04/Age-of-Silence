using TMPro;
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

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;
    [SerializeField] TextMeshProUGUI texto;
    [SerializeField] Animator textoAnimator;
    [SerializeField] private float fontSizeExclamacion;
    [SerializeField] private float fontSizeInteract;

    [Header("Clear Trigger")]
    [SerializeField] PlayerController player;

    private void Awake()
    {
        SetUI("!", fontSizeExclamacion, true);
    }

    public void PedirAyuda()
    {
        player.ClearTriggerAyuda();

        if (tipo == PuzleHelpType.DOOR)
        {
            if (GameManager.Instance.confianza >= 50)
            {
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                onDialogueEnded.Subscribe(HandleDialogoAcabado);
                this.gameObject.SetActive(false);
            }
            else
            {
                DialogueManager.Instance.StartInfo(graphNoAyuda);
                GameManager.Instance.ResolverPuzzle("AidenNiega");
                this.gameObject.SetActive(false);
            }
               
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
            {
                DialogueManager.Instance.StartInfo(graphNoAyuda);
                this.gameObject.SetActive(false);
            }
                
        }
       
    }

    private void HandleDialogoAcabado()
    {
        onDialogueEnded.Unsubscribe(HandleDialogoAcabado);
        bool esperarEnDestino = (tipo == PuzleHelpType.BRIDGE);
        aiden.Help(puntoDestino.position, onAyudaCompletada, esperarEnDestino);
    }

    private void SetUI(string texto, float size, bool animar)
    {
        if (canvasworld != null)
        {
            this.texto.text = texto;
            this.texto.fontSize = size;
            textoAnimator.enabled = animar;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("Pulsa E para pedir ayuda a Aiden", fontSizeInteract, false);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("!", fontSizeExclamacion, true);
        }
    }
}