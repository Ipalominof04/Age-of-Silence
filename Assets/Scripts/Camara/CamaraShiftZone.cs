using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CamaraShiftZone : MonoBehaviour
{
    [Header("Camaras")]
    public CinemachineCamera normalCam;
    public CinemachineCamera shiftedCam; //donde quieres mirar

    [Header("Tiempo")]
    public float timeBeforeReturn = 3f; // segundos mirando la otra zona

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shiftedCam.Priority = 3;
            normalCam.Priority = 2;

            StartCoroutine(ReturnToPlayer());
        }
    }

    IEnumerator ReturnToPlayer()
    {
        yield return new WaitForSeconds(timeBeforeReturn);

        shiftedCam.Priority = 1;
        normalCam.Priority = 2;

        gameObject.SetActive(false);
    }
}
