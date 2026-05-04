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
   // [SerializeField] private string puzzleRequeridoID;
    [SerializeField] private List<string> puzzleRequeridoID;
    [SerializeField] private bool requiereTodos; // false cuando 

    private void Start()
    {
        if (puzzleRequeridoID == null || puzzleRequeridoID.Count == 0) return;

        bool activo = false;
        if (requiereTodos)
        {
            activo = true;
            foreach (string id in puzzleRequeridoID)
            {
                if (!GameManager.Instance.PuzzleResuelto(id))
                {
                    activo = false;
                    break;
                }
            }

        }
        else
        {
            activo = false;
            foreach (string id in puzzleRequeridoID)
            {
                bool resuelto = GameManager.Instance.PuzzleResuelto(id);
                if (resuelto)
                {
                    activo = true;
                    break;
                }
            }
        }
        gameObject.SetActive(activo);
    }
    //private void Start()
    //{
    //    if (puzzlesRequeridosIDs == null || puzzlesRequeridosIDs.Count == 0) return;

    //    bool activo = requiereTodos
    //        ? puzzlesRequeridosIDs.TrueForAll(id => GameManager.Instance.PuzzleResuelto(id))
    //        : puzzlesRequeridosIDs.Exists(id => GameManager.Instance.PuzzleResuelto(id));

    //    gameObject.SetActive(activo);
    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCollider"))
        {
            GameManager.Instance.lastDoorID = targetDoorID;
            GameManager.Instance.ChangeScene(nombreEscena);
        }
    }
}