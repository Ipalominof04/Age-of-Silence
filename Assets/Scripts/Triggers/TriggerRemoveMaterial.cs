using UnityEngine;

public class TriggerRemoveMaterial : MonoBehaviour
{
    public GameObject objetoObjetivo;

    Renderer rend;
    Material[] materialesOriginales;

    void Start()
    {
        // Obtenim el Renderer de l'objecte objectiu.
        rend = objetoObjetivo.GetComponent<Renderer>();

        // Guardem els materials actuals per poder restaurar-los mes endavant.
        materialesOriginales = rend.materials;
    }

    void OnTriggerEnter(Collider other)
    {
        // Recuperem els materials originals de l'objecte.
        rend.materials = materialesOriginales;

        // Eliminem aquest trigger un cop s'ha activat.
        Destroy(this.gameObject);
    }
}