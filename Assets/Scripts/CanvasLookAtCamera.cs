using UnityEngine;
public class CanvasLookAtCamera : MonoBehaviour
{
    private Camera camara;

    void Update()
    {
        if (camara == null)
            camara = Camera.main;

        if (camara == null) return;

        Vector3 direction = transform.position - camara.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}