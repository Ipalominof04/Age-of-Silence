using UnityEngine;
using UnityEngine.Playables;

public class CinematicaSelector : MonoBehaviour
{
    public PlayableDirector finalMalo;
    public PlayableDirector finalBueno;
    public float umbralConfianza = 40f;

    private void Start()
    {
        finalMalo.gameObject.SetActive(false);
        finalBueno.gameObject.SetActive(false);
        ReproducirCinematica();
    }

    public void ReproducirCinematica()
    {
        if (GameManager.Instance.confianza >= umbralConfianza)
        {
            finalBueno.gameObject.SetActive(true);
            finalBueno.Play();
        }
        else
        {
            finalMalo.gameObject.SetActive(true);
            finalMalo.Play();
        }
    }
}
