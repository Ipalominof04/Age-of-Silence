using UnityEngine;

public class BalaController : MonoBehaviour
{
    private bool yaImpacto = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (yaImpacto) return;
        yaImpacto = true;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(1);

        Destroy(gameObject);
    }
}