using UnityEngine;

[CreateAssetMenu(fileName = "SOGeneral", menuName = "Juego/SOGeneral")]
public class SOGeneral : ScriptableObject
{
    [Header("Datos a guardar")]
    public float confianza;
    public float PlayerHealth;
    public ObjetoAgarrado objetoMano;
    private string nameScene;
    public SOEscena[] escenas;
    public string NameScene { get { return nameScene; } set
        {
            nameScene = value;
        } 
    }


}
