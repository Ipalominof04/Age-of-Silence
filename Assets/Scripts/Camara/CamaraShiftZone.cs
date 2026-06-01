using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CamaraShiftZone : MonoBehaviour
{
    [Header("Camaras")]
    public CinemachineCamera normalCam; // camara que segueix al jugador
    public CinemachineCamera shiftedCam; //camara que enfoca la zona que volem mostrar

    [Header("Tiempo")]
    public float timeBeforeReturn = 3f; // segons abans de tornar a la camara del jugador

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            shiftedCam.Priority = 3; // dona mes prioritat a la shiftedcam
            normalCam.Priority = 2; // mante la camera normal amb una prioritat inferior

            StartCoroutine(ReturnToPlayer());
        }
    }

    IEnumerator ReturnToPlayer()
    {
        // espera el temps configurat abans de canviar de now la camera
        yield return new WaitForSeconds(timeBeforeReturn);

        shiftedCam.Priority = 1; // redueix la prioritat de la shiftedcam
        normalCam.Priority = 2; // torna a donar prioritat a la camara principal

        gameObject.SetActive(false); // es desactiva el trigger pq no es pugui activar una segona vegada
    }
}
