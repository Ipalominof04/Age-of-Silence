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

    /*  [Header("Dialogos")]
      [SerializeField] private RuntimeDialogueGraph graphOrdenIncorrecto;
      [SerializeField] private RuntimeDialogueGraph graphNoFusible; */

    private bool verdeColocado;
    private bool azulColocado;

    public void Interact(ObjetoAgarrado objetoMano)
    {
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            print("no tienes fusible gilipollas");
            return;
        }

        string fusible = objetoMano.nombreObjeto;

        if (fusible == "FusibleVerde" && !verdeColocado)
        {
            verdeColocado = true;
            meshVerde.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("FusibleVerde");
        }
        else if (fusible == "FusibleAzul" && verdeColocado && !azulColocado)
        {
            azulColocado = true;
            meshAzul.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("FusibleAzul");
        }
        else if (fusible == "FusibleAmarillo" && azulColocado)
        {
            meshAmarillo.SetActive(true);
            objetoMano.UsarObjeto();
            GameManager.Instance.ResolverPuzzle("FusibleAmarillo");
        }
        else
            //  DialogueManager.Instance.StartInfo(graphOrdenIncorrecto);
            print("orden incorrecto");
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


