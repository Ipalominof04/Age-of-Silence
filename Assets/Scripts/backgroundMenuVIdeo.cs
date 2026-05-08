using System.Collections;
using UnityEngine;
using UnityEngine.Video;
public class backgroundMenuVideo : MonoBehaviour
{
    [SerializeField] VideoPlayer videoplayer;
    private void OnEnable()
    {
        videoplayer.targetCamera = Camera.main;
    }
}