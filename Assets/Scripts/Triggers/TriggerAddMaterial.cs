using UnityEngine;
public class TriggerAddMaterial : MonoBehaviour
{
    public GameObject[] objetosObjetivo;
    public Material materialExtra;
    Renderer[] renderers;
    Material[][] materialesOriginales;
    // L'Start guarda una referencia al Renderer de cada objecte i una copia dels seus materials originals
    void Start()
    {
        renderers = new Renderer[objetosObjetivo.Length];
        materialesOriginales = new Material[objetosObjetivo.Length][];
        for (int i = 0; i < objetosObjetivo.Length; i++)
        {
            renderers[i] = objetosObjetivo[i].GetComponent<Renderer>();
            materialesOriginales[i] = renderers[i].materials;
        }
    }
    // L'OnTriggerEnter afegeix el material extra a cada objecte objectiu quan alguna cosa entra al trigger
    void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            // creem un array nou amb un slot extra per afegir el material sense sobreescriure els originals
            Material[] originales = materialesOriginales[i];
            Material[] mats = new Material[originales.Length + 1];
            for (int j = 0; j < originales.Length; j++)
                mats[j] = originales[j];
            mats[mats.Length - 1] = materialExtra;
            renderers[i].materials = mats;
        }
    }
}