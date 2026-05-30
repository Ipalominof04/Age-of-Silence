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

    //s'executa quan el jugador interactua amb el trigger del puzle i porta un objecte a la ma (el SO de ObjetoAgarrado)
    public void Interact(ObjetoAgarrado objetoMano)
    {
        //si el jugador no porta cap objecte, no fa res (en altres puzles surt un graph, aixo depen)
        if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
        {
            return;
        }

        string fusible = objetoMano.nombreObjeto;

        //comprova quin fusible porta el jugador i activa el mesh corresponent
        if (fusible == "FusibleVerde")
        {
            meshVerde.SetActive(true);
            //treu l'objecte de la del jugador (posa l'OS en blanc)
            objetoMano.ClearObject();
            //notifica al gm de que s'ha resolt el puzle d'aquell fusible per obrir una porta que necessita aquest puzle
            GameManager.Instance.ResolverPuzzle("FusibleVerde");
        }
        //comprova quin fusible porta el jugador i activa el mesh corresponent
        else if (fusible == "FusibleAzul")
        {
            meshAzul.SetActive(true);
            //treu l'objecte de la del jugador (posa l'OS en blanc)
            objetoMano.ClearObject();
            //notifica al gm de que s'ha resolt el puzle d'aquell fusible per obrir una porta que necessita aquest puzle
            GameManager.Instance.ResolverPuzzle("FusibleAzul");
        }
        //comprova quin fusible porta el jugador i activa el mesh corresponent
        else if (fusible == "FusibleAmarillo")
        {
            meshAmarillo.SetActive(true);
            //treu l'objecte de la del jugador (posa l'OS en blanc)
            objetoMano.ClearObject();
            //notifica al gm de que s'ha resolt el puzle d'aquell fusible per obrir una porta que necessita aquest puzle
            GameManager.Instance.ResolverPuzzle("FusibleAmarillo");
        }
    }

    //s'executa quan el jugador entra al rang d'interaccio del trigger
    public void OnEnterRange()
    {
        //forca al jugador a entrar en idle per a que no es quedi bugejat en altre estat (sobre tot per l'animacio)
        PlayerController.Instance.ForceIdle();

        //mostra un dialeg d'info nomes la primera vegada que s'apropa (torna a sortir si el player surt de l'escena i torna a entrar)
        if (!infoMostrada && graphInfo != null)
        {
            infoMostrada = true;
            DialogueManager.Instance.StartInfo(graphInfo);
            //amaga l'indicacio de que hi ha una interaccio disponible
            canvasworld.SetActive(false);
        }
    }
}


