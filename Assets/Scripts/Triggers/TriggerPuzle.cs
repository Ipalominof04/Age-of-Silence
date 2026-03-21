using UnityEngine;

public class TriggerPuzle : MonoBehaviour
{
    [SerializeField] private string objetoRequerido;
    public RuntimeDialogueGraph RuntimeGraph;

    public void TryOpen(ObjetoAgarrado objetoMano)
    {
        if (objetoMano.nombreObjeto == null)
        {
            Debug.Log("No tengo ningún objeto en la mano");
            return; //si no tengo ningun objeto, no hago nada
        }

        if (objetoMano.nombreObjeto == objetoRequerido)
            Debug.Log("Abierto");
        else
            Debug.Log("Necesitas el objeto correcto");

        objetoMano.UsarObjeto(); //independientemente de si el objeto es correcto o no, se usa
    }
}