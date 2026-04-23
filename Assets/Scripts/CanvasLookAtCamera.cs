using UnityEngine;

public class CanvasLookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera camara;

    private void Start()
    {
        camara = Camera.main;
    }
    void Update()
    {
        if (camara == null) return;

       
        Vector3 direction = transform.position - camara.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}