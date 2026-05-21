using UnityEngine;
public class ArmaController : MonoBehaviour
{
    [SerializeField] GameObject ShootUI;
    [SerializeField] public ParticleSystem shootFlash;

    public void Equipar()
    {
        ShootUI.SetActive(true);
    }

    public void Soltar()
    {
        if (ShootUI != null)
            ShootUI.SetActive(false);
        Destroy(gameObject);
    }
}