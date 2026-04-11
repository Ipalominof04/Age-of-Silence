using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOEscena", menuName = "Juego/SOEscena")]
public class SOEscena : ScriptableObject
{
    public string sceneName;
    public List<ScenePart> objetos = new List<ScenePart>();
}