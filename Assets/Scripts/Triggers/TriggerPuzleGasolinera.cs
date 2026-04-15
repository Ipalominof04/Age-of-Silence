using System.Net.NetworkInformation;
using UnityEngine;

public class TriggerPuzleGasolinera : MonoBehaviour, IPuzle
{

    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Meshes")]
    [SerializeField] private GameObject meshGalon;
    [SerializeField] private GameObject meshCerilla;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            print("no tienes nada en la mano");
            return;
        }
        string objeto = objetoMano.nombreObjeto;

        if (objeto == "Galon")
        {
            meshGalon.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("GalonUsado");

        }
        else if (objeto == "Cerillas")
        {
            meshCerilla.SetActive(true);
            objetoMano.UsarObjeto();
           // GameManager.Instance.ResolverPuzzle("CerillasTiradas");
        }
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
