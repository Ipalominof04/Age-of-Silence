using UnityEngine;
using UnityEngine.Video;

public class backgroundMenuVideo : MonoBehaviour
{
    [SerializeField] VideoPlayer videoplayer;

    private void OnEnable()
    {
        videoplayer.Play();
    }

    private void OnDisable()
    {
        videoplayer.Stop();
    }
}