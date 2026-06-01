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
    [SerializeField]  string nombreEscena;
    [SerializeField]  string targetDoorID;

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        // si el puzle no requereix objecte, es resol directament
        if (!requiereObjeto)
        {
            ResolvePuzzle();
            return;
        }
        // si el jugador no porta cap objecte
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            DialogueManager.Instance.StartInfo(grapSinObjeto);
            return;
        }
        // si objecte es el correcte
        if (objetoMano.nombreObjeto == objetoRequerido)
        {;
            ResolvePuzzle();
        }
        else 
            DialogueManager.Instance.StartInfo(graphObjetoIncorrecto);

        //neteja objecte de la ma del jugador
        objetoMano.ClearObject();
    }

    public void OnEnterRange()
    {
        // mostra la informacio una vegada
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            canvasworld.SetActive(false);
            PlayerController.Instance.ForceIdle(); // força al jugador a estat idle
        }
    }
    private void ResolvePuzzle()
    {
        // desactiva interaccio del puzle al jugador
        PlayerController.Instance.ClearTriggerPuzle();
        // si hi ha canvi escena configurat
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            GameManager.Instance.lastDoorID = targetDoorID; // guarda porta desti per spawn
            GameManager.Instance.ChangeScene(nombreEscena);
        }
        gameObject.SetActive(false);
    }
}