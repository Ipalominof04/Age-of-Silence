using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TriggerAyuda : MonoBehaviour
{
    [Header("Que puzle es")]
    [SerializeField] private PuzleHelpType tipo; // enum identifica a que puzzle pertenece este trigger
    [Header("Dialogos")]
    [SerializeField] private RuntimeDialogueGraph graphSiAyuda; // dialogo cuando Aiden acepta ayudar
    [SerializeField] private RuntimeDialogueGraph graphNoAyuda; // dialogo cuando Aiden se niega
    [Header("Ayuda")]
    [SerializeField] private Transform puntoDestino; // pos a la que Aiden se desplaza para ayudar
    [SerializeField] private GameEvent onAyudaCompletada; // evento que se lanza cuando el player toca la otra placa
    [SerializeField] private GameEvent onDialogueEnded; // evento que se lanza cuando el dialogo de aceptacion termina
    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;  
    [SerializeField] TextMeshProUGUI texto; // texto flotante sobre el trigger
    [SerializeField] Animator textoAnimator; // animator del texto cuando muestra "!"
    [SerializeField] private float fontSizeExclamacion; // tamano del texto en estado idle "!"
    [SerializeField] private float fontSizeInteract; // tamano del texto al estar en rango "pulsa E"

    private void Awake()
    {
        // estado inicial: muestra "!" animado para indicar que hay algo de interes
        SetUI("!", fontSizeExclamacion, true);
    }

    // Llamado desde PlayerController cuando el player pulsa E estando en rango
    public void PedirAyuda()
    {
        // desregistra este trigger del PlayerController para q no se pueda volver a interactuar
        PlayerController.Instance.ClearTriggerAyuda();

        if (tipo == PuzleHelpType.DOOR)
        {
            if (GameManager.Instance.confianza >= 25)
            {
                // lanza el dialogo de aceptacion y espera a que termine
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                onDialogueEnded.Subscribe(HandleDialogoAcabado); // se suscribe para saber cuando acaba el dialogo 
            }
            else
            {
                // Aiden se niega y se registra que el puzzle fue rechazado
                DialogueManager.Instance.StartInfo(graphNoAyuda);
                GameManager.Instance.ResolverPuzzle("AidenNiega");
            }
        }
        else if (tipo == PuzleHelpType.BRIDGE)
        {
            if (GameManager.Instance.confianza >= 40)
            {
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                onDialogueEnded.Subscribe(HandleDialogoAcabado);
            }
            else
            {
                // en el puente no hay consecuencia de puzzle al negarse, solo dialogo
                DialogueManager.Instance.StartInfo(graphNoAyuda);
            }
        }
        // desactiva el trigger en todos los casos para q no sea interactuable
        this.gameObject.SetActive(false);
    }

    // Callback que se ejecuta cuando el dialogo de aceptacion termina
    private void HandleDialogoAcabado()
    {
        // Se desuscribe para evitar que se llame mas veces en el futuro
        onDialogueEnded.Unsubscribe(HandleDialogoAcabado);
        // el puente requiere que Aiden espere en el destino hasta que el player cruce
        // La puerta no, Aiden solo va, actua y sigue
        bool esperarEnDestino = (tipo == PuzleHelpType.BRIDGE);
        // ordena a Aiden que se mueva al punto de destino y ejecute la ayuda
        AidenController.Instance.Help(puntoDestino.position, onAyudaCompletada, esperarEnDestino);
    }

    // Metodo reutilizable para cambiar el estado visual del canvas flotante
    private void SetUI(string texto, float size, bool animar)
    {
        if (canvasworld != null)
        {
            this.texto.text = texto;
            this.texto.fontSize = size;
            textoAnimator.enabled = animar; 
        }
    }

    // cuando player entra en rango cambia el texto para indicar q puede interactuar
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("Pulsa E para pedir ayuda a Aiden", fontSizeInteract, false);
        }
    }
    // cuando el player sale del rango vuelve al estado idle con la "!" animada
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("!", fontSizeExclamacion, true);
        }
    }
}