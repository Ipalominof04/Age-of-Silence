using UnityEngine;

public class CanvasLookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    void Update()
    {
        if (_camera == null) return;

       
        Vector3 direction = transform.position - _camera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}