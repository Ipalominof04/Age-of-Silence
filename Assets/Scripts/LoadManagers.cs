using System.Collections;
using UnityEngine;

public class LoadManagers : MonoBehaviour
{
    public void Start()
    {
        string destino = GameManager.Instance.datos.NameScene;
        GameManager.Instance.IrAScene(!string.IsNullOrEmpty(destino) ? destino : "Escena2");
    }
}