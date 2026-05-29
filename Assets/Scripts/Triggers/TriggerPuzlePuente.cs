using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzlePuente : MonoBehaviour, IPuzle
{
    [SerializeField] private string objetoRequerido; // obj q hay q colocar para resolver el puzle
    
    public bool infoMostrada = false; //evita que el dialogo de info se muestre mas de una vez

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;
    public RuntimeDialogueGraph grapSinObjeto;
    public RuntimeDialogueGraph graphObjetoIncorrecto;

    [Header("Meshes")]
    [SerializeField] GameObject maderas; // mesh de las maderas oculto

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld; //icono flotante q desaparece al recibir el dialogo

    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente; // collider invisible q impide el paso hasta resolver el puzle

    private PlayerController player;


    private void Start()
    {
        // Busca el player en la escena para poder llamar a ClearTriggerPuzle despues
        player = FindAnyObjectByType<PlayerController>();
    }

    // Se llama desde PlayerController cuando el player pulsa interactuar estando en rango
    public void Interact(ObjetoAgarrado objetoMano)
    {
        // Si el player no lleva ningun objeto, avisa y corta
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }
        if (objetoMano.nombreObjeto == objetoRequerido)
        {
            // Puzzle resuelto
            bloqueoPuente.SetActive(false); //quita el collider del bloqueo para q el player pueda pasar
            gameObject.SetActive(false); // desactiva el trigger para q no se pueda volver a interactuar
            maderas.SetActive(true); // muestra el mesh de las tablas
            player.ClearTriggerPuzle(); // le dice al script del player q no hay puzzle activo en rango

        }
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto); // El objeto existe pero no es el correcto
        // En cualquier caso vacia la mano del player tras interaccionar
        objetoMano.ClearObject();
    }

    // se llama desde PlayerController cuando el player entra en el rango del trigger
    public void OnEnterRange()
    {
        // solo muestra el dialogo informativo la primera vez que el player se acerca
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo); // lanza el dialogo explicativo
            canvasworld.SetActive(false); // oculta el icono flotante una vez mostrado el dialogo
        }
    }
}