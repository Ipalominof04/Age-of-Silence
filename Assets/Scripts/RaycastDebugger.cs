using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"=== RAYCAST RESULTS ({results.Count}) ===");
            foreach (var r in results)
                Debug.Log($"{r.gameObject.name} | depth: {r.depth} | order: {r.sortingOrder}");
        }
    }
}