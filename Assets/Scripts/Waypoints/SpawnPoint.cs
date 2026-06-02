using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // id unic del trigger associat a aquest punt on apareix
    [SerializeField] private string doorID;
    // distancia q es desplaca Aiden respecte al punt on apareix
    [SerializeField] private float aidenOffset = 1f;

    public string GetDoorID()
    {
        return doorID; // retorna id del spawn
    }

    public float AidenOffset
    {
        get { return aidenOffset; } // propietat de lectura per obtenir el offset
    }
}
