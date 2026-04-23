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
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            print("no tienes nada en la mano");
            return;
        }
        string objeto = objetoMano.nombreObjeto;

        if (objeto == "ComidaNPC")
        {
            meshComida.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("MisionComidaNPC");

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
