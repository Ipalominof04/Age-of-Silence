using System.Linq;
using UnityEngine;

public class ObjetoGuardable : MonoBehaviour
{
    public SOEscena soEscena;
    public string id; // ponlo igual al nombre del GameObject, o asígnalo a mano

    private void Reset()
    {
        id = gameObject.name; // se autorellena al añadir el componente
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
            soEscena.objetos[idx] = parte;  // actualiza
        else
            soEscena.objetos.Add(parte);     // primera vez, lo añade solo
    }

    public void Restaurar()
    {
        ScenePart parte = soEscena.objetos.FirstOrDefault(p => p.id == id);

        if (parte.id == null) return; // nunca se guardó, deja defaults

        gameObject.SetActive(parte.active);
        transform.position = parte.pos;
        transform.rotation = parte.rot;
    }
}