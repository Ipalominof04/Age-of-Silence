using System.Collections.Generic;
using UnityEngine;

public class TriggerCambioEscena : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] string nombreEscena;
    [Header("Spawn")]
    [SerializeField] string targetDoorID;
    [Header("Requiere puzzle")]
    [SerializeField] private string puzzleRequeridoID;

    //A l'Start comprobem si esta el puzzle requerit a la llista de puzzles del GameManager i en cas de que no es trobi l'objecte es desactiva.
    private void Start()
    {
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
            gameObject.SetActive(GameManager.Instance.PuzzleResuelto(puzzleRequeridoID));
    }

    // En el OnTriggerEnter comprobem si fem collisio amb el player i si es aixi assignem el doorID i cambiem d'escena
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}