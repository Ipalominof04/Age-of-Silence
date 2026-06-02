using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ObjetoAgarrado", fileName = "ObjetoAgarrado")]
public class ObjetoAgarrado : ScriptableObject
{
    public string nombreObjeto;
    // guarda son objecte q el jugador agafa
    public void SaveObject(GameObject obj)
    {
        nombreObjeto = obj.name;
    }

    // borra info del SO
    public void ClearObject()
    {
        nombreObjeto = null;
    }
}

