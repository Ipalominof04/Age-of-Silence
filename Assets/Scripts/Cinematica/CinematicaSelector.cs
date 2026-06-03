using UnityEngine;
using UnityEngine.Playables;

public class CinematicaSelector : MonoBehaviour
{
    // tria i reprodueix la cinematica final segons la confianca acumulapa pel jugador
    public PlayableDirector finalMalo;
    public PlayableDirector finalBueno;
    public float umbralConfianza = 40f;

    private void Start()
    {
        // amaguem els dos objectes fins saber quin cal reproduir
        finalMalo.gameObject.SetActive(false);
        finalBueno.gameObject.SetActive(false);
        ReproducirCinematica();
    }

    //activa i reprodueix la cinematica q correspon
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
