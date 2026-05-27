using UnityEngine;

public class TriggerPuzleFusible : MonoBehaviour, IPuzle
{
    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Meshes")]
    [SerializeField] private GameObject meshVerde;
    [SerializeField] private GameObject meshAzul;
    [SerializeField] private GameObject meshAmarillo;

    [Header("Canvas")]
    [SerializeField] GameObject canvasworld;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            return;
        }

        string fusible = objetoMano.nombreObjeto;

        if (fusible == "FusibleVerde")
        {
            meshVerde.SetActive(true);
            objetoMano.ClearObject();
            GameManager.Instance.ResolverPuzzle("FusibleVerde");
        }
        else if (fusible == "FusibleAzul")
        {
            meshAzul.SetActive(true);
            objetoMano.ClearObject();
            GameManager.Instance.ResolverPuzzle("FusibleAzul");
        }
        else if (fusible == "FusibleAmarillo")
        {
            meshAmarillo.SetActive(true);
            objetoMano.ClearObject();
            GameManager.Instance.ResolverPuzzle("FusibleAmarillo");
        }
    }

    public void OnEnterRange()
    {
        PlayerController.Instance.ForceIdle();
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            canvasworld.SetActive(false);
        }
    }
}


