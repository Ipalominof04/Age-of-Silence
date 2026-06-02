using UnityEngine;

public class CollisionDebugger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[ENTER] {gameObject.name} chocó con: {collision.gameObject.name} | Tag: {collision.gameObject.tag} | Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log($"[EXIT] {gameObject.name} dejó de tocar: {collision.gameObject.name}");
    }
}