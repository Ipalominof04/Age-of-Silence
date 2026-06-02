using UnityEngine;
public class ObjectoActivable : MonoBehaviour
{
    [SerializeField] private string puzzleRequeridoID; // id puzle necesari pq aquest objecte s'activi
    [SerializeField] private Renderer[] rends;
    [SerializeField] private Collider col;
    [SerializeField] private Light luz;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        // NOTA: Anteriormente se usaba gameObject.SetActive(resuelto) sobre este mismo objeto,
        // lo que causaba un bug: si el objeto empezaba inactivo, Unity nunca ejecutaba este Start(),
        // y si empezaba activo pero el puzzle no estaba resuelto, se desactivaba y no podía
        // reactivarse solo. La solución es controlar Renderer, Collider y Light directamente,
        // así el script siempre se ejecuta independientemente del estado del padre en la jerarquia.
        if (!string.IsNullOrEmpty(puzzleRequeridoID))
        {
            bool resuelto = GameManager.Instance.PuzzleResuelto(puzzleRequeridoID);
            Debug.Log($"{gameObject.name} → puzzle {puzzleRequeridoID} = {resuelto}");
            foreach (Renderer rend in rends)
                if (rend != null) rend.enabled = resuelto;
            if (col != null) col.enabled = resuelto;
            if (luz != null) luz.enabled = resuelto;
            if(canvas != null) canvas.enabled = resuelto;
        }
    }
}