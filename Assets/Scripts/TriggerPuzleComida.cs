using UnityEngine;

public class TriggerPuzleComida : MonoBehaviour, IPuzle
{

    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Meshes")]
    [SerializeField] private GameObject meshComida;


    public void Interact(ObjetoAgarrado objetoMano)
    {
       
            meshComida.SetActive(true);
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
}
