using System.Linq;
using UnityEngine;

public class ObjetoGuardable : MonoBehaviour
{
    public SOEscena soEscena;
    public string id;

    [Header("Persistencia")]
    [Tooltip("Si true, NO se borra al morir el jugador")]
    public bool persisteAlMorir = false;

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
            rot = transform.rotation,
            persisteAlMorir = persisteAlMorir
        };

        if (idx >= 0)
            soEscena.objetos[idx] = parte; 
        else
            soEscena.objetos.Add(parte);     
    }

    public void Restaurar()
    {
        ScenePart parte = soEscena.objetos.FirstOrDefault(p => p.id == id);
        if (parte.id == null)
        {
            return;
        }
        gameObject.SetActive(parte.active);
        transform.position = parte.pos;
        transform.rotation = parte.rot;
    }
}