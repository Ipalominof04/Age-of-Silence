using UnityEngine;

public class ObjectoActivable : MonoBehaviour
{
    [Header("Requiere puzzle")]
    [SerializeField] private string puzzleRequeridoID;

    private void Start()
    {
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
        {
            gameObject.SetActive(GameManager.Instance.PuzzleResuelto(puzzleRequeridoID));
        }
}

    // Update is called once per frame
    void Update()
    {

    }
}
