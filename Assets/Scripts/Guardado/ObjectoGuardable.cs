using System.Linq;
using UnityEngine;

public class ObjetoGuardable : MonoBehaviour
{
    public SOEscena soEscena;
    public string id; 

    private void Reset()
    {
        id = gameObject.name;
    }

    public void Guardar()
    {
        int idx = soEscena.objetos.FindIndex(p => p.id == id);

        ScenePart parte = new ScenePart
        {
            id = id,
            active = gameObject.activeSelf,
            pos = transform.position,
            rot = transform.rotation
        };

        if (idx >= 0)
            soEscena.objetos[idx] = parte; 
        else
            soEscena.objetos.Add(parte);     
    }

    public void Restaurar()
    {
        ScenePart parte = soEscena.objetos.FirstOrDefault(p => p.id == id);

        if (parte.id == null) return;

        Debug.Log($"{id}: restaurando active={parte.active}");
        gameObject.SetActive(parte.active);
        transform.position = parte.pos;
        transform.rotation = parte.rot;
    }
}