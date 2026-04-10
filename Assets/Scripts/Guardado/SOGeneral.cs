using UnityEngine;

[CreateAssetMenu(fileName = "SOGeneral", menuName = "ScriptableObjects/SOGeneral")]
public class SOGeneral : ScriptableObject
{
    [Header("Datos a guardar")]
    public float confianza;
    public float PlayerHealth;
    public ObjetoAgarrado objetoMano;
    private string nameScene;
    public string NameScene { get { return nameScene; } set
        {
            nameScene = value;
        } 
    }


}
