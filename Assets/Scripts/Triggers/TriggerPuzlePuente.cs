using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzlePuente : MonoBehaviour, IPuzle
{
    [SerializeField] private string objetoRequerido;
    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;
    public RuntimeDialogueGraph grapSinObjeto;
    public RuntimeDialogueGraph graphObjetoIncorrecto;

    [Header("Meshes")]
    [SerializeField] GameObject maderas;

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;


    private PlayerController player;


    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    public void Interact(ObjetoAgarrado objetoMano)
    {

        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }
        if (objetoMano.nombreObjeto == objetoRequerido)
        {
            gameObject.SetActive(false);
            maderas.SetActive(true);
            player.ClearTriggerPuzle();


        }
        else
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        objetoMano.ClearObject();
    }

    public void OnEnterRange()
    {
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            canvasworld.SetActive(false);
        }
    }
}