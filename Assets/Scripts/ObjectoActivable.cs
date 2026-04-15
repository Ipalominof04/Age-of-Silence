using UnityEngine;
public class ObjectoActivable : MonoBehaviour
{
    [Header("Requiere puzzle")]
    [SerializeField] private string puzzleRequeridoID;

    private void Start()
    {
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
        {
            bool resuelto = GameManager.Instance.PuzzleResuelto(puzzleRequeridoID);
            Debug.Log($"{gameObject.name}: puzzle '{puzzleRequeridoID}' resuelto={resuelto} → SetActive({resuelto})");
            gameObject.SetActive(resuelto);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: puzzleRequeridoID está vacío");
        }
    }
}