using UnityEngine;
public class ArmaController : MonoBehaviour
{
    [SerializeField] GameObject ShootUI;
    [SerializeField] public ParticleSystem shootFlash;

    //funcio per equipar l'arma al player (es crida desde el script del player)
    public void Equipar()
    {
        ShootUI.SetActive(true);
    }
    //funcio per soltar l'arma del player (es crida desde el script del player), treu l'arma de la ma del player
    public void Soltar()
    {
        if (ShootUI != null)
            ShootUI.SetActive(false);
        Destroy(gameObject);
    }
}