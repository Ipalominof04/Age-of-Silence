using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private string objetoRequerido;

    public void TryOpen(ObjetoAgarrado objetoMano)
    {
        if (objetoMano.objeto != null && objetoMano.objeto.name == objetoRequerido)
            Debug.Log("Puerta abierta");
        else
            Debug.Log("Necesitas el objeto correcto");
    }
}