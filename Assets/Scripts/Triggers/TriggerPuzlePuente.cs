using UnityEngine;
using UnityEngine.Events;
public class TriggerPuzlePuente : MonoBehaviour, IPuzle
{
    [SerializeField] private string objetoRequerido; // objecte que cal col·locar per resoldre el puzzle

    public bool infoMostrada = false; // evita que el dialeg d'info es mostri mes d'una vegada
    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;
    public RuntimeDialogueGraph grapSinObjeto;
    public RuntimeDialogueGraph graphObjetoIncorrecto;
    [Header("Meshes")]
    [SerializeField] GameObject maderas; // mesh de les fustes amagat
    [Header("Canvas")]
    [SerializeField] GameObject canvasworld; // icona flotant que desapareix en rebre el dialeg
    [Header("Bloqueo puente")]
    [SerializeField] private GameObject bloqueoPuente; // collider invisible que impedeix el pas fins resoldre el puzzle
    private PlayerController player;
    // L'Start busca el player a l'escena per poder cridar a ClearTriggerPuzle despres
    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }
    // L'Interact es crida des del PlayerController quan el player prem interactuar estant en rang
    public void Interact(ObjetoAgarrado objetoMano)
    {
        // si el player no porta cap objecte, avisa i talla
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }
        if (objetoMano.nombreObjeto == objetoRequerido)
        {
            // puzzle resolt
            bloqueoPuente.SetActive(false); // treu el collider del bloqueig perque el player pugui passar
            gameObject.SetActive(false); // desactiva el trigger perque no es pugui tornar a interactuar
            maderas.SetActive(true); // mostra el mesh de les fustes
            player.ClearTriggerPuzle(); // li diu al script del player que no hi ha puzzle actiu en rang
        }
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto); // l'objecte existeix pero no es el correcte
        // en qualsevol cas buida la ma del player despres d'interaccionar
        objetoMano.ClearObject();
    }
    // L'OnEnterRange es crida des del PlayerController quan el player entra en el rang del trigger
    public void OnEnterRange()
    {
        // nomes mostra el dialeg informatiu la primera vegada que el player s'acosta
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo); // llanca el dialeg explicatiu
            canvasworld.SetActive(false); // amaga la icona flotant un cop mostrat el dialeg
        }
    }
}