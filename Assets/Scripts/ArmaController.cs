using UnityEngine;

public class ArmaController : MonoBehaviour
{
    [SerializeField] GameObject ShootUI;

    public void Equipar()
    {
        ShootUI.SetActive(true);
    }

    public void Soltar()
    {
        ShootUI.SetActive(false);
        Destroy(gameObject);
    }
}
