using UnityEngine;
public class ObjectoActivable : MonoBehaviour
{
    [SerializeField] private string puzzleRequeridoID;
    [SerializeField] private Renderer rend;
    [SerializeField] private Collider col;

    private void Start()
    {
        // NOTA: Anteriormente se usaba gameObject.SetActive(resuelto) sobre este mismo objeto,
        // lo que causaba un bug: si el objeto empezaba inactivo, Unity nunca ejecutaba este Start(),
        // y si empezaba activo pero el puzzle no estaba resuelto, se desactivaba y no podía
        // reactivarse solo. La solución es controlar Renderer y Collider directamente, con el objeto activado en la escena
        // así el script siempre se ejecuta independientemente del estado del padre en la jerarquía.
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
        {
            bool resuelto = GameManager.Instance.PuzzleResuelto(puzzleRequeridoID);
            if (rend != null) rend.enabled = resuelto;
            if (col != null) col.enabled = resuelto;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: puzzleRequeridoID está vacío");
        }
    }
}