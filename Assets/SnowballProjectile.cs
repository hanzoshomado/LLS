using UnityEngine;
using Bolt;

public class SnowballProjectile : MonoBehaviour
{
    public int Damage;
    public float ExplosionRadius;
    public GameObject ImpactEffect;

    private SantaCharacterController _owner;
    private int _attackID;
    private bool _hasExploded;

    public void Initialize(SantaCharacterController owner, int attackID, Vector3 velocity)
    {
        _owner = owner;
        _attackID = attackID;
        GetComponent<Rigidbody>().velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!BoltNetwork.isServer) return;
        if (_hasExploded) return; // prevent double-trigger
        _hasExploded = true;

        if (_owner == null)
        {
            BoltNetwork.Destroy(gameObject);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius);
        foreach (Collider hit in hits)
        {
            SantaCharacterController target = hit.GetComponentInParent<SantaCharacterController>();
            if (target != null)
            {
                Vector3 dir = (target.transform.position - transform.position).normalized;
                target.TryTakeDamageFromAttack(_owner, Damage, dir, _attackID, WeaponType.SnowballLauncher, transform.position);
            }
        }

        if (ImpactEffect != null)
        {
            GameObject effect = Instantiate(ImpactEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * ExplosionRadius;
            Destroy(effect, 3f);
        }

        BoltNetwork.Destroy(gameObject);
    }
}