using Unity.Cinemachine;
using UnityEngine;

public class FindPlayerForCamera : MonoBehaviour
{
    private CinemachineCamera cinemachine;

    private void Start()
    {
        cinemachine = gameObject.GetComponent<CinemachineCamera>();
        PlayerController player = FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            cinemachine.Follow = player.transform;
        }
    }
}