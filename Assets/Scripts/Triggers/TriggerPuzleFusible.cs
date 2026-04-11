using UnityEngine;

public class TriggerPuzleFusible : MonoBehaviour, IPuzle
{
    public bool infoMostrada = false;

    [Header("Dialogos")]
    public RuntimeDialogueGraph graphInfo;

    [Header("Eventos")]
    [SerializeField] private GameEvent onFusibleVerdeColocado;
    [SerializeField] private GameEvent onFusibleAzulColocado;
    [SerializeField] private GameEvent onFusibleAmarilloColocado;

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
            onFusibleVerdeColocado.Raise();
        }
        else if (fusible == "FusibleAzul" && verdeColocado && !azulColocado)
        {
            azulColocado = true;
            meshAzul.SetActive(true);
            objetoMano.UsarObjeto();
            onFusibleAzulColocado.Raise();
        }
        else if (fusible == "FusibleAmarillo" && azulColocado)
        {
            meshAmarillo.SetActive(true);
            objetoMano.UsarObjeto();
            onFusibleAmarilloColocado.Raise();
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


