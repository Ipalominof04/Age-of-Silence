using System.Collections.Generic;
using UnityEngine;

public class TriggerCambioEscena : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] string nombreEscena;
    [Header("Spawn")]
    [SerializeField] string miDoorID;
    [SerializeField] string targetDoorID;
    [Header("Requiere puzzle")]
    [SerializeField] private string puzzleRequeridoID;

    private void Start()
    {
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
            gameObject.SetActive(GameManager.Instance.PuzzleResuelto(puzzleRequeridoID));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}