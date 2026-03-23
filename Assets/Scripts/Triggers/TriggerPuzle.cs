using UnityEngine;
using UnityEngine.Events;

public class TriggerPuzle : MonoBehaviour
{
    [SerializeField] private string objetoRequerido;
    public RuntimeDialogueGraph RuntimeGraph;

    [Header("Acción al resolver el puzzle")]
    public UnityEvent OnPuzzleSolved;

    public void TryOpen(ObjetoAgarrado objetoMano)
    {
        if (objetoMano.nombreObjeto == null)
        {
            Debug.Log("No tengo ningún objeto en la mano");
            return; //si no tengo ningun objeto, no hago nada
        }

        if (objetoMano.nombreObjeto == objetoRequerido)
        {
            Debug.Log("Abierto");
            OnPuzzleSolved?.Invoke();
        }

        else
            Debug.Log("Necesitas el objeto correcto");

        objetoMano.UsarObjeto(); //independientemente de si el objeto es correcto o no, se usa
    }
}