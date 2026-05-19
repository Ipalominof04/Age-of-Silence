using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ObjetoAgarrado", fileName = "ObjetoAgarrado")]
public class ObjetoAgarrado : ScriptableObject
{
    public string nombreObjeto;
    public void SaveObject(GameObject obj)
    {
        nombreObjeto = obj.name;
    }

    public void ClearObject()
    {
        nombreObjeto = null;
    }
}

