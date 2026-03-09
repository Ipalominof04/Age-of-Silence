using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    [Header("Valor")]
    public int Valor = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void SumValor(int amount)
    {
        Valor += amount;
        Debug.Log($"Valor subió a: {Valor}");
    }

    public void RestValor(int amount)
    {
        Valor -= amount;
        Debug.Log($"Valor bajó a: {Valor}");
    }
}
