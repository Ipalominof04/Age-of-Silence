using UnityEngine;
using UnityEngine.Video;

public class backgroundMenuVideo : MonoBehaviour
{
    // referencia al VideoPLayer q reprodueix el video de fons al menu
    [SerializeField] VideoPlayer videoplayer;

    private void OnEnable()
    {
        // quan objecte s'activa, es reprodueix el video
        videoplayer.Play();
    }

    private void OnDisable()
    {
        // quan objecte es desactiva, es para el video
        videoplayer.Stop();
    }
}