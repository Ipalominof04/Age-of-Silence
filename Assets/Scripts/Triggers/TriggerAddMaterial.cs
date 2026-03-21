using UnityEngine;

public class TriggerAddMaterial : MonoBehaviour
{
    public GameObject objetoObjetivo; // objeto al que se le añadirá el material
    public Material materialExtra;     // material que se añadirá

    Renderer rend;
    Material[] materialesOriginales;

    void Start()
    {
        rend = objetoObjetivo.GetComponent<Renderer>();
        materialesOriginales = rend.materials; // guardamos los materiales originales
    }

    void OnTriggerEnter(Collider other)
    {
        // Creamos un nuevo array con 1 espacio extra
        Material[] mats = new Material[materialesOriginales.Length + 1];
        for (int i = 0; i < materialesOriginales.Length; i++)
            mats[i] = materialesOriginales[i];

        mats[mats.Length - 1] = materialExtra; // añadimos el material extra
        rend.materials = mats;
    }
}