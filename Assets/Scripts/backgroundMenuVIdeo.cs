using UnityEngine;
using UnityEngine.Video;

public class backgroundMenuVideo : MonoBehaviour
{
    [SerializeField] VideoPlayer videoplayer;
    //panel negro que cubre el fondo mientras carga el video
    [SerializeField] GameObject panelNegro; 

    private void OnEnable()
    {
        //activar el panel mientras el video carga
        panelNegro.SetActive(true);
        //asignar la camara principal al videoplayer
        videoplayer.targetCamera = Camera.main;
        //reproducir el video de fondo
        videoplayer.Play(); 
    }

    private void Update()
    {
        //si la camara se pierde (al cambiar de escena), reasignarla
        if (videoplayer.targetCamera == null && Camera.main != null)
        {
            videoplayer.targetCamera = Camera.main;
            videoplayer.Play();
        }

        //cuando el video esta reproduciendose, ocultar el panel negro
        if (panelNegro.activeSelf && videoplayer.isPlaying)
        {
            panelNegro.SetActive(false);
        }
    }

    private void OnDisable()
    {
        //parar el video al desactivar el objeto
        videoplayer.Stop(); 
    }
}