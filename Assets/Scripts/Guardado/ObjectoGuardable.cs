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

        Debug.Log($"[Restaurar] {id} | lista count: {soEscena.objetos.Count} | parte.id: '{parte.id}'");
        
        if (parte.id == null)
        {
            return;
        }
        

        Debug.Log($"[Restaurar] {id} ? active={parte.active}");
        gameObject.SetActive(parte.active);
        transform.position = parte.pos;
        transform.rotation = parte.rot;
    }
}