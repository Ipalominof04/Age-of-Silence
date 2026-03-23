using UnityEngine;

public class NeveraPuzle : MonoBehaviour
{
    public void SubirConfianza()
    {
        GameManager.Instance.SumValor(5);
    }
}
