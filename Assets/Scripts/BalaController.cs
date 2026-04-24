using System.Collections;
using UnityEngine;

public class BalaController : MonoBehaviour
{
    private bool yaImpacto = false;
    private float damage;


    private void Start()
    {
        StartCoroutine(DestroyBala());
    }

    public void SetDamage(float amount)
    {
        damage = amount;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (yaImpacto) return;
        yaImpacto = true;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }

    
    IEnumerator DestroyBala()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

}