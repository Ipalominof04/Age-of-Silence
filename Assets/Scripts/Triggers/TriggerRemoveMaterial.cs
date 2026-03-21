using UnityEngine;

public class TriggerRemoveMaterial : MonoBehaviour
{
    public GameObject objetoObjetivo; // objeto al que se le quitará el material

    Renderer rend;
    Material[] materialesOriginales;

    void Start()
    {
        rend = objetoObjetivo.GetComponent<Renderer>();
        materialesOriginales = rend.materials; // guardamos los materiales originales
    }

    void OnTriggerEnter(Collider other)
    {
        // Restauramos los materiales originales
        rend.materials = materialesOriginales;
        Destroy(this.gameObject);
    }
}