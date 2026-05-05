using UnityEngine;
using UnityEngine.Video;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] VideoPlayer player;
    private void Awake()
    {
        player.targetCamera = Camera.main;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
