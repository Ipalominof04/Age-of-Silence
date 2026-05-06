using System.Collections;
using UnityEngine;

public class LoadManagers : MonoBehaviour
{
    public void Start()
    {
        string destino = GameManager.Instance.datos.NameScene;
        Debug.Log("LoadManagers destino: " + destino);
        GameManager.Instance.IrAScene(!string.IsNullOrEmpty(destino) ? destino : "Escena2");
    }
}