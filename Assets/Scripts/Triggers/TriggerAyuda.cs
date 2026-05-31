using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private Transform puntoDestino; 
    [SerializeField] private GameEvent onAyudaCompletada; 
    [SerializeField] private GameEvent onDialogueEnded; 
    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;  
    [SerializeField] TextMeshProUGUI texto;
    [SerializeField] Animator textoAnimator; 
    [SerializeField] private float fontSizeExclamacion; 
    [SerializeField] private float fontSizeInteract; 

    private void Awake()
    {
        //estat inicial: mostra "!" animat per a indicar que hi ha una interaccio disponible
        SetUI("!", fontSizeExclamacion, true);
    }

    //es crida desde PlayerControlles quan el player interacciona dins del rang d'ajuda
    public void PedirAyuda()
    {
        //fa un clear del trigger al PlayerController per a que no pugui tornar a interactuar i no es buguegi per no netejar la referencia
        PlayerController.Instance.ClearTriggerAyuda();

        //tipos de puzle d'ajuda DOOR
        if (tipo == PuzleHelpType.DOOR)
        {
            //si el jugador te mes de 25 de confianca aiden acceptara
            if (GameManager.Instance.confianza >= 25)
            {
                //llanca el dialeg de acceptacio i espera a que s'acabi
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                //es suscriu per saber quan s'acaba el dialeg
                onDialogueEnded.Subscribe(HandleDialogoAcabado); 
            }
            else
            {
                //aiden es nega i es registra el puzle de AidenNiega per poder actiar un objecte necessari per a completar
                //completar el puzle sense l'ajuda d'aiden
                DialogueManager.Instance.StartInfo(graphNoAyuda);
                GameManager.Instance.ResolverPuzzle("AidenNiega");
            }
        }

        //tipus de puzzle d'ajuda BRIDGE
        else if (tipo == PuzleHelpType.BRIDGE)
        {
            //si el player te mes de 40 de confianca aiden acceptara
            if (GameManager.Instance.confianza >= 40)
            {
                //llanca el dialeg de acceptacio i espera a que s'acabi
                DialogueManager.Instance.StartDialogue(graphSiAyuda);
                //es suscriu per saber quan s'acaba el dialeg
                onDialogueEnded.Subscribe(HandleDialogoAcabado);
            }
            else
            {
                //en BRIDGE no es necessari cap puzle registrar si aiden es nega a ajudar
                DialogueManager.Instance.StartInfo(graphNoAyuda);
            }
        }

        //desactivar el trigger en tots els casos per a que no torni a ser interactuable una vegada ja s'ha interactuat amb ell
        this.gameObject.SetActive(false);
    }

    //s'executa quan el dialeg d'acceptacio termina
    private void HandleDialogoAcabado()
    {
        //es desuscriu de l'event per evitar que es cridi mes vegades sense voler
        onDialogueEnded.Unsubscribe(HandleDialogoAcabado);

        //en BRIDGE es requereix que Aiden esperi en el desti fins a que el player arribi a un punt concret
        //en DOOR aixo no es necessari, nomes actua i torna amb el jugador
        bool esperarEnDestino = (tipo == PuzleHelpType.BRIDGE);

        //ordena a aiden que es mogui al punt de desti i executi l'ajuda
        AidenController.Instance.Help(puntoDestino.position, onAyudaCompletada, esperarEnDestino);
    }

    //s'utilitza per a cambiar l'estat de la UI que apareix a sobre del triggerAyuda per informa de que hi ha una interaccio
    private void SetUI(string texto, float size, bool animar)
    {
        if (canvasworld != null)
        {
            this.texto.text = texto;
            this.texto.fontSize = size;
            textoAnimator.enabled = animar; 
        }
    }

    //quan el player entra en el rang del trigger, el text canvia per indicar al player que pot interactuar
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("Pulsa E para pedir ayuda a Aiden", fontSizeInteract, false);
        }
    }

    //quan el player surt del rang es torna a posar el "!" a sobre del trigger per informar de que hi ha una interaccio disponible
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            SetUI("!", fontSizeExclamacion, true);
        }
    }
}